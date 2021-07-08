using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiPermissionGroupService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiPermissionGroup> _group;
        private readonly YukiPermissionService _permissionService;
        private readonly YukiGlobalService _globalService;
        public YukiPermissionGroupService(IYukiDatabaseSettings settings, YukiGlobalService globalService,YukiPermissionService permissionService)
        {
            _group = getMongoCollection<YukiPermissionGroup>(settings.permissionGroupColleName, settings);
            _globalService = globalService;
            _permissionService = permissionService;
        }

        public void create(YukiPermissionGroup group)
        {
            _group.InsertOne(group);
        }
        public YukiPermissionGroup get(Expression<Func<YukiPermissionGroup, bool>> filter) =>
            _group.Find(filter).FirstOrDefault();
        public List<YukiPermissionGroup> getList(Expression<Func<YukiPermissionGroup, bool>> filter) =>
            _group.Find(filter).ToList();
        public bool update<TResult>(Expression<Func<YukiPermissionGroup, bool>> filter,
                    Expression<Func<YukiPermissionGroup, TResult>> updatedProperty,
                    TResult value
            ) =>
            _group.UpdateOne(
                filter,
                Builders<YukiPermissionGroup>.Update.Set(updatedProperty, value)
            ).ModifiedCount>0;
        public bool replace(Expression<Func<YukiPermissionGroup, bool>> filter, YukiPermissionGroup group) =>
            _group.ReplaceOne(filter, group).ModifiedCount>0;

        public bool delete(Expression<Func<YukiPermissionGroup, bool>> filter) =>
            _group.DeleteOne(filter).DeletedCount>0;

        public YukiPermissionGroup getByPGid(int pgid) =>
            get(group => group.pgid == pgid);

        public List<YukiPermissionGroup> getAll() =>
            getList(g => g.pgid != 1);
        public List<YukiPermissionGroup> getPGByMember(int uid) =>
            getList(g => g.members.Contains(uid));
        public List<YukiPermissionGroup> getAllByRoot() =>
            getList(g => true);
        public List<YukiPermissionGroup> getFreeGroup() =>
            getList(group => group.pgid != 1 && group.isFreeGroup == true);
        public List<YukiPermissionGroup> getFreeGroupByRoot() =>
            getList(group => group.isFreeGroup == true || group.pgid == 1);

        public bool addPermission(int pgid,YukiPermissionItem item)
        {
            if (item.ptype == YukiPermissionType.Global_Root_Administrator) return false;
            if (item.ptype == YukiPermissionType.Global_Core_Group_Member) return false;
            var group = getByPGid(pgid);
            if (group == null) return false;
            if (group.permissionList.Contains(item, new YukiPermissionItemEqualComparer())) return false;
            group.permissionList.Add(item);
            foreach(int uid in group.members)
            {
                _permissionService.addGranter(uid, item.ptype, item.pObjectId, pgid);
            }
            return update(grp => grp.pgid == pgid, g => g.permissionList, group.permissionList);
        }
        public bool addPermission(int pgid, YukiPermissionType ptype, int poid)
        {
            var item = new YukiPermissionItem
            {
                ptype = ptype,
                pObjectId = poid
            };
            return addPermission(pgid, item);
        }
        public bool removePermission(int pgid, YukiPermissionType ptype, int poid)
        {
            if (ptype == YukiPermissionType.Global_Root_Administrator) return false;
            if (ptype == YukiPermissionType.Global_Core_Group_Member) return false;
            var group = getByPGid(pgid);
            if (group == null) return false;
            YukiPermissionItem item = null;
            foreach (var pi in group.permissionList)
            {
                if (pi.ptype == ptype && pi.pObjectId == poid) 
                {
                    item = pi;
                    break;
                }
            }
            if (item == null) return false ;
            group.permissionList.Remove(item);
            foreach (int uid in group.members)
            {
                _permissionService.removeGranter(uid, item.ptype, item.pObjectId, pgid);
            }
            return update(grp => grp.pgid == pgid, g => g.permissionList, group.permissionList);
        }
        public bool addMember(int pgid,int uid)
        {
            var group = getByPGid(pgid);
            if (group == null) return false;
            if (group.members.Contains(uid)) return false;
            group.members.Add(uid);
            foreach(var item in group.permissionList)
            {
                _permissionService.addGranter(uid, item.ptype, item.pObjectId, pgid);
            }
            return update(grp => grp.pgid == pgid, g => g.members, group.members);
        }
        public bool removeMember(int pgid, int uid)
        {
            var group = getByPGid(pgid);
            if (group == null) return false;
            if (!group.members.Contains(uid)) return false;
            group.members.Remove(uid);
            foreach (var item in group.permissionList)
            {
                _permissionService.removeGranter(uid, item.ptype, item.pObjectId, pgid);
            }
            return update(grp => grp.pgid == pgid, g => g.members, group.members);
        }
        public bool removeGroup(int pgid)
        {
            var group = getByPGid(pgid);
            if (group == null) return false;
            if (!group.isFreeGroup) return false;
            foreach(var member in group.members)
            {
                foreach(var item in group.permissionList)
                {
                    _permissionService.removeGranter(member, item.ptype, item.pObjectId, pgid);
                }
            }
            return delete(grp => grp.pgid == pgid);
        }
        public bool editGroupName(int pgid, string name) =>
            update(group => group.pgid == pgid, grp => grp.name, name);
        public YukiPermissionGroup createGroup(string name,bool isFreeGroup)
        {
            var group = new YukiPermissionGroup
            {
                pgid = _globalService.nextPGid(),
                name = name,
                isFreeGroup = isFreeGroup,
                members = new List<int>(),
                permissionList = new List<YukiPermissionItem>()
            };
            create(group);
            return group;
        }
        public int createCommitteeLockedGroup(int cid, int fgid, string name)
        {
            int pgid = createGroup(name, false).pgid;
            var pl = createCLGDefaultPermissions(cid, fgid);
            update(grp => grp.pgid == pgid, g => g.permissionList, pl);
            return pgid;
        }
        public List<YukiPermissionItem> createCLGDefaultPermissions(int cid,int fgid)
        {
            return new List<YukiPermissionItem>
            {
                new YukiPermissionItem { ptype = YukiPermissionType.Committee_View, pObjectId = cid},
                new YukiPermissionItem { ptype = YukiPermissionType.FileGroup_Download_File, pObjectId = 0 }, //Global FileGroup手动创建
                new YukiPermissionItem { ptype = YukiPermissionType.FileGroup_Download_File, pObjectId = fgid } 
            };
        }
    }
}
