using System;
using System.Collections.Generic;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Network_Test.Fody.Extensions;

namespace Network_Test.Fody.Core
{
    public static class Processor
    {
        public enum CallType
        {
            Server,
            Client,
            Multi
        }
        public struct RpcInvoker
        {
            public MethodDefinition Method;
            public CallType CallType;

            public RpcInvoker(MethodDefinition method, CallType callType)
            {
                Method = method;
                CallType = callType;
            }
        }
        static Dictionary<ushort, RpcInvoker> _rpcInvokers = new Dictionary<ushort, RpcInvoker>();
        
        //functions we use everywhere
        private static MethodReference _dispose;
        private static MethodReference _networkManagerInstanceGetter;
        private static MethodReference _netManagerSendToServer;
        private static MethodReference _netManagerSendToClient;
        private static MethodReference _netManagerSendToAllClients;
        private static MethodReference _writerGetData;

        public static void Initialize(ModuleDefinition module)
        {
            _dispose = module.ImportReference(typeof(IDisposable).GetMethod("Dispose"));
            _networkManagerInstanceGetter = module.ImportReference(ModuleWeaver.NetworkManagerType.GetMethod("get_Instance"));
            _netManagerSendToServer = module.ImportReference(ModuleWeaver.NetworkManagerType.GetMethod("SendToServer"));
            _netManagerSendToClient = module.ImportReference(ModuleWeaver.NetworkManagerType.GetMethod("SendToClient"));
            _netManagerSendToAllClients = module.ImportReference(ModuleWeaver.NetworkManagerType.GetMethod("SendToAllClients"));
            _writerGetData = module.ImportReference(ModuleWeaver.NetworkWriterType.GetMethod("GetData"));
        }
        public static void ProcessMethod(MethodDefinition md)
        {
            var attributes = md.CustomAttributes;
            foreach (var customAttribute in attributes)
            {
                if (customAttribute.IsAttribute(ModuleWeaver.ServerRpcAttr))
                {
                    CheckValidRpc(md, false);
                    ProcessServerRpc(md, customAttribute);
                }
                if (customAttribute.IsAttribute(ModuleWeaver.ClientRpcAttr))
                {
                    CheckValidRpc(md, true);
                    ProcessClientRpc(md, customAttribute);
                }
                if (customAttribute.IsAttribute(ModuleWeaver.MultiRpcAttr))
                {
                    CheckValidRpc(md, false);
                    ProcessMultiRpc(md, customAttribute);
                }
            }
           
        }

        static void CheckValidRpc(MethodDefinition md, bool isClient)
        {
            if (!md.IsPublic || !md.IsStatic || !md.ReturnType.EqualsTo(typeof(void)))
            {
                throw new WeavingException("rpc must be public static void!");
            }

            if (isClient && (md.Parameters.Count == 0 || md.Parameters[0].ParameterType != ModuleWeaver.ClientNetworkConnectionType))
            {
                throw new WeavingException($"Client rpc [{md.FullName}] must have first parameter be of type [ClientNetworkConnection] that must not be used inside the method.");
            }
        }


        static void ProcessServerRpc(MethodDefinition md, CustomAttribute attribute)
        {
            ushort hash = md.FullName.GetStableHashCode();
            if (_rpcInvokers.TryGetValue(hash, out var invoker))
            {
                throw new WeavingException($"rpc hash from [{md.FullName}] collided with another hash! rename it or collided [{invoker.Method.FullName}]");
            }
            
            MethodDefinition userCode = GenerateRpcWrite(md, attribute, hash, CallType.Server);
            MethodDefinition invokeUserCode = GenerateRpcInvoke(md, userCode, CallType.Server);
            
            _rpcInvokers.Add(hash, new RpcInvoker(invokeUserCode, CallType.Server));
        }
        
        static void ProcessClientRpc(MethodDefinition md, CustomAttribute attribute)
        {
            ushort hash = md.FullName.GetStableHashCode();
            if (_rpcInvokers.TryGetValue(hash, out var invoker))
            {
                throw new WeavingException($"rpc hash from [{md.FullName}] collided with another hash! rename it or collided [{invoker.Method.FullName}]");
            }
            
            MethodDefinition userCode = GenerateRpcWrite(md, attribute, hash, CallType.Client);
            MethodDefinition invokeUserCode = GenerateRpcInvoke(md, userCode, CallType.Client);
            
            _rpcInvokers.Add(hash, new RpcInvoker(invokeUserCode, CallType.Client));
        }
        
