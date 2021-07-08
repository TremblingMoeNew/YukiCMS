using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiFile
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public byte[] blob { get; set; }
        public string fileOriginalName { get; set; }
        public string fileStorageName { get; set; }
        public string fileClaimedName { get; set; }
        public string fileContentType { get; set; }
        public int fileQuoteCount { get; set; }
    }
}
