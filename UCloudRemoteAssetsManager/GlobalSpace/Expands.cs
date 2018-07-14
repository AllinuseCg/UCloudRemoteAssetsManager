using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UCloudRemoteAssetsManager
{
    static class xmlExpand
    {
        public static XmlNode getChildNode(this XmlNode self, string child)
        {
            XmlNode n = self.SelectSingleNode(child);
            if (n == null)
            {
                n = self.OwnerDocument.CreateElement(child);
                self.AppendChild(n);
            }
            return n;
        }
    }

    static class DictionaryExpand
    {
        public static T getValue<T>(this Dictionary<string,T> self,string key)
        {
            T resault;
            if (self.TryGetValue(key, out resault))
            {
                return resault;
            }
            else
            {
                return default(T);
            }
        }
    }
}