        static void ProcessMultiRpc(MethodDefinition md, CustomAttribute attribute)
        {
            ushort hash = md.FullName.GetStableHashCode();
            if (_rpcInvokers.TryGetValue(hash, out var invoker))
            {
                throw new WeavingException($"rpc hash from [{md.FullName}] collided with another hash! rename it or collided [{invoker.Method.FullName}]");
            }
            
            MethodDefinition userCode = GenerateRpcWrite(md, attribute, hash, CallType.Multi);
            MethodDefinition invokeUserCode = GenerateRpcInvoke(md, userCode, CallType.Multi);
            
            _rpcInvokers.Add(hash, new RpcInvoker(invokeUserCode, CallType.Multi));
        }

        static MethodDefinition GenerateRpcWrite(MethodDefinition md, CustomAttribute attribute, ushort hash, CallType callType)
        {
            MethodDefinition userRpc = new MethodDefinition($"UserRpc_{md.Name}", md.Attributes, md.ReturnType);
            userRpc.IsPublic = false;
            userRpc.IsFamily = true;
            userRpc.IsStatic = true;
            
            foreach (var parameterDefinition in md.Parameters)
            {
                userRpc.Parameters.Add(parameterDefinition);
            }
            
            (userRpc.Body, md.Body) = (md.Body, userRpc.Body);
            
            // thanks mirror!
            // Move over all the debugging information
            foreach (SequencePoint sequencePoint in md.DebugInformation.SequencePoints)
                userRpc.DebugInformation.SequencePoints.Add(sequencePoint);
            md.DebugInformation.SequencePoints.Clear();

            foreach (CustomDebugInformation customInfo in md.CustomDebugInformations)
                userRpc.CustomDebugInformations.Add(customInfo);
            md.CustomDebugInformations.Clear();

            (md.DebugInformation.Scope, userRpc.DebugInformation.Scope) = (userRpc.DebugInformation.Scope, md.DebugInformation.Scope);

            md.DeclaringType.Methods.Add(userRpc);

            md.Body.InitLocals = true;
            ILProcessor il = md.Body.GetILProcessor();
            VariableDefinition netWriterVar = GenerateMakeNetworkWriter(il, md);
            
            //for a using(networkWriter writer) { } since its a IDisposable
            //I hate this
            var tryStart = il.Create(OpCodes.Nop);
            var finallyStart = il.Create(OpCodes.Ldloc, netWriterVar);
            var finallyEnd = il.Create(OpCodes.Nop);
            var preEndFinally = il.Create(OpCodes.Nop);
            var ret = il.Create(OpCodes.Ret);
            
            
            il.Emit(OpCodes.Call, _networkManagerInstanceGetter);
            il.Emit(OpCodes.Brfalse, ret);
            
            //body of using()
            il.Append(tryStart);
            
            WriteHeader(il, netWriterVar, hash);
            WriteArguments(il, netWriterVar, md, callType == CallType.Client);
            
            il.Emit(OpCodes.Call, _networkManagerInstanceGetter); //networkManager instance loaded
            
            il.Emit(OpCodes.Ldloc, netWriterVar); //loads writer
            
            if (callType == CallType.Client)
            {
                il.Emit(OpCodes.Ldarg_0); //ClientConnection
            }
            
            il.Emit(OpCodes.Call, _writerGetData); //calls get data from writer
            
            
            il.Emit(OpCodes.Ldc_I4, attribute.GetSendType());
            
            if (callType == CallType.Server)
            {
                il.Emit(OpCodes.Callvirt, _netManagerSendToServer); //call SendToServer on networkManager
            }
            else if (callType == CallType.Client)
            {
                il.Emit(OpCodes.Callvirt, _netManagerSendToClient); //call SendToServer on networkManager
            }
            else
            {
                il.Emit(OpCodes.Callvirt, _netManagerSendToAllClients); //call SendToServer on networkManager
            }
            
            il.Emit(OpCodes.Leave, ret);
            //il.Append(finallyStart);
            
            //disposing
            il.Append(finallyStart);
            //il.Emit(OpCodes.Ldloc, netWriterVar);
            il.Emit(OpCodes.Brfalse, preEndFinally);
            il.Emit(OpCodes.Ldloc, netWriterVar);
            il.Emit(OpCodes.Callvirt, md.Module.ImportReference(_dispose));
            il.Append(preEndFinally);
            il.Emit(OpCodes.Endfinally);
            il.Append(finallyEnd);

            var tryFinally = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = tryStart,
                TryEnd = finallyStart,
                HandlerStart = finallyStart,
                HandlerEnd = finallyEnd
            };
            
            
            il.Append(ret);
            md.Body.ExceptionHandlers.Add(tryFinally);
            
            return userRpc;
        }

