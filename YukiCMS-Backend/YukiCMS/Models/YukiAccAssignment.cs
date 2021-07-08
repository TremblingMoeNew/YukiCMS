using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiAccAssignment
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int accaId { get; set; }
        public List<int> assignment { get; set; }
    }
}
