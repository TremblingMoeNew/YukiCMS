using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiSeat
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int sid { get; set; }
        public int cid { get; set; }
        public string name { get; set; }
        public int uid { get; set; }
        public YukiSeatStatus status { get; set; }
    }
    public enum YukiSeatStatus
    {
        unassigned = 0,
        assigned = 1
    }
}
