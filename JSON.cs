using Newtonsoft.Json;
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

        public dynamic this[string key]
        {
            get
            {
                if (base.ContainsKey(key))
                    return base[key];
                else
                {
                    return null;
                }
            }
            set
            {
                if (base.ContainsKey(key))
                    base[key] = value;
                else
                    this.Add(key, value);
            }
        }

        public static JSON From(NameValueCollection obj)
        {
            JSON json = new JSON();
            for (int i = 0; i < obj.Count; i++)
            {
                json.Add(obj.GetKey(i), obj[obj.GetKey(i)]);
            }
            return json;
        }

        /// <summary>
        /// 只保留数组中的键
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public JSON Only(params string[] parms)
        {
            var keys = this.Keys.Where(s => !parms.Contains(s));
            List<string> kys = new List<string>();
            foreach (var item in keys)
            {
                kys.Add(item);
            }
            foreach (var item in kys)
            {
                this.Remove(item);
            }
            GC.Collect();
            return this;
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

        public string String(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            return this[key].ToString();
        }
        public int Int(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            return Convert.ToInt32(this[key]);

        }
        public decimal Decimal(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            return Convert.ToDecimal(this[key]);
        }

        public double Double(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            return Convert.ToDouble(this[key]);
        }
        public bool Boolean(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            return Convert.ToBoolean(this[key]);
        }

        public string ToString()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch (Exception ex)
            {
                throw new Exception("转换JsonString失败:" + ex.Message);
            }
        }



    }
}
