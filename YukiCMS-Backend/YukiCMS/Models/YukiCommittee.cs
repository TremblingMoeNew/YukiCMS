using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiCommittee
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int cid { get; set; }
        public YukiCommitteeType ctype { get; set; }
        public string name { get; set; }
        public string cdesc { get; set; }
        public int fgrpid { get; set; }
        public int pgrpid { get; set; }
        public List<int> autoPushedTasks { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime applyDDL { get; set; }
        public List<int> members { get; set; }
        public YukiCommitteePaymentSettings paymentSettings { get; set; }
    }
    public enum YukiCommitteeType
    {
        appliableNormalMutualCommittee = 0,
        appliableAdmCommittee = 1,
        unappliable = 2,
        unappliableMutual = 3,
    }
    public class YukiCommitteePaymentSettings
    {
        public double price { get; set; }
        public double refund { get; set; }
        public bool paymentEnabled { get; set; }
    }
}
