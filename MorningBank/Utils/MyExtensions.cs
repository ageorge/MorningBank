using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace MorningBank.Utils
{
    public static class MyExtensions
    {
        public static string LosSerialize(this object obj)
        {
            var sw = new StringWriter();
            var formatter = new LosFormatter();
            formatter.Serialize(sw,obj);
            return sw.ToString();
        }

        public static object LosDeserialize(this string locEncData)
        {
            if(String.IsNullOrEmpty(locEncData))
            {
                return null;
            }
            var formatter = new LosFormatter();
            return formatter.Deserialize(locEncData);
        }
    }
}