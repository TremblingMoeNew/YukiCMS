using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiPermissionService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiPermission> _permission;
        private readonly YukiGlobalService _globalService;
        public YukiPermissionService(IYukiDatabaseSettings settings, YukiGlobalService globalService)
        {
            _permission = getMongoCollection<YukiPermission>(settings.permissionColleName, settings);
            _globalService = globalService;
        }

        public bool create(YukiPermission permission)
        {
            if (permission.permission.ptype == YukiPermissionType.Global_Root_Administrator) return false;
            if (permission.grantFromGroup == null) permission.grantFromGroup = new List<int>();
            _permission.InsertOne(permission);
            return true;
        }
        public YukiPermission get(Expression<Func<YukiPermission, bool>> filter) =>
            _permission.Find(filter).FirstOrDefault();
        public List<YukiPermission> getList(Expression<Func<YukiPermission, bool>> filter) =>
            _permission.Find(filter).ToList();
        public void update<TResult>(Expression<Func<YukiPermission, bool>> filter,
                    Expression<Func<YukiPermission, TResult>> updatedProperty,
                    TResult value
            ) =>
            _permission.UpdateOne(
                filter,
                Builders<YukiPermission>.Update.Set(updatedProperty, value)
            );
        public void replace(Expression<Func<YukiPermission, bool>> filter, YukiPermission permission) =>
            _permission.ReplaceOne(filter, permission);

        public void delete(Expression<Func<YukiPermission, bool>> filter) =>
            _permission.DeleteOne(filter);

        public YukiPermission getPermission(int uid, YukiPermissionType ptype, int poid) =>
            get(p => p.uid == uid && p.permission.ptype == ptype && p.permission.pObjectId == poid);

        private bool vertify(int uid, YukiPermissionType ptype, int pObjectId) =>
            getPermission(uid, ptype, pObjectId) != null;
        public bool vertifyRoot(int uid) =>
            vertify(uid, YukiPermissionType.Global_Root_Administrator, 0);
        public bool vertifySuperUser(int uid) =>
            vertifyRoot(uid) || vertify(uid, YukiPermissionType.Global_Core_Group_Member, 0);
        public bool vertifyPermission(int uid, YukiPermissionType ptype, int pObjectId) =>
            vertifySuperUser(uid)||vertify(uid, ptype, pObjectId);

        public List<YukiPermission> getPermissionsByUT(int uid, YukiPermissionType ptype) =>
            getList(p => p.uid == uid && p.permission.ptype == ptype);
        public List<YukiPermission> getPermissionsByTO(YukiPermissionType ptype,int pObjectId) =>
           getList(p => p.permission.pObjectId == pObjectId && p.permission.ptype == ptype);
        public bool addGranter(int uid,YukiPermissionType ptype,int pObjectID,int granter)
        {
            var permission = getPermission(uid, ptype, pObjectID);
            if (permission == null)
            {
                permission = new YukiPermission
                {
                    uid = uid,
                    permission = new YukiPermissionItem
                    {
                        ptype = ptype,
                        pObjectId = pObjectID
                    },
                    grantFromGroup = new List<int>
                    {
                        granter
                    }
                };
                return create(permission);
            }
            else
            {
                permission.grantFromGroup.Add(granter);
                replace(p => p.uid == uid && p.permission.ptype == ptype && p.permission.pObjectId == pObjectID, permission);
                return true;
            }
        }
        public bool removeGranter(int uid, YukiPermissionType ptype, int pObjectID, int granter)
        {
            if (ptype == YukiPermissionType.Global_Root_Administrator) return false;
            var permission = getPermission(uid, ptype, pObjectID);
            if (!permission.grantFromGroup.Remove(granter)) return false;
            if (permission.grantFromGroup.Count == 0)
                delete(p => p.uid == uid && p.permission.ptype == ptype && p.permission.pObjectId == pObjectID);
            else
                replace(p => p.uid == uid && p.permission.ptype == ptype && p.permission.pObjectId == pObjectID, permission);
            return true;
        }
        public bool isGlobalPermissionType(YukiPermissionType ptype)
        {
            return new HashSet<YukiPermissionType>
            {
                YukiPermissionType.Global_Get_Users,
                YukiPermissionType.Global_Withdrawal_Users,
                YukiPermissionType.Global_Get_User_Info,
                YukiPermissionType.Global_Edit_User_Info,
                YukiPermissionType.Global_Create_Committee,
                YukiPermissionType.Global_Transfer_Committee_Members,
                YukiPermissionType.Global_Get_User_Permissions_List,
                YukiPermissionType.Global_Grant_Revoke_User_Permissions_Customize,
                YukiPermissionType.Global_Get_Group_Permissions,
                YukiPermissionType.Global_Grant_Revoke_Group_Permissions,
                YukiPermissionType.Global_Create_Group_Customize,
                YukiPermissionType.Global_Grant_Revole_Group_To_User_Customize,
                YukiPermissionType.Global_View_Payments,
                YukiPermissionType.Global_Modify_Payments,
                YukiPermissionType.Global_Edit_Payment_Settings,
                YukiPermissionType.Global_View_Accommodations,
                YukiPermissionType.Global_Modify_Accommodations,
                YukiPermissionType.Global_Cannot_Withdrawal,
            }.Contains(ptype);
        }
        public bool isCommitteePermissionType(YukiPermissionType ptype)
        {
            return new HashSet<YukiPermissionType>
            {
                YukiPermissionType.Committee_View,
                YukiPermissionType.Committee_Get_Members,
                YukiPermissionType.Committee_Edit_Info,
                YukiPermissionType.Committee_Create_Seats,
                YukiPermissionType.Committee_Assign_Seats,
                YukiPermissionType.Committee_Create_Reviews,
                YukiPermissionType.Committee_Push_Reviews,
                YukiPermissionType.Committee_General_Reviewer,
                YukiPermissionType.Committee_Cancel_Reviews,
                YukiPermissionType.Committee_Create_Review_Template
            }.Contains(ptype);
        }
        public bool isFileGroupPermissionType(YukiPermissionType ptype)
        {
            return new HashSet<YukiPermissionType>
            {
                YukiPermissionType.FileGroup_Upload_File,
                YukiPermissionType.FileGroup_Download_File
            }.Contains(ptype);
        }
    }


}
