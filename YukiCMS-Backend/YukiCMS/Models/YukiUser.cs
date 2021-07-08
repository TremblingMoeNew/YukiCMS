using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace YukiCMS.Models
{
    public class YukiUser
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int uid { get; set; }
        public string email { get; set; }
        public string password_hash { get; set; }
        public string password_salt { get; set; }
        public YukiUserInfo info { get; set; }
        public YukiUserAccInfo accommodation { get; set; }
        public bool isRegPaid { get; set; }
        public bool isWithdrawaled { get; set; }
    }
    public class YukiUserInfo
    {
        public string name { get; set; }
        public string sex { get; set; }
        public string residentId { get; set; }
        public YukiUserResidentIdType residentIdType { get; set; }
        public string qqNumber { get; set; }
        public string wechatNumber { get; set; }
        public string phoneNumber { get; set; }
        public string state { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string school { get; set; }
        public bool isinSchoolGroup { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime birthday { get; set; }
        public string cv { get; set; }
    }
    public enum YukiUserResidentIdType
    {
        CNResidentID=0,Others=1
    }
    public class YukiUserAccInfo
    {
        public bool isGA { get; set; }
        public int aheadDays { get; set; }
        public int extendDays { get; set; }
        public string appliedRoommateName { get; set; }
        public bool isPaid { get; set; }
    }
}
