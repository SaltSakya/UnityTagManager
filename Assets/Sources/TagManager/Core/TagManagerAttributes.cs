using System;

namespace SaltSakya.TagManager 
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class TagManagerAttribute : Attribute
    {
        private readonly Type _type;
        private readonly int _mask;

        public Type ValueType => _type;
        public int Mask => _mask;
        
        public TagManagerAttribute(Type type)
        {
            _type = type;
            if (_type == typeof(byte))
            {
                _mask = 3;
            }
            else if (_type == typeof(ushort))
            {
                _mask = 4;
            }
            else if (_type == typeof(uint))
            {
                _mask = 5;
            }
            else if (_type == typeof(ulong))
            {
                _mask = 6;
            }
            else
            {
                _mask = -1;
            }
        }
    }
}
