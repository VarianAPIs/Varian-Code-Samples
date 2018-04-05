using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cardan.CodeCompletion
{
    public class StaticProvider
    {
        private List<Type> Types = new List<Type>();

        public StaticProvider()
        {

            foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Types.AddRange(item.GetTypes().Where(t => t.IsClass));
                }
                catch (Exception e)
                {
                }
            }
        }

        public bool IsClassName(string last, ref List<CompletionData> data)
        {
            var found = Types.Where(t =>
            {
                var name = t.Name.Split('.').Last();
                return last == name;
            });
            if (found != null)
            {
                foreach (var f in found)
                {
                    foreach (var name in f.GetMembers(BindingFlags.Public | BindingFlags.Static).Where(p => char.IsUpper(p.Name[0])))
                    {
                        data.Add(new CompletionData(name.Name));
                    }
                }
            }
            return found != null;
        }
    }
}
