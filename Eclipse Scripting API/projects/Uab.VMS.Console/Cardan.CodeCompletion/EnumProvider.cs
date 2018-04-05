using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cardan.CodeCompletion
{
    public class EnumProvider
    {
        private List<Type> EnumTypes = new List<Type>();
        public EnumProvider()
        {
            

            foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    EnumTypes.AddRange(item.GetTypes().Where(t => t.IsEnum));
                }
                catch (Exception e)
                {
                }
            }
        }


        public bool IsEnum(string enumName, ref List<CompletionData> data)
        {
            var found = EnumTypes.FirstOrDefault(t =>
            {
                var name = t.Name.Split('.').Last();
                return enumName == name;
            });
            if (found != null)
            {
                foreach (var name in found.GetEnumNames())
                {
                    data.Add(new CompletionData(name));
                }
            }
            return found != null;
        }
    }
}
