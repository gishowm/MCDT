using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCDT
{
    /// <summary>
    /// JWT操作帮助类
    /// </summary>
    public sealed class JwtHelper
    {
        /// <summary>
        /// 签发Token
        /// </summary>
        /// <param name="playload">载荷</param>
        /// <returns></returns>
        public static string GetToken(JwtSup playload)
        {

            string token = String.Empty;
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            if (playload.Exp == 0)
                //设置过期时间
                playload.Exp = Function.GetTimeStamp(DateTime.Now.AddHours(2));
            //获取私钥
            string secret = GetSecret();

            token = encoder.Encode(playload, secret);


            return token;
        }


        /// <summary>
        /// Token校验
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static JwtSup CheckToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return null;

                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);

                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, urlEncoder);

                //获取私钥
                string secret = GetSecret();
                JwtSup playloadInfo = decoder.DecodeToObject<JwtSup>(token, secret, true);
                if (playloadInfo == null) throw new Exception("Token 验证不通过!");
                DateTime exp = Function.GetDateTime(playloadInfo.Exp);
                if (DateTime.Now > exp)
                    throw new Exception("Token 已过期！");
                return playloadInfo;
            }
            catch (Exception ex)
            {
                throw ex;
                //return null;
            }
        }
        /// <summary>
        /// 获取私钥   appsetting>TokenPrivateKey
        /// </summary>
        /// <returns></returns>
        private static string GetSecret()
        {
            var appPrivateKeys = System.Configuration.ConfigurationManager.AppSettings["TokenPrivateKey"];
            if (string.IsNullOrEmpty(appPrivateKeys))
                //TODO 从文件中去读真正的私钥
                return "eyJpc3MiOiJCZXJyeS5TZXJ2aWNlIiwic3ViIjoiMTgyODQ1OTQ2MTkiLCJhdWQiOiJndWVzdCIsImlhdCI6IjE1MzEzODE5OTgiLCJleHAiOiIxNTMxMzg5MTk4IiwibmJmIjowLCJqdGkiOiI1YzdmN2ZhM2E4ODVlODExYTEzNTQ4ZDIyNGMwMWQwNSIsInVzZXJpZCI6bnVsbCwiZXh0ZW5kIjpudWxsfQ";
            return appPrivateKeys;
        }


    }

}