using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using YukiCMS.Models;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace YukiCMS.Service
{
    public class YukiUserService: YukiServiceBase
    {
        private readonly IMongoCollection<YukiUser> _users;
        private readonly YukiGlobalService _globalService;
        public YukiUserService(IYukiDatabaseSettings settings,YukiGlobalService globalService)
        {
            _users = getMongoCollection<YukiUser>(settings.userColleName, settings);
            _globalService = globalService;
        }

        public void create(YukiUser user)
        {
            _users.InsertOne(user);
        }
        public YukiUser get(Expression<Func<YukiUser, bool>> filter) =>
            _users.Find(filter).FirstOrDefault();
        public List<YukiUser> getList(Expression<Func<YukiUser, bool>> filter) =>
            _users.Find(filter).ToList();
        public bool update<TResult>(
                Expression<Func<YukiUser, bool>> filter, Expression<Func<YukiUser, TResult>> updatedProperty, TResult value) =>
            _users.UpdateOne(
                filter,
                Builders<YukiUser>.Update.Set(updatedProperty, value)
            ).ModifiedCount>0;
        public bool replace(Expression<Func<YukiUser, bool>>filter, YukiUser user) =>
            _users.ReplaceOne(filter, user).ModifiedCount>0;

        public bool delete(Expression<Func<YukiUser, bool>> filter) =>
            _users.DeleteOne(filter).DeletedCount>0;


        public YukiUser getByUid(int uid) =>
            get(user => user.uid == uid);
        public YukiUser getByEmail(string email) =>
            get(user => user.email == email);
        public YukiUser getByPhone(string phone) =>
            get(user => user.info.phoneNumber == phone);
        public YukiUser getByQQ(string qq) =>
            get(user => user.info.qqNumber == qq);
        public YukiUser getByWeChat(string wechat) =>
            get(user => user.info.wechatNumber == wechat);

        public List<YukiUser> getUsersByResidentIdType(YukiUserResidentIdType type) =>
            getList(user => user.info.residentIdType == type);
        public List<YukiUser> getUsersByRegPayment(bool paid) =>
            getList(user => user.isRegPaid == paid);
        public List<YukiUser> getUsersInListByRegPayment(bool paid, List<int> uids) =>
           getList(user => user.isRegPaid == paid && (uids.Contains(user.uid)));
        public List<YukiUser> getUsersNotInList(List<int> uids) =>
        getList(user => !uids.Contains(user.uid));
        public List<YukiUser> getUserByWithdrawal(bool wd) =>
            getList(user => user.isWithdrawaled == wd);
        public List<YukiUser> getUsersByGA(bool ga) =>
            getList(user => user.accommodation.isGA == ga);
        public List<YukiUser> getUsersByGAPaid(bool paid) =>
            getList(user => user.accommodation.isPaid == paid);
        public List<YukiUser> getUsersByProvince(string province) =>
            getList(user => user.info.province == province);
        public List<YukiUser> getUsersByCity(string city) =>
            getList(user => user.info.city == city);
        public List<YukiUser> getUsersBySchool(string school) =>
            getList(user => user.info.school == school);

        public YukiUserInfo getInfoByUid(int uid)
        {
            var usr = getByUid(uid);
            if (usr == null) return null;
            return usr.info;
        }

        public YukiUserAccInfo getAccInfoByUid(int uid)
        {
            var usr = getByUid(uid);
            if (usr == null) return null;
            return usr.accommodation;
        }
        public YukiUser login(string email,string password)
        {
            var user = getByEmail(email);
            if (user == null) return null;
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(user.password_salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            if (hashed == user.password_hash) return user; else return null;
        }
        public int create(string email,string password,YukiUserInfo info)
        {
            if (getByEmail(email) != null) return -1;
            YukiUser user = new YukiUser
            {
                email = email,
                uid = _globalService.nextUid(),
                info = info,
                accommodation = new YukiUserAccInfo()
            };
            user.accommodation.isGA = true;
            setPassword(user, password);
            create(user);
            return user.uid;
        }
        public bool resetPassword(int uid,string npassword)
        {
            var user = getByUid(uid);
            if (user == null) return false;
            setPassword(user, npassword);
            return replace((u => u.uid == uid), user);
        }

        public bool updateInfo(int uid, YukiUserInfo info) =>
            update((u => u.uid == uid), (user => user.info), info);
        public bool updateRegPaid(int uid, bool paid) =>                   //  NOTE: 由BillService处理、唯一调用
            update(u => u.uid == uid, user => user.isRegPaid, paid);

        public bool updateAccInfo(int uid, YukiUserAccInfo info) =>        //  NOTE: AccommodationService调用BillService处理GA变化
            update(u => u.uid == uid, user => user.accommodation, info);
        public bool updateAccPaid(int uid, bool paid) =>                   //  NOTE: 由BillService处理、唯一调用
            update(u => u.uid == uid, user => user.accommodation.isPaid, paid);
        public bool updateWD(int uid, bool wd) =>                          //  NOTE: 由BillService处理、唯一调用
             update(u => u.uid == uid, user => user.isWithdrawaled, wd);


        public void setPassword(YukiUser user,string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            user.password_salt = Convert.ToBase64String(salt);
            user.password_hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(user.password_salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

        }

        public bool deleteByUid(int uid) =>
            delete(user => user.uid == uid);
    }
}
