using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiBill
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int billid { get; set; }
        public int uid { get; set; }
        public YukiBillType type { get; set; }
        public double amount { get; set; }
        public YukiBillStatus status { get; set; }
        public int signatureCode { get; set; }
    }
    public enum YukiBillType
    {
        normalIncome = 0,
        regConsume = 1,
        accommodationComsume = 2,
        regAndAccComsume = 3,
        normalOutcome = 4,
        withdrawal = 5,
        financialIncome = 6,
        financialOutcome = 7,
        regAdjustRefund = 8
    }
    public enum YukiBillStatus
    {
        incompleted = 0,
        completed = 1,
        cancelled = 2,
        partial = 3,
        merged = 4
    }
}
