using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cardan.CodeCompletion
{
    public static class ExtensionProvider
    {
        public static MethodInfo[] GetExtensionMethods(this Type t)
        {
            List<Type> AssTypes = new List<Type>();

            foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    AssTypes.AddRange(item.GetTypes());
                }
                catch (Exception e)
                {
                    Debug.Write(item);
                }
            }

            var methods = AssTypes
                .Where(ty => ty.IsSealed && !t.IsNested)
                .SelectMany(ty => ty.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(m => m.IsDefined(typeof (ExtensionAttribute), false))
                .Where(m => m.GetParameters()[0].ParameterType == t)
                .ToArray();

            return methods.ToArray<MethodInfo>();
        }
    }
}
