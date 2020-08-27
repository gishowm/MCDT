using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        //public long GetTime() {
        //    DateTime.Now.Millisecond.ToString()
        //}
    }
}
