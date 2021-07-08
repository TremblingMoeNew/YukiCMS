using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;
using YukiCMS.Models.Response;

namespace YukiCMS.Service
{
    public class YukiCommitteeService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiCommittee> _committee;
        private readonly YukiGlobalService _globalService;
        private readonly YukiReviewService _reviewService;
        private readonly YukiPermissionGroupService _permissionGroupService;
        private readonly YukiFileGroupService _fileGroupService;
        private readonly YukiEmailService _emailService;
        public YukiCommitteeService(
                IYukiDatabaseSettings settings,
                YukiGlobalService globalService,
                YukiReviewService reviewService,
                YukiPermissionGroupService permissionGroupService,
                YukiEmailService emailService,
                YukiFileGroupService fileGroupService
            )
        {
            _committee = getMongoCollection<YukiCommittee>(settings.committeeColleName, settings);
            _globalService = globalService;
            _reviewService = reviewService;
            _permissionGroupService = permissionGroupService;
            _emailService = emailService;
            _fileGroupService = fileGroupService;
        }

        public int create(YukiCommittee committee)
        {
            committee.cid = _globalService.nextCid();
            committee.members = new List<int>();
            committee.autoPushedTasks = new List<int>();
            committee.fgrpid = _fileGroupService.create(committee.name);
            committee.pgrpid = _permissionGroupService.createCommitteeLockedGroup(committee.cid, committee.fgrpid, committee.name);
            committee.paymentSettings = new YukiCommitteePaymentSettings();
            _committee.InsertOne(committee);
            return committee.cid;
        }
        public YukiCommittee get(Expression<Func<YukiCommittee, bool>> filter) =>
            _committee.Find(filter).FirstOrDefault();
        public List<YukiCommittee> getList(Expression<Func<YukiCommittee, bool>> filter) =>
            _committee.Find(filter).ToList();
        public bool update<TResult>(Expression<Func<YukiCommittee, bool>> filter,
                    Expression<Func<YukiCommittee, TResult>> updatedProperty,
                    TResult value
            ) =>
            _committee.UpdateOne(
                filter,
                Builders<YukiCommittee>.Update.Set(updatedProperty, value)
            ).ModifiedCount > 0;
        public bool replace(Expression<Func<YukiCommittee, bool>> filter, YukiCommittee committee) =>
            _committee.ReplaceOne(filter, committee).ModifiedCount > 0;

        public bool delete(Expression<Func<YukiCommittee, bool>> filter) =>
            _committee.DeleteOne(filter).DeletedCount > 0;

        public YukiCommittee getByCid(int cid) =>
            get(com => com.cid == cid);

        public List<YukiCommittee> getCommitteesByType(YukiCommitteeType type) =>
            getList(com => com.ctype == type);
        public List<YukiCommittee> getCommitteesByMember(int member) =>
            getList(com =>  com.members.Contains(member));
        public List<YukiCommittee> getCommitteesByTypeAndMember(YukiCommitteeType type, int member) =>
            getList(com => com.ctype == type && com.members.Contains(member));
        public List<YukiCommittee> getMutualCommittees() =>
            getList(
                com =>
                    (com.ctype == YukiCommitteeType.appliableNormalMutualCommittee || com.ctype == YukiCommitteeType.unappliableMutual)
                );
        public List<YukiCommittee> getMutualCommitteesByMember(int member) =>
            getList(
                com =>
                    (com.ctype == YukiCommitteeType.appliableNormalMutualCommittee || com.ctype == YukiCommitteeType.unappliableMutual)
                    && com.members.Contains(member)
                );
        public List<YukiCommittee> getCompaibleCommitteesWithoutMember(int member) =>
           getList(
               com =>
                   (com.ctype == YukiCommitteeType.appliableAdmCommittee || com.ctype == YukiCommitteeType.unappliable)
                   && !com.members.Contains(member)
               );
        public List<YukiCommittee> getCommitteesByTypeWithoutMember(YukiCommitteeType type, int member) =>
            getList(com => com.ctype == type && !com.members.Contains(member));
        public YukiCommittee getPaymentEnabledCommitteeByMember(int member) =>
            get(com => com.paymentSettings.paymentEnabled == true && com.members.Contains(member));
        public List<YukiCommittee> getByFGid(int fgid) =>
            getList(com => com.fgrpid == fgid);
        public List<YukiCommittee> getByPGid(int pgid) =>
            getList(com => com.pgrpid == pgid);
        public List<int> getAutoPushedTasks(int cid) =>
            getByCid(cid).autoPushedTasks;

