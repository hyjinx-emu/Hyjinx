using System;

namespace Hyjinx.HLE.HOS.Services
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class ServiceAttribute : Attribute
    {
        public readonly string Name;
        public readonly object Parameter;

        public ServiceAttribute(string name, object parameter = null)
        {
            Name = name;
            Parameter = parameter;
        }
    }
}