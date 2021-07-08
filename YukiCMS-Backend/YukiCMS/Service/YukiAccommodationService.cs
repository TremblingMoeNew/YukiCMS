using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiAccommodationService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiAccAssignment> _acca;
        private readonly YukiGlobalService _globalService;
        private readonly YukiUserService _userService;
        private readonly YukiCommitteeService _committeeService;
        private readonly YukiPaymentService _billService;
        public YukiAccommodationService(
                IYukiDatabaseSettings settings,
                YukiGlobalService globalService,
                YukiUserService userService,
                YukiCommitteeService committeeService,
                YukiPaymentService billService
            )
        {
            _acca = getMongoCollection<YukiAccAssignment>(settings.accommodationManagementColleName, settings);
            _globalService = globalService;
            _userService = userService;
            _committeeService = committeeService;
            _billService = billService;
        }
        public bool create(List<int> assignment)
        {
            if (assignment != null) return false;
            if (assignment.Count == 0) return false;
            if (assignment.Count > 2) return false;
            foreach(var u in assignment)
            {
                if (getByUid(u) != null) return false;
                var usr = _userService.getByUid(u);
                if (usr == null) return false;
                if (!usr.accommodation.isGA) return false;
                if (usr.isWithdrawaled) return false;
            }
            YukiAccAssignment acca = new YukiAccAssignment
            {
                accaId = _globalService.nextACCAId(),
                assignment = assignment
            };
            _acca.InsertOne(acca);
            return true;
        }
        public YukiAccAssignment get(Expression<Func<YukiAccAssignment, bool>> filter) =>
            _acca.Find(filter).FirstOrDefault();
        public List<YukiAccAssignment> getList(Expression<Func<YukiAccAssignment, bool>> filter) =>
            _acca.Find(filter).ToList();
        public YukiAccAssignment getByACCAId(int accaid) =>
            get(a => a.accaId == accaid);
        public YukiAccAssignment getByUid(int uid) =>
            get(a => a.assignment.Contains(uid));
        public List<YukiAccAssignment> getAll() =>
            getList(a => true);

        public bool delete(int accaid) =>
            _acca.DeleteOne(a => a.accaId == accaid).DeletedCount > 0;

        public bool updateAccInfo(int uid, YukiUserAccInfo info)
        {
            var acc = _userService.getAccInfoByUid(uid);
            if (acc == null) return false;
            if (!_userService.updateAccInfo(uid, info)) return false;
            info.isPaid = acc.isPaid;
            _billService.recalculateRegBillByUid(uid);
            return true;
        }
    }
}
