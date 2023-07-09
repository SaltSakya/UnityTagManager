using System;
using System.Reflection;
using UnityEngine;


namespace SaltSakya.TagManager
{
    public static class TagManager
    {
        public enum TagRelation
        {
            None,
            Parents,
            Same,
            Child
        }
        public static bool CheckSame<T>(T aEnum, T bEnum) where T: Enum
        {
            typeof(T).Attributes.GetTypeCode();
            var a = typeof(T).GetCustomAttribute<TagManagerAttribute>();
            Debug.Log(a);
            switch (aEnum.GetTypeCode())
            {
                case TypeCode.Byte:
                    return (byte)(object)aEnum == (byte)(object)bEnum; 
                    //break;
                case TypeCode.UInt16:
                    return (ushort)(object)aEnum == (ushort)(object)bEnum;
                    //break;
                case TypeCode.UInt32:
                    return (uint)(object)aEnum == (uint)(object)bEnum;
                    //break;
                case TypeCode.UInt64:
                    return (ulong)(object)aEnum == (ulong)(object)bEnum;
                    //break;
                default:
                    Debug.Log("Error");
                    break;
            }
            return false;
        }

        public static bool CheckRelation<T>(T a, T b) where T: Enum
        {
            /*if (!a.GetType().IsEnum)
                Debug.Log($"Error");
            switch (a.GetTypeCode())
            {
                case TypeCode.Byte:
                    return 
            }*/
            return false;
        }
    }
}