        public YukiCommitteePaymentSettings getPaymentSettingsByCid(int cid)
        {
            var com = getByCid(cid);
            if (com == null) return null;
            return com.paymentSettings;
        }
        public bool enableCommitteePayment(int cid) =>                  //  NOTE: 由BillService处理、唯一调用
             update(committee => committee.cid == cid, c => c.paymentSettings.paymentEnabled, true);
        public bool disableCommitteePayment(int cid) =>                 //  NOTE: 由BillService处理、唯一调用
            update(committee => committee.cid == cid, c => c.paymentSettings.paymentEnabled, false);
        
        public bool editPrice(int cid, double price, double refund)     //  NOTE: 由BillService处理、唯一调用
        {
            var ps = getPaymentSettingsByCid(cid);
            ps.price = price;
            ps.refund = refund;
            return update(com => com.cid == cid, c => c.paymentSettings, ps);
        }


        public bool changePG(int cid, int pgid)
        {
            var committee=getByCid(cid);
            if (committee == null) return false;
            if (committee.pgrpid == pgid) return false;
            foreach(var uid in committee.members)
            {
                _permissionGroupService.addMember(pgid, uid);
            }
            _permissionGroupService.removeGroup(committee.pgrpid);
            return update(com => com.cid == cid, c => c.pgrpid, pgid);
        }

        public bool editInfo(int cid, YukiCommitteeBriefResponse info)
        {
            var com = getByCid(cid);
            com.name = info.name;
            com.ctype = info.ctype;
            com.cdesc = info.cdesc;
            com.applyDDL = info.applyDDL;
            if (replace(cid, com))
            {
                _fileGroupService.editGroupName(com.fgrpid, info.name);
                _permissionGroupService.editGroupName(com.pgrpid, info.name);
                return true;
            }
            else return false;
        }

        public bool changeType(int cid, YukiCommitteeType type) =>
            update(com => com.cid == cid, c => c.ctype, type);

        public bool addAutoPushedTask(int cid,int tid)
        {
            var committee = getByCid(cid);
            if (committee == null) return false;
            if (committee.autoPushedTasks.Contains(tid)) return false;
            committee.autoPushedTasks.Add(tid);
            foreach(int uid in committee.members)
            {
                _reviewService.create(uid, tid);
                _emailService.sendRevieweeNotification(uid, committee.name);
            }
            return replace(cid, committee);
        }
        public bool removeAutoPushedTask(int cid,int tid)
        {
            var committee = getByCid(cid);
            if (committee == null) return false;
            if (!committee.autoPushedTasks.Contains(tid)) return false;
            committee.autoPushedTasks.Remove(tid);
            return replace(cid, committee);
        }
        public bool addMember(int cid,int uid)  //  TODO: Bill 
        {
            var committee = getByCid(cid);
            if (committee == null) return false;
            if (committee.members.Contains(uid)) return false;
            if (committee.ctype==YukiCommitteeType.appliableNormalMutualCommittee||committee.ctype==YukiCommitteeType.unappliableMutual)
            {
                var mutual = getCommitteesByTypeAndMember(YukiCommitteeType.appliableNormalMutualCommittee, uid);
                if (mutual.Count > 0) return false;
            }

            committee.members.Add(uid);
            _emailService.sendJoinComNotification(uid, committee.name);
            foreach(int tid in committee.autoPushedTasks)
            {
                _reviewService.create(uid, tid);
                _emailService.sendRevieweeNotification(uid, committee.name);
            }
            _permissionGroupService.addMember(committee.pgrpid, uid);
            return replace(cid, committee);
        }
        public bool removeMember(int cid,int uid)   //  TODO: Bill
        {
            var committee = getByCid(cid);
            if (committee == null) return false;
            committee.members.Remove(uid);
            _reviewService.cancelByUC(uid, cid);
            _permissionGroupService.removeMember(committee.pgrpid, uid);
            return replace(cid, committee);
        }
        public bool replace(int cid, YukiCommittee committee) =>
            replace(com => com.cid == cid, committee);
        public bool delete(int cid) =>  
            delete(com => com.cid == cid);

    }
}
