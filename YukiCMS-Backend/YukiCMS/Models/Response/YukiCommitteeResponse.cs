using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models.Response
{
    public class YukiCommitteeNameItemResponse
    {
        public int cid { get; set; }
        public string name { get; set; }
    }
    public class YukiCommitteeNamePermissionResponse
    {
        public int cid { get; set; }
        public string name { get; set; }
        public bool enableSeatManagement { get; set; }
    }
    public class YukiCommitteeBriefResponse
    {
        public int cid { get; set; }
        public YukiCommitteeType ctype { get; set; }
        public string name { get; set; }
        public string cdesc { get; set; }
        public DateTime applyDDL { get; set; }
    }
    public class YukiCommitteePaymentSettingsResponse
    {
        public int cid { get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public double refund { get; set; }
        public bool paymentEnabled { get; set; }
    }
    public class YukiCommitteeDetailedResponse
    {
        public int cid { get; set; }
        public YukiCommitteeType ctype { get; set; }
        public string name { get; set; }
        public string cdesc { get; set; }
        public int autoPushedTasksCount { get; set; }
        public DateTime applyDDL { get; set; }
        public int membersCount { get; set; }
        public YukiCommitteePaymentSettings paymentSettings { get; set; }
        public int paidMembersCount { get; set; }
    }
}
