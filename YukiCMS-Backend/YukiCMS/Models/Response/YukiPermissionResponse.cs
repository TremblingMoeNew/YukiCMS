using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models.Response
{
    public class YukiPermissionResponse
    {
        public YukiPermissionType type { get; set; }
        public List<YukiPermissionResponseObjectItem> pObject { get; set; }
    }
    public class YukiPermissionResponseObjectItem
    {
        public int pObjectId { get; set; }
        public string pObjectName { get; set; }
        public int pGranterCount { get; set; }
        public bool customizeGranted { get; set; }
    }
    public class YukiPermissionGranterResponseItem
    {
        public int granterId { get; set; }
        public string granterName { get; set; }
    }
    public class YukiPermissionGroupResponse
    {
        public int pgid { get; set; }
        public string name { get; set; }
        public List<YukiPermissionGroupResponsePermissionItem> permissionList { get; set; }
        public List<YukiUserBriefResponse> members { get; set; }
        public bool isFreeGroup { get; set; }
        public int cid { get; set; }
        public string cname { get; set; }
        public int fgrpid { get; set; }
    }
    public class YukiPermissionGroupResponsePermissionItem
    {
        public YukiPermissionType ptype { get; set; }
        public int pObjectId { get; set; }
        public string pObjectName { get; set; }
    }
}
