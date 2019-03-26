using Nuke.Common;
using System;
using System.Reflection;

namespace Nuke.Useful
{
    public class AzureVariableAttribute : ParameterAttribute
    {
        private readonly string _name;

        public AzureVariableAttribute(string variableName = null) => _name = variableName;

        public override object GetValue(MemberInfo member, object instance)
        {
            var name = _name ?? member.Name;
            var value = Environment.GetEnvironmentVariable(name?.ToUpper());

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Azure variable is not defined: " + name);
            }

            return value;
        }
    }
}
