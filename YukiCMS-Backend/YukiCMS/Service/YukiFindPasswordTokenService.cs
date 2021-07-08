using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiFindPasswordTokenService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiFindPasswordToken> _token;
        public YukiFindPasswordTokenService(IYukiDatabaseSettings settings)
        {
            _token = getMongoCollection<YukiFindPasswordToken>(settings.findPasswordTokenColleName, settings);
        }
        public string create(int uid)
        {
            string token = "";
            do
            {
                byte[] salt = new byte[2048];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
                using (SHA1 sha = SHA1.Create())
                {
                    byte[] hmb = sha.ComputeHash(salt);
                    StringBuilder sBuilder = new StringBuilder();
                    for (int i = 0; i < hmb.Length; i++)
                    {
                        sBuilder.Append(hmb[i].ToString("x2"));
                    }
                    token = sBuilder.ToString();
                }
            } while (checkUid(token)!=-1);
            var res = new YukiFindPasswordToken
            {
                uid = uid,
                token = token,
                expire = DateTime.Now.AddHours(1)
            };
            _token.InsertOne(res);
            return token;
        }
        public int checkUid(string token)
        {
            var res = _token.Find(t => t.token == token).FirstOrDefault();
            if (res == null) return -1;
            if (res.expire < DateTime.Now)
            {
                expire();
                return -1;
            }
            return res.uid;
        }
        public void expire()
        {
            _token.DeleteMany(token=>token.expire<DateTime.Now);
        }
        public void invalidateToken(string token)
        {
            _token.DeleteOne(t => t.token == token);
        }
    }
}