        static MethodDefinition GenerateRpcInvoke(MethodDefinition md, MethodDefinition userRpc, CallType callType)
        {
            MethodDefinition invokeMethod = new MethodDefinition($"InvokeUserCode_{md.Name}", 
                MethodAttributes.Family | 
                MethodAttributes.Static |
                MethodAttributes.Public | 
                MethodAttributes.HideBySig,
                ModuleWeaver.TypeSystem.Void);
            
            invokeMethod.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, md.Module.ImportReference(ModuleWeaver.NetworkReaderType)));
            
            ILProcessor il = invokeMethod.Body.GetILProcessor();
            
            ReadArguments(il, md, callType == CallType.Client);
            
            il.Emit(OpCodes.Call, userRpc);
            il.Emit(OpCodes.Ret);
            
            md.DeclaringType.Methods.Add(invokeMethod);

            return invokeMethod;
        }
        static VariableDefinition GenerateMakeNetworkWriter(ILProcessor ilProcessor, MethodDefinition md)
        {
            var netWriterRef = md.Module.ImportReference(ModuleWeaver.NetworkWriterType);
            VariableDefinition variableDefinition = new VariableDefinition(netWriterRef);
            md.Body.Variables.Add(variableDefinition);
            
            ilProcessor.Emit(OpCodes.Newobj, md.Module.ImportReference(netWriterRef.GetConstructor()));
            ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
            
            return variableDefinition;
        }

        static void WriteHeader(ILProcessor il, VariableDefinition netWriterVar, ushort hash)
        {
            il.Emit(OpCodes.Ldloc, netWriterVar);
            il.Emit(OpCodes.Ldc_I4, hash);
            
            var writerFunc = ReadersWriters.GetWriterMethod(ModuleWeaver.TypeSystem.UInt16);
            
            il.Emit(OpCodes.Call, writerFunc);
        }

        static void WriteArguments(ILProcessor ilProcessor, VariableDefinition netWriterVar,
            MethodDefinition md, bool isClientRpc)
        {
            int argCount = 0;
            foreach (var parameterDefinition in md.Parameters)
            {
                if (argCount == 0 && isClientRpc)
                {
                    argCount++;
                    continue;
                }

                var writerFunc = ReadersWriters.GetWriterMethod(parameterDefinition.ParameterType);
                
                ilProcessor.Emit(OpCodes.Ldloc, netWriterVar); //load this networkWriter for extension
                ilProcessor.Emit(OpCodes.Ldarg, argCount); //load function arg from index
                ilProcessor.Emit(OpCodes.Call, writerFunc);
                
                argCount++;
            }
        }

        static void ReadArguments(ILProcessor ilProcessor,
            MethodDefinition md, bool isClientRpc)
        {
            foreach (var parameterDefinition in md.Parameters)
            {
                if (isClientRpc) //skip first param
                {
                    ilProcessor.Emit(OpCodes.Ldnull);
                    isClientRpc = false;
                    continue;
                }
                
                var readerMethod = ReadersWriters.GetReaderMethod(parameterDefinition.ParameterType);
                
                ilProcessor.Emit(OpCodes.Ldarg_0); //load this networkReader for extension
                ilProcessor.Emit(OpCodes.Call, readerMethod);
            }
        }
        
        public static void WriteRpcsToConstructor()
        {
            TypeDefinition registerFuncsType =
                new TypeDefinition("Network_Test", "RpcFuncRegistersGenerated", 
                    TypeAttributes.Class |
                    TypeAttributes.Abstract |
                    TypeAttributes.Sealed, ModuleWeaver.TypeSystem.Object);
            
            MethodDefinition registerAll = new MethodDefinition(".cctor", 
                MethodAttributes.Private | 
                MethodAttributes.Static |
                MethodAttributes.HideBySig | 
                MethodAttributes.SpecialName | 
                MethodAttributes.RTSpecialName, ModuleWeaver.TypeSystem.Void);
            
            registerFuncsType.Methods.Add(registerAll);
            ModuleWeaver.Module.Types.Add(registerFuncsType);
            
            
            MethodReference delegateCtor = ModuleWeaver.Module.ImportReference(ModuleWeaver.RpcDelegateType.GetMethod(".ctor"));
            MethodReference registerFunc = ModuleWeaver.Module.ImportReference(ModuleWeaver.RpcHandlerType.GetMethod("RegisterRpc")); //ushort, delegate, callType
            
            registerAll.Body.Instructions.Clear();
           
            ILProcessor il = registerAll.Body.GetILProcessor();
            
            foreach (var invoker in _rpcInvokers)
            {
                il.Emit(OpCodes.Ldc_I4, invoker.Key);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ldftn, invoker.Value.Method);
                il.Emit(OpCodes.Newobj, delegateCtor);
                il.Emit(OpCodes.Ldc_I4, (int)invoker.Value.CallType);
                il.Emit(OpCodes.Call, registerFunc);
            }
            il.Emit(OpCodes.Ret);
        }
    }
    
}