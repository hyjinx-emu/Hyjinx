using System;

namespace Hyjinx.HLE.HOS.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class CommandCmifAttribute : Attribute
    {
        public readonly int Id;

        public CommandCmifAttribute(int id) => Id = id;
    }
}