using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiFindPasswordToken
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int uid { get; set; }
        public string token { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime expire { get; set; } 
    }
}
