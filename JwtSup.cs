using Newtonsoft.Json.Linq;
using System;

namespace MCDT
{
    /// <summary>
    /// JWT载荷实体
    /// </summary>
    public sealed class JwtSup
    {

        /// <summary>
        /// jwt的签发时间
        /// </summary>
        public long TimeStamp { get; set; } = Function.GetTimeStamp();
        /// <summary>
        /// jwt的过期时间，这个过期时间必须要大于签发时间.默认60分钟
        /// </summary>
        public long Exp { get; set; }

        public string IP { get; set; }
        /// <summary>
        /// jwt的唯一身份标识，主要用来作为一次性token,从而回避重放攻击。
        /// </summary>
        public string Jti { get; set; } = Guid.NewGuid().ToString("N");
        /// <summary>
        /// 自定义对象
        /// </summary>
        public object Data { get; set; }
    }
}