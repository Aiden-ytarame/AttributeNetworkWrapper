using System;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace AttributeNetworkWrapper.Fody
{
    public static class Extensions
    {
        //fnv1a hashing folded to 16bits
        public static ushort GetStableHashCode(this string text)
        {
            unchecked
            {
                uint hash = 0x811c9dc5;
                uint prime = 0x1000193;

                foreach (var t in text)
                {
                    byte value = (byte)t;
                    hash ^= value;
                    hash *= prime;
                }
                
                return (ushort)((hash >> 16) ^ hash);
            }
        }
        
        public static CustomAttribute GetAttribute<T>( this Collection<CustomAttribute> attributes)
        {
            return attributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(T).FullName);
        }
        public static bool HasAttribute<T>( this Collection<CustomAttribute> attributes)
        {
            return attributes.GetAttribute<T>() != null;
        }

        public static bool IsAttribute(this CustomAttribute attribute, TypeDefinition type)
        {
            return attribute.AttributeType.FullName == type.FullName;
        }
        
        public static int GetSendType(this CustomAttribute attribute)
        {
            foreach (var customAttributeNamedArgument in attribute.Fields)
            {
                if (customAttributeNamedArgument.Name == "sendType")
                {
                    return (int)customAttributeNamedArgument.Argument.Value;
                }
            }
            return 0;
        }
        
        public static bool EqualsTo(this TypeReference typeRef, TypeDefinition type)
        {
            return typeRef.FullName == type.FullName;
        }
        
        public static bool EqualsTo(this TypeReference typeRef, Type type)
        {
            return typeRef.FullName == type.FullName;
        }

        public static MethodReference GetConstructor(this TypeReference typeRef)
        {
            foreach (var methodDefinition in typeRef.Resolve().Methods)
            {
                if (methodDefinition.Name == ".ctor" &&
                    methodDefinition.IsPublic &&
                    methodDefinition.Parameters.Count == 0)
                {
                    return methodDefinition;
                }
            }
            return null;
        }

        public static MethodDefinition GetMethod(this TypeDefinition typeDef, string methodName)
        {
            return typeDef.Methods.FirstOrDefault(x => x.Name == methodName);
        }
    }
}