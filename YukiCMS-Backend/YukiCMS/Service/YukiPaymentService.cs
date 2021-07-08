using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiPaymentService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiBill> _bill;
        private readonly YukiGlobalService _globalService;
        private readonly YukiUserService _userService;
        private readonly YukiCommitteeService _committeeService;
        private readonly YukiEmailService _emailService;
        public YukiPaymentService(
                IYukiDatabaseSettings settings,
                YukiGlobalService globalService,
                YukiUserService userService,
                YukiEmailService emailService,
                YukiCommitteeService committeeService
            )
        {
            _bill = getMongoCollection<YukiBill>(settings.billColleName, settings);
            _globalService = globalService;
            _userService = userService;
            _emailService = emailService;
            _committeeService = committeeService;
        }
        public void create(YukiBill bill)
        {
            _bill.InsertOne(bill);
        }
        public YukiBill get(Expression<Func<YukiBill, bool>> filter) =>
            _bill.Find(filter).FirstOrDefault();
        public List<YukiBill> getList(Expression<Func<YukiBill, bool>> filter) =>
            _bill.Find(filter).ToList();
        public bool update<TResult>(
                Expression<Func<YukiBill, bool>> filter, Expression<Func<YukiBill, TResult>> updatedProperty, TResult value) =>
            _bill.UpdateOne(
                filter,
                Builders<YukiBill>.Update.Set(updatedProperty, value)
            ).ModifiedCount > 0;
        public bool replace(Expression<Func<YukiBill, bool>> filter, YukiBill bill) =>
            _bill.ReplaceOne(filter, bill).ModifiedCount > 0;

        public bool delete(Expression<Func<YukiBill, bool>> filter) =>
            _bill.DeleteOne(filter).DeletedCount > 0;

        public YukiBill getBillById(int billid) =>
            get(b => b.billid == billid);
        public YukiBill getActivatedBillBySC(int signatureCode) =>
            get(b => b.signatureCode == signatureCode && b.status == YukiBillStatus.incompleted);

        public List<YukiBill> getBillsByUid(int uid) =>
            getList(b => b.uid == uid);
        public List<YukiBill> getBillsBySC(int signatureCode) =>
            getList(b => b.signatureCode == signatureCode);

        public List<YukiBill> getBillsByUidAndStatus(int uid, YukiBillStatus status) =>
            getList(b => b.uid == uid && b.status == status);
        public List<YukiBill> getBillsByUidAndType(int uid, YukiBillType type) =>
            getList(b => b.uid == uid && b.type == type);
        public List<YukiBill> getBillsByUTS(int uid, YukiBillType type, YukiBillStatus status) =>
            getList(b => b.uid == uid && b.type == type && b.status == status);
        public List<YukiBill> getBillsByTypeAndStatus(YukiBillType type, YukiBillStatus status) =>
            getList(b => b.type == type && b.status == status);
        public YukiBill getRegBillByUid(int uid) =>
            get(
                b =>
                    b.uid == uid
                    && (b.type == YukiBillType.regConsume || b.type == YukiBillType.regAndAccComsume)
            );
        public YukiBill getPartialRegBillByUid(int uid) =>
            get(
                b =>
                    b.uid == uid
                    && (b.type == YukiBillType.regConsume || b.type == YukiBillType.regAndAccComsume)
                    && b.status == YukiBillStatus.partial
            );
        public YukiBill getActivatedRegBillByUid(int uid) =>
            get(
                b =>
                    b.uid == uid
                    && (b.type == YukiBillType.regConsume || b.type == YukiBillType.regAndAccComsume)
                    && b.status == YukiBillStatus.incompleted
            );
        public YukiBill getCancelledRegBillByUid(int uid) =>
            get(
                b =>
                    b.uid == uid
                    && (b.type == YukiBillType.regConsume || b.type == YukiBillType.regAndAccComsume)
                    && b.status == YukiBillStatus.cancelled
            );
        public YukiBill getCompletedRegBillByUid(int uid) =>
           get(
               b =>
                   b.uid == uid
                   && (b.type == YukiBillType.regConsume || b.type == YukiBillType.regAndAccComsume)
                   && b.status == YukiBillStatus.completed
           );
        public List<YukiBill>getActivatedRegBills()=>
            getList(
               b =>
                   (b.type == YukiBillType.regConsume || b.type == YukiBillType.regAndAccComsume)
                   && b.status == YukiBillStatus.incompleted
           );
        public List<YukiBill> getCompletedRegBills() =>
            getList(
               b =>
                   (b.type == YukiBillType.regConsume || b.type == YukiBillType.regAndAccComsume)
                   && b.status == YukiBillStatus.completed
           );
        public List<YukiBill> getRegBills() =>
            getList(b => (b.type == YukiBillType.regConsume || b.type == YukiBillType.regAndAccComsume));


        public List<YukiBill> getActivatedWithdrawalBillsById(int uid) =>
            getList(b => b.uid == uid && b.type == YukiBillType.withdrawal && b.status == YukiBillStatus.incompleted);
        public List<YukiBill> getActivatedWithdrawalBills() =>
            getList(b => b.type == YukiBillType.withdrawal && b.status == YukiBillStatus.incompleted);
        public List<YukiBill> getCompletedWithdrawalBills() =>
            getList(b => b.type == YukiBillType.withdrawal && b.status == YukiBillStatus.completed);
        public List<YukiBill> getWithdrawalBillsById(int uid) =>
            getList(b => b.uid == uid && b.type == YukiBillType.withdrawal);
        public List<YukiBill> getWithdrawalBills() =>
            getList(b => b.type == YukiBillType.withdrawal);
        public bool createRegBill(int uid)
        {
            var usr = _userService.getByUid(uid);
            var com = _committeeService.getPaymentEnabledCommitteeByMember(uid);
            if (com == null) return false;
            double price = com.paymentSettings.price;
            YukiBillType type = YukiBillType.regConsume;
            if(usr.accommodation.isGA)
            {
                var gs = _globalService.getSettings();
                price += gs.standardAccPrice;
                price += (usr.accommodation.aheadDays + usr.accommodation.extendDays) * gs.extendedAccDailyPrice;
                type = YukiBillType.regAndAccComsume;
            }
            var rng = new Random();
            int a = rng.Next(100000, 1000000);
            while (getActivatedBillBySC(a) != null) a++;

            YukiBill bill = new YukiBill
            {
                billid = _globalService.nextBillId(),
                uid = uid,
                type = type,
                amount = price,
                signatureCode = a,
                status = YukiBillStatus.incompleted
            };
            create(bill);
            _emailService.sendRegBillNotification(uid, price);
            return true;
        }
        private bool recalculateRegBill(YukiBill cbill)
        {
            if (cbill == null) return false;
            var usr = _userService.getByUid(cbill.uid);
            var com = _committeeService.getPaymentEnabledCommitteeByMember(cbill.uid);
            if (com == null) return false;
            double price = com.paymentSettings.price;
            YukiBillType type = YukiBillType.regConsume;
            if (usr.accommodation.isGA)
            {
                var gs = _globalService.getSettings();
                price += gs.standardAccPrice;
                price += (usr.accommodation.aheadDays + usr.accommodation.extendDays) * gs.extendedAccDailyPrice;
                type = YukiBillType.regAndAccComsume;
            };
            cbill.amount = price;
            cbill.type = type;
            return replace(b => b.billid == cbill.billid, cbill);
        }
        public bool recalculateRegBillByUid(int uid)
        {
            var cbill=getActivatedRegBillByUid(uid);
            return recalculateRegBill(cbill);
        }
        public void recalculateActiveRegBills()
        {
            var bills = getActivatedRegBills();
            foreach(var cbill in bills)
            {
                recalculateRegBill(cbill);
            }
        }

        public bool createWithdrawalRefundBill(int uid)
        {
            var usr = _userService.getByUid(uid);
            if (usr == null) return false;
            _emailService.sendWithdrawNotification(uid);
            if (usr.isRegPaid == false)
            {
                var abill = getActivatedRegBillByUid(uid);
                if (abill != null) cancelBillById(abill.billid);
            }
            var obill = getCompletedRegBillByUid(uid);
            if (obill == null) return true;
            var com = _committeeService.getPaymentEnabledCommitteeByMember(uid);
            if (com == null) return false;
            double price = com.paymentSettings.refund;
            if (usr.accommodation.isGA && usr.accommodation.isPaid == true) 
            {
                var gs = _globalService.getSettings();
                price += gs.standardAccRefund;
                price += (usr.accommodation.aheadDays + usr.accommodation.extendDays) * gs.extendedAccDailyRefund;
            }
             price = Math.Min(obill.amount, price);
            if (price == 0) return true;
            var rng = new Random();
            int a = rng.Next(100000, 1000000);
            while (getActivatedBillBySC(a) != null) a++;

            YukiBill bill = new YukiBill
            {
                billid = _globalService.nextBillId(),
                uid = uid,
                type = YukiBillType.withdrawal,
                amount = price,
                signatureCode = a,
                status = YukiBillStatus.incompleted
            };
            create(bill);
            return true;
        }
        private bool recalculateWithdrawalBill(YukiBill cbill)
        {
            if (cbill == null) return false;
            var usr = _userService.getByUid(cbill.uid);
            var com = _committeeService.getPaymentEnabledCommitteeByMember(cbill.uid);
            if (com == null) return false;
            double price = com.paymentSettings.refund;
            if (usr.accommodation.isGA && usr.accommodation.isPaid == true)
            {
                var gs = _globalService.getSettings();
                price += gs.standardAccRefund;
                price += (usr.accommodation.aheadDays + usr.accommodation.extendDays) * gs.extendedAccDailyRefund;
            }
            price = Math.Min(price, cbill.amount);
            if (price == 0)
            {
                cancelBill(cbill);
                return true;
            }
            cbill.amount = price;
            return replace(b => b.billid == cbill.billid, cbill);
        }
        public bool recalculateWithdrawalBillByUid(int uid)
        {
            var bills = getActivatedWithdrawalBillsById(uid);
            foreach(var cbill in bills)
            {
                recalculateWithdrawalBill(cbill);
            }
            return true;
        }
        public void recalculateWithdrawalBills()
        {
            var bills = getActivatedWithdrawalBills();
            foreach (var cbill in bills)
            {
                recalculateWithdrawalBill(cbill);
            }
        }



        private bool completeBill(YukiBill bill)
        {
            if (bill == null) return false;
            if (bill.status != YukiBillStatus.incompleted) return false;
            bill.status = YukiBillStatus.completed;
            if (bill.type == YukiBillType.regConsume)
            {
                _userService.updateRegPaid(bill.uid, true);
            }
            else if (bill.type == YukiBillType.regAndAccComsume)
            {
                _userService.updateRegPaid(bill.uid, true);
                _userService.updateAccPaid(bill.uid, true);
            }
            return replace(b => b.billid == bill.billid, bill);
        }
        public bool completeBillBySC(int sc)
        {
            var bill = getActivatedBillBySC(sc);
            return completeBill(bill);
        }
        public bool completeBillById(int id)
        {
            var bill = getBillById(id);
            return completeBill(bill);
        }
        public bool completeBillBySCWithPriceModified(int sc, double price)
        {
            var bill = getActivatedBillBySC(sc);
            if (bill == null) return false;
            bill.amount = price;
            return completeBill(bill);
        }
        public bool completeBillByIdWithPriceModified(int id,double price)
        {
            var bill = getBillById(id);
            if (bill == null) return false;
            bill.amount = price;
            return completeBill(bill);
        }
        private bool cancelBill(YukiBill bill)
        {
            if (bill == null) return false;
            if (bill.status != YukiBillStatus.incompleted) return false;
            bill.status = YukiBillStatus.cancelled;
            return replace(b => b.billid == bill.billid, bill);
        }
        public bool cancelBillById(int id)
        {
            var bill = getBillById(id);
            return cancelBill(bill);
        }
        public bool cancelBillBySC(int sc)
        {
            var bill = getActivatedBillBySC(sc);
            return cancelBill(bill);
        }
        public bool cancelRegBill(int uid)
        {
            var bill = getActivatedRegBillByUid(uid);
            return cancelBill(bill);
        }
        public bool enableCommitteePayment(int cid)
        {
            var com = _committeeService.getByCid(cid);
            if (com == null) return false;
            if (com.paymentSettings.paymentEnabled) return false;
            _committeeService.enableCommitteePayment(cid);
            foreach (var uid in com.members)
            {
                createRegBill(uid);
            }
            return true;
        }
        public bool disableCommitteePayment(int cid)
        {
            var com = _committeeService.getByCid(cid);
            if (com == null) return false;
            if (!com.paymentSettings.paymentEnabled) return false;
            foreach (var uid in com.members)
            {
                cancelRegBill(uid);
            }
            _committeeService.disableCommitteePayment(cid);
            return true;
        }
        public bool editCommitteeRegPrice(int cid,YukiCommitteePaymentSettings settings)
        {
            if (!_committeeService.editPrice(cid, settings.price, settings.refund)) return false;
            recalculateActiveRegBills();
            recalculateWithdrawalBills();
            return true;
        }
        public bool withdrawByUid(int uid)       // NOTE: 需要由Controller来控制退会时退出所有东西
        {
            var usr = _userService.getByUid(uid);
            if (usr == null) return false;
            createWithdrawalRefundBill(uid);
            _userService.updateWD(uid, true);
            return true;
        }
        public double calculateConfPaymentCurIncome()
        {
            double income = 0;
            var regc = getCompletedRegBills();
            foreach(var b in regc)
            {
                income += b.amount;
            }
            var wdlc = getCompletedWithdrawalBills();
            foreach(var b in wdlc)
            {
                income -= b.amount;
            }
            return income;
        }
        public double calculateConfExpectedIncome()
        {
            double income = calculateConfPaymentCurIncome();
            var rega = getActivatedRegBills();
            foreach (var b in rega)
            {
                income += b.amount;
            }
            var wdla = getActivatedWithdrawalBills();
            foreach (var b in wdla)
            {
                income -= b.amount;
            }
            return income;
        }
    }
}
