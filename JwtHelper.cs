using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System.Collections.Generic;
using System.Configuration;

namespace MCDT
{
    public class JwtHelp<T>
    {

        public JwtHelp(string secret)
        {
            this._secret = secret;
        }

        //私钥  web.config中配置
        //"GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
        //private static string secret = ConfigurationManager.AppSettings["Secret"].ToString();
        private string _secret = "";
        /// <summary>
        /// 生成JwtToken
        /// </summary>
        /// <param name="payload">不敏感的用户数据</param>
        /// <returns></returns>
        public string SetJwtEncode(JSON payload)
        {

            //格式如下
            //var payload = new Dictionary<string, object>
            //{
            //    { "username","admin" },
            //    { "pwd", "claim2-value" }
            //};

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, _secret);
            return token;
        }

        /// <summary>
        /// 根据jwtToken  获取实体
        /// </summary>
        /// <param name="token">jwtToken</param>
        /// <returns></returns>
        public T GetJwtDecode(string token)
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);
            var userInfo = decoder.DecodeToObject<T>(token, _secret, verify: true);//token为之前生成的字符串
            return userInfo;
        }
    }
}