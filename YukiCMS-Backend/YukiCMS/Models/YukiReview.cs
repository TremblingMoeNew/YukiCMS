using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiReview
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int tid { get; set; }
        public int uid { get; set; }
        public int cid { get; set; }
        public int admUid { get; set;}
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime startAt { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ddl { get; set; }
        public List<int> questions { get; set; }
        public string comment { get; set; }
        public YukiReviewStatus status { get; set; }
    }
    public class YukiReviewTemplate
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int ttid { get; set; }
        public int cid { get; set; }
        public string name { get; set; }
        public int duration { get; set; }
        public List<String> questions { get; set; }
    }
    public class YukiQuestion
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int qid { get; set; }
        public int uid { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public string attachment { get; set; }
    }
    public enum YukiReviewStatus
    {
        incompleted = 0,
        submitted = 1,
        completed = 2,
        expired = 3,
        cancelled = 4
    }
}
