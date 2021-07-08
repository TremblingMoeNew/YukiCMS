using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YukiCMS.Models
{
    public class YukiFileGroup
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int fgid { get; set; }
        public string name { get; set; }
        public List<String> files { get; set; }
    }
}
