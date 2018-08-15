using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    internal static class TypeExtension
    {
        /// <summary>
        /// Get the specified attribute from the enumerator field
        /// </summary>
        /// <typeparam name="TAtt">Attribute type</typeparam>
        /// <param name="type">Type</param>
        /// <returns>Attribute</returns>
        /// <exception cref="NullReferenceException"/>
        /// <remarks>This method throws an exception if attribute is missing</remarks>
        internal static TAtt GetAttribute<TAtt>(this Type type)
            where TAtt : Attribute
        {
            var atts = type.GetCustomAttributes(typeof(TAtt), false);

            if (atts != null && atts.Any())
            {
                return atts.First() as TAtt;
            }
            else
            {
                throw new NullReferenceException($"Attribute of type {typeof(TAtt)} is not found on {type.FullName}");
            }
        }

        internal static bool IsAssignableToGenericType(this Type thisType, Type genericType)
        {
            return thisType.TryFindGenericType(genericType) != null;
        }

        internal static Type[] GetArgumentsOfGenericType(this Type thisType, Type genericType)
        {
            var type = thisType.TryFindGenericType(genericType);

            if (type != null)
            {
                return type.GetGenericArguments();
            }
            else
            {
                return Type.EmptyTypes;
            }
        }

        internal static Type TryFindGenericType(this Type thisType, Type genericType)
        {
            var interfaceTypes = thisType.GetInterfaces();

            Predicate<Type> canCastFunc = (t) => t.IsGenericType && t.GetGenericTypeDefinition() == genericType;

            foreach (var it in interfaceTypes)
            {
                if (canCastFunc(it))
                {
                    return it;
                }
            }

            if (canCastFunc(thisType))
            {
                return thisType;
            }

            var baseType = thisType.BaseType;

            if (baseType != null)
            {
                return baseType.TryFindGenericType(genericType);
            }

            return null;
        }
    }
}