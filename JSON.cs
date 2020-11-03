using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MCDT
{
    /// <summary>
    /// this base class is Dictionary<string, object>
    /// </summary>
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

        public static JSON Merge(JSON obj)
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
            if (this[key] == null) return "";
            return this[key].ToString();
        }
        public int Int(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            if (this[key] == null) return 0;
            return Convert.ToInt32(this[key]);

        }
        public decimal Decimal(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            if (this[key] == null) return 0.00M;
            return Convert.ToDecimal(this[key]);
        }

        public double Double(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            if (this[key] == null) return 0.00D;
            return Convert.ToDouble(this[key]);
        }
        public bool Boolean(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            if (this[key] == null) return false;
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

        public JArray List(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            if (this[key] == null) return null;
            return this[key];
        }

        public List<dynamic> List<T>(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            if (this[key] == null) return null;

            var type = typeof(T);
            List<dynamic> list = new List<dynamic>();
            switch (type.Name)
            {
                case "Int":
                    foreach (var item in this[key])
                    {
                        list.Add((T)item.Value.Value);
                    }
                    break;
                case "String":
                    foreach (var item in this[key])
                    {
                        list.Add(item.Value.ToString());
                    }
                    break;
                case "JSON":
                    foreach (var item in this[key])
                    {
                        JSON reJSON = new JSON();
                        foreach (var name in item)
                        {
                            reJSON.Add(name.Name, name.Value);
                        }
                        list.Add(reJSON);
                    }
                    break;
            }
            return list;
        }
        public dynamic Json(string key)
        {
            if (!this.Keys.Contains(key)) throw new Exception("Key:\"" + key + "\" Is Not Found");
            if (this[key] == null) return null;
            JSON reJSON = new JSON();
            foreach (var item in this[key])
            {
                reJSON.Add(item.Name, item.Value);
            }
            return reJSON;
        }

    }
}
