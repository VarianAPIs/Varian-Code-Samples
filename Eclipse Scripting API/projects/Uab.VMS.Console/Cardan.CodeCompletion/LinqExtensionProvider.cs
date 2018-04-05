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
    public static class LinqExtensionProvider
    {
        public static bool IsIEnumerable(object o)
        {
            var ty = o.GetType();
            foreach (Type t in ty.GetInterfaces())
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLinqType(Type ty, out Type[] innerTypes)
        {
            foreach (Type t in ty.GetInterfaces())
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof (IEnumerable<>))
                {
                    innerTypes = t.GenericTypeArguments;
                    return true;
                }
            }
            innerTypes = new Type[0];
            return false;
        }

        public static MethodInfo[] GetExtensions(Type t)
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

            var methods = typeof(System.Linq.Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.IsGenericMethod);
            return methods.ToArray<MethodInfo>();
        }
    }
}
