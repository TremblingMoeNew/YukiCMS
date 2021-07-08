using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiPermission
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int uid { get; set; }
        public YukiPermissionItem permission { get; set; }
        public List<int> grantFromGroup { get; set; }
    }
    public class YukiPermissionGroup
    {
        [BsonId]
        public BsonObjectId id { get; set; }
        public int pgid { get; set; }
        public string name { get; set; }
        public List<YukiPermissionItem> permissionList { get; set; }
        public List<int> members { get; set; }
        public bool isFreeGroup { get; set; }
    }
    public class YukiPermissionItem
    {
        public YukiPermissionType ptype { get; set; }
        public int pObjectId { get; set; }
    }
    public enum YukiPermissionType   // TODO: More PermissionTypes
    {
        // Placeholder Permission
        None,
        // Global / Conference Permission
        Global_Get_Users,
        Global_Withdrawal_Users,
        Global_Get_User_Info,
        Global_Edit_User_Info,
        Global_View_Committee_Details,
        Global_Create_Committee,
        Global_Transfer_Committee_Members,
        Global_Get_User_Permissions_List,
        Global_Grant_Revoke_User_Permissions_Customize,
        Global_Get_Group_Permissions,
        Global_Grant_Revoke_Group_Permissions,
        Global_Create_Group_Customize,
        Global_Grant_Revole_Group_To_User_Customize,
        Global_View_Payments,
        Global_Modify_Payments,
        Global_Edit_Payment_Settings,
        Global_View_Accommodations,
        Global_Modify_Accommodations,
        Global_Cannot_Withdrawal,

        // Committee Permission
        Committee_View,
        Committee_Get_Members,
        Committee_Edit_Info,
        Committee_Create_Seats,
        Committee_Assign_Seats,
        Committee_Create_Reviews,
        Committee_Push_Reviews,
        Committee_General_Reviewer,
        Committee_Cancel_Reviews,
        Committee_Create_Review_Template,

        // File Group Permission
        FileGroup_Upload_File,
        FileGroup_Download_File,

        // Hidden ( Super User ) Permission
        Global_Core_Group_Member,
        Global_Root_Administrator
    }
    public class YukiPermissionItemEqualComparer : IEqualityComparer<YukiPermissionItem>
    {
        public bool Equals(YukiPermissionItem x, YukiPermissionItem y)
        {
            return x.ptype == y.ptype && x.pObjectId == y.pObjectId;
        }

        public int GetHashCode(YukiPermissionItem obj)
        {
            return base.GetHashCode();
        }
    }
}
