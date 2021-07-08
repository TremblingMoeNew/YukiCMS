using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiGlobal
    {
        [BsonId]
        public BsonObjectId id { get; set; }

        public YukiGlobalSettings settings { get; set; }

        public YukiGlobalIdCounter counter { get; set; }

    }
    public class YukiGlobalSettings
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime regPaymentDeadline { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime conferenceBeginDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime conferenceEndDate { get; set; }
        public double standardAccPrice { get; set; }
        public double extendedAccDailyPrice { get; set; }
        public double standardAccRefund { get; set; }
        public double extendedAccDailyRefund { get; set; }
    }
    public class YukiGlobalIdCounter
    {
        public int nextUid { get; set; }  // YukiUser / User Colle
        public int nextCid { get; set; }  // YukiCommittee / Committee Colle
        public int nextSid { get; set; }  // YukiSeat / Seat Colle
        public int nextTid { get; set; }  // YukiReview / Review/Task Colle
        public int nextTTid { get; set; } // YukiReviewTemplate / Review/Task-Template Colle
        public int nextQid { get; set; }  // YukiQuestion / Question Colle
                                          // YukiPermission / Permission Colle
        public int nextPGid { get; set; } // YukiPermissionGroup / Permission-Group Colle
        public int nextFGid { get; set; } // YukiFileGroup / File-Group Colle
                                          // YukiFile / File Colle
        public int nextBillId { get; set; }  // YukiBill / Bill Colle
        public int nextBudgetId { get; set; }// YukiBudget / Budget Colle ( Deprecated )

        public int nextACCAId { get; set; }  // YukiAccAssignment / Accomodation-Assignment Colle
    }
}
