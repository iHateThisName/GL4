using System;
using System.Collections.Generic;

namespace Gaskellgames
{
    /// <remarks>
    /// Code created by Gaskellgames: https://gaskellgames.com
    /// </remarks>
    
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns true if the comparisonType is the same as or a subclass of a base class.
        /// </summary>
        /// <param name="comparisonType"></param>
        /// <param name="baseClass"></param>
        /// <returns></returns>
        public static bool IsSameOrSubclass(Type comparisonType, Type baseClass)
        {
            return comparisonType.IsSubclassOf(baseClass) || comparisonType == baseClass;
        }

        /// <summary>
        /// Returns true if the parent type is generic and the child type implements it.
        /// </summary>
        public static bool IsGenericSubclass(Type parent, Type child)
        {
            if (!parent.IsGenericType)
            {
                return false;
            }

            Type currentType = child;
            bool Subclass = false;
            while (!Subclass && currentType != null)
            {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == parent.GetGenericTypeDefinition())
                {
                    Subclass = true;
                    break;
                }
                currentType = currentType.BaseType;
            }
            return Subclass;
        }

        /// <summary>
        /// Returns the Unity specific name of the type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string DisplayName(this Type type)
        {
            // special cases: return unity specific naming for system types
            switch (type.Name)
            {
                case "Boolean":
                    return "Bool";
                    
                case "Single":
                    return "Float";
                    
                case "Int16":
                    return "Short";
                    
                case "UInt16":
                    return "UShort";
                    
                case "Int32":
                    return "Int";
                    
                case "UInt32":
                    return "UInt";
                    
                case "Int64":
                    return "Long";
                    
                case "UInt64":
                    return "ULong";
            }
            
            // otherwise return system type name
            return type.Name;
        }
        
        /// <summary>
        /// Returns a string array containing the Unity specific names of each type within the type array.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static string[] DisplayNames(this Type[] types)
        {
            if (types == null) { return null; }
            
            List<string> argTypesAsString = new List<string>();
            foreach (Type type in types)
            {
                argTypesAsString.Add(type.DisplayName());
            }
            return argTypesAsString.ToArray();
        }
        
        /// <summary>
        /// Returns a single formatted string containing the Unity specific names of each type within the type array.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static string FormattedDisplayNames(this Type[] types)
        {
            if (types == null) { return string.Empty; }

            string[] argTypesAsString = types.DisplayNames();
            string returnString = argTypesAsString[0];
            for (int i = 1; i < argTypesAsString.Length; i++)
            {
                returnString += $", {argTypesAsString[i]}";
            }
            return returnString;
        }

    } // class end
}