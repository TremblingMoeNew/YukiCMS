using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models.Response
{
    public class YukiUserNameResponse
    {
        public int uid { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }
    public class YukiUserBriefResponse
    {
        public int uid { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string sex { get; set; }
        public string qqNumber { get; set; }
        public string wechatNumber { get; set; }
        public string school { get; set; }
        public string phoneNumber { get; set; }
    }
    public class YukiUserBriefWithConfResponse
    {
        public int uid { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string sex { get; set; }
        public string qqNumber { get; set; }
        public string wechatNumber { get; set; }
        public string phoneNumber { get; set; }
        public List<YukiCommitteeNameItemResponse> confs { get; set; }
    }
    public class YukiUserPaymentStatusResponse
    {
        public int uid { get; set; }
        public YukiUserAccInfo accommodation { get; set; }
        public bool isRegPaid { get; set; }
        public bool isWithdrawaled { get; set; }
    }
    public class YukiUserGeneralResponse
    {
        public int uid { get; set; }
        public bool isRegPaid { get; set; }
        public bool isGA { get; set; }
        public int comCount { get; set; }
        public int activeReviewsCount { get; set; }
        public int seatsCount { get; set; }
        public bool isAccAssigned { get; set; }
        public int accExtendedDays { get; set; }
        public bool isWithdrawed { get; set; }
    }
    public class YukiUserDetailedResponse
    {
        public int uid { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string sex { get; set; }
        public string school { get; set; }
        public bool isinSchoolGroup { get; set; }
        public bool isGA { get; set; }
        public bool isRegPaid { get; set; }
        public bool isWithdrawaled { get; set; }
    }
}
