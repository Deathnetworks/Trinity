using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Trinity.Helpers
{
    public class ToStringReflector
    {
        public static string GetObjectString(object obj)
        {
            if (obj == null)
                return string.Empty;

            string output = "";
            Type t = obj.GetType();
            List<PropertyInfo> properties;
            foreach (var property in t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var p = t.GetProperty(property.Name);                
                output += property.Name + "=" + ((p != null) ? p.GetValue(obj, null) : "Unknown") + " ";
            }
            foreach (var field in t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var f = t.GetField(field.Name);
                output += field.Name + "=" + ((f != null) ? f.GetValue(obj) : "Unknown") +" ";
            }
            return output;

        }
    }
}
