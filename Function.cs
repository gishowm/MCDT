using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ThoughtWorks.QRCode.Codec;

namespace MCDT
{
    public static class Function
    {
        private static void Test()
        {

        }

        #region 时间扩展
        public static string ToDateTimeString(this DateTime dateTime)
        {
            if (dateTime == null) return "";
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static string ToDateString(this DateTime dateTime)
        {
            if (dateTime == null) return "";
            return dateTime.ToString("yyyy-MM-dd");
        }
        #endregion

        #region String扩展
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="formate">X2|x2</param>
        /// <returns></returns>
        public static string ToMD5(this string str, string formate = "X2")
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            byte[] byteArray = Encoding.ASCII.GetBytes(str);

            byteArray = md5.ComputeHash(byteArray);

            string hashedValue = "";

            foreach (byte b in byteArray)
            {
                hashedValue += b.ToString(formate);
            }

            return hashedValue;
        }

        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="formate">X2|x2</param>
        /// <returns></returns>
        public static string ToSHA256(this string str, string formate = "X2")
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] hash = SHA256Managed.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString(formate));
            }
            return builder.ToString();
        }

        /// <summary>
        /// 下载网络图片ToBase64
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ImgUrlToBase64(this string url)
        {
            using (WebClient mywebclient = new WebClient())
            {
                byte[] Bytes = mywebclient.DownloadData(url);
                using (MemoryStream ms = new MemoryStream(Bytes))
                {
                    Image img = Image.FromStream(ms);

                    Bitmap bmp = new Bitmap(img);
                    MemoryStream ms2 = new MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] arr = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(arr, 0, (int)ms.Length);
                    ms.Close();
                    var base64 = Convert.ToBase64String(arr);
                    return "data:image/png;base64," + base64;
                }
            }
        }



        /// <summary>
        ///
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static string ToUTF8Base64Str(this string Str)
        {
            byte[] b = System.Text.Encoding.UTF8.GetBytes(Str);
            return Convert.ToBase64String(b);

        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static string FromUTF8Base64Str(this string Str)
        {
            byte[] b = Convert.FromBase64String(Str);
            return System.Text.Encoding.UTF8.GetString(b);
        }

        #endregion

        public static string webRequest(string url, string postString, Encoding encod, JSON headers = null, string method = "POST")
        {
            //创建WebClient 对象
            using (WebClient web = new WebClient())
            {
                //需要上传的数据
                //以form表单的形式上传
                web.Headers.Add("Content-Type", "application/json;");
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        web.Headers.Add(item.Key.ToString(), item.Value.ToString());
                    }
                }
                // 转化成二进制数组
                byte[] postData = encod.GetBytes(postString);
                // 上传数据
                byte[] responseData = web.UploadData(url, method, postData);
                //Console.Write("服务器路径：" + pathQuery);
                //获取返回的二进制数据.
                string huifu = encod.GetString(responseData);
                return huifu;
            }
        }

        public static void DelCookie(String Key)
        {
            System.Web.HttpCookie myCookie = new System.Web.HttpCookie(Key);
            myCookie.Expires = DateTime.Now.AddDays(-7);
            myCookie.Value = "";
            System.Web.HttpContext.Current.Response.Cookies.Add(myCookie);
        }
        //获取 
        public static string GetCookie(string Key)
        {
            if (System.Web.HttpContext.Current.Request.Cookies[Key] != null)
            {
                return System.Web.HttpContext.Current.Request.Cookies[Key].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 类属性去String空格
        /// </summary>
        /// <typeparam name="T">泛型类</typeparam>
        /// <param name="model">泛型实例</param>
        /// <returns></returns>
        public static T PropertyTrim<T>(T model) where T : class, new()
        {
            T t = new T();

            Type type = model.GetType();
            foreach (var item in type.GetProperties())
            {
                if (item.PropertyType.Name == "String")
                {
                    if (item.GetValue(model) != null)
                        item.SetValue(t, item.GetValue(model).ToString().Trim());
                }
                else
                {
                    item.SetValue(t, item.GetValue(model));
                }
            }
            return t;
        }


        #region Request.From扩展
        public static T ToModel<T>(this NameValueCollection form) where T : class, new()
        {
            T objmodel = new T();
            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                string name = info.Name;

                if (form.GetValues(name) != null)
                {
                    //如果不是泛型
                    if (!info.PropertyType.IsGenericType)
                    {
                        //如果是空则设置空，非空则设置值。
                        info.SetValue(objmodel, string.IsNullOrEmpty(form.GetValues(name).ToString()) ? null : Convert.ChangeType(form.GetValues(name), info.PropertyType), null);
                    }
                    //如果是泛型，则找他的基础类型
                    else if (info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        info.SetValue(objmodel, string.IsNullOrEmpty(form.GetValues(name).ToString()) ? null : Convert.ChangeType(form.GetValues(name), Nullable.GetUnderlyingType(info.PropertyType)), null);
                    }
                }
            }
            return objmodel;
        }
        #endregion

        /// <summary>
        /// toJSON
        /// </summary>
        /// <param name="form"></param>
        /// <param name="ignoreNull">是否排除空值，默认排除</param>
        /// <returns></returns>
        public static JSON ToJSON(this NameValueCollection form, bool ignoreNull = true)
        {
            JSON json = new JSON();
            foreach (string item in form.Keys)
            {
                string name = item;
                if (name.IndexOf("[]") > -1)
                    name = name.Replace("[]", "");
                if (ignoreNull)
                    if (string.IsNullOrEmpty(form[item])) continue;
                json[name] = form[item];
            }
            return json;
        }

        /// <summary>
        /// toJSON
        /// </summary>
        /// <param name="form"></param>
        /// <param name="Ignore">此数组内的键如果是空值将不会被过滤</param>
        /// <returns></returns>
        public static JSON ToJSON(this NameValueCollection form, params string[] Ignore)
        {
            JSON json = new JSON();
            foreach (string item in form.Keys)
            {
                if (!Ignore.Contains(item))
                    if (string.IsNullOrEmpty(form[item])) continue;
                json[item] = form[item];
            }
            return json;
        }



        /// <summary>
        /// 根据时间获取时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetTimeStamp(DateTime dateTime)
        {
            TimeSpan ts = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        /// <summary>
        /// string转为Json对象
        /// </summary>
        /// <param name="jstr"></param>
        /// <returns></returns>
        public static JSON toJSON(this string jstr)
        {
            try
            {
                return JsonConvert.DeserializeObject<JSON>(jstr);
            }
            catch (Exception ex)
            {
                throw new Exception("转换 JSON 失败:" + ex.Message);
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jstr"></param>
        /// <param name="Istrim">是否去头掐尾</param>
        /// <returns></returns>
        public static List<string> ToList(this string jstr, bool Istrim = true)
        {
            try
            {
                if (Istrim)
                    jstr = jstr.TrimEnd(']').TrimStart('[');
                return jstr.Split(',').ToList<string>();
            }
            catch (Exception ex)
            {
                throw new Exception("转换 JSON 失败:" + ex.Message);
            }
        }

         /// <summary>
        /// 根据概率返回索引
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static int ProbabilityRandomRumber(decimal[] rate)
        {
            int total = 0;
            for (int i = 0; i < rate.Length; i++)
            {
                total += Convert.ToInt32(rate[i] * 1000);
            }
            Random myRandom = new Random();
            int r = myRandom.Next(0, total);
            int t = 0;
            for (int i = 0; i < rate.Length; i++)
            {
                t += Convert.ToInt32(rate[i] * 1000);
                if (r < t)
                    return i;
            }
            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="jstr"></param>
        /// <param name="Istrim">是否去头掐尾</param>
        /// <returns></returns>
        public static List<JSON> ToJsonList(this string jstr, bool Istrim = true)
        {
            List<JSON> list = new List<JSON>();
            try
            {
                var collection = JsonConvert.DeserializeObject<JToken>(jstr);
                foreach (var item in collection)
                {
                    JSON json = new JSON();
                    foreach (Newtonsoft.Json.Linq.JProperty name in item)
                    {
                        json.Add(name.Name, name.Value.ToString());
                    }
                    list.Add(json);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("转换 JSON 失败:" + ex.Message);
            }
        }


        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        public static long TimeStamp(this DateTime dateTime)
        {
            TimeSpan ts = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>  
        /// 时间戳Timestamp转换成日期  
        /// </summary>  
        /// <param name="timeStamp"></param>  
        /// <returns></returns>  
        public static DateTime GetDateTime(long timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = ((long)timeStamp * 10000000);
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime targetDt = dtStart.Add(toNow);
            return targetDt;
        }

        public static string QRcode(string url, bool retbase64 = false)
        {

            Bitmap bt;
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeScale = 8;
            qrCodeEncoder.QRCodeForegroundColor = Color.Black;
            qrCodeEncoder.QRCodeBackgroundColor = Color.White;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            //qrCodeEncoder.setStructureappend(400, 400, 5);

            string strbaser64;
            bt = qrCodeEncoder.Encode(url, Encoding.UTF8);


            Bitmap bitmap = new Bitmap(bt.Width + 20, bt.Height + 20);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            g.DrawRectangle(new Pen(Color.White), new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            g.DrawImage(bt, new PointF(10, 10));


            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                strbaser64 = Convert.ToBase64String(ms.GetBuffer());
            }
            if (retbase64 == false)
            {
                return "data:image/jpg;base64," + strbaser64;
            }
            else
            {
                return strbaser64;
            }

        }


    }
}
