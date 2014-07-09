using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace SolrNetLight.Utils
{
    internal static class ExtensionsMethods
    {
        /// <summary>
        /// Gets the property attributes.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="dic">The dic.</param>
        /// <returns></returns>
        internal static Dictionary<string, object> GetPropertyAttributes(this PropertyInfo property, Dictionary<string, object> dic)
        {
            // look for attributes that takes one constructor argument
            foreach (var attribData in property.GetCustomAttributes(false))
            {
                if (attribData is DataMemberAttribute)
                {
                    string dataMemberName = ((DataMemberAttribute)attribData).Name;
                    bool isDictionnary = dataMemberName.Contains("_");
                    if (isDictionnary && property.PropertyType.Name == "IDictionary`2")
                    {
                        dic.Add(dataMemberName, property);
                    }
                }

            }
            return dic;
        }
    }
}
