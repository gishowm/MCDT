using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MCDT
{
    public class JSON : Dictionary<string, object>
    {
        public static JSON From(NameValueCollection obj)
        {
            JSON json = new JSON();
            for (int i = 0; i < obj.Count; i++)
            {
                json.Add(obj.GetKey(i), obj[obj.GetKey(i)]);
            }
            return json;
        }

        public static JSON Form(JSON obj)
        {
            JSON json = new JSON();
            foreach (var item in obj)
            {
                json[item.Key] = item.Value;
            }
            return json;
        }

    }
}
