using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiBudget
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int budgetId { get; set; }
        public string name { get; set; }
        public YukiBudgetType type { get; set; }
        public int count { get; set; }
        public double priceSingle { get; set; }
    }
    public enum YukiBudgetType
    {

    }
}
