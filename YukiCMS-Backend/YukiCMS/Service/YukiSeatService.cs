using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiSeatService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiSeat> _seat;
        private readonly YukiGlobalService _globalService;
        public YukiSeatService(IYukiDatabaseSettings settings, YukiGlobalService globalService)
        {
            _seat = getMongoCollection<YukiSeat>(settings.seatColleName, settings);
            _globalService = globalService;
        }

        public int create(YukiSeat seat)
        {
            seat.sid = _globalService.nextSid();
            _seat.InsertOne(seat);
            return seat.sid;
        }
        public YukiSeat get(Expression<Func<YukiSeat, bool>> filter) =>
            _seat.Find(filter).FirstOrDefault();
        public List<YukiSeat> getList(Expression<Func<YukiSeat, bool>> filter) =>
            _seat.Find(filter).ToList();
        public bool update<TResult>(Expression<Func<YukiSeat, bool>> filter,
                    Expression<Func<YukiSeat, TResult>> updatedProperty,
                    TResult value
            ) =>
            _seat.UpdateOne(
                filter,
                Builders<YukiSeat>.Update.Set(updatedProperty, value)
            ).ModifiedCount > 0;
        public bool replace(Expression<Func<YukiSeat, bool>> filter, YukiSeat seat) =>
            _seat.ReplaceOne(filter, seat).ModifiedCount > 0;

        public bool delete(Expression<Func<YukiSeat, bool>> filter) =>
            _seat.DeleteOne(filter).DeletedCount > 0;


        public YukiSeat getBySid(int sid) =>
            get(seat => seat.sid == sid);
        public YukiSeat getByUC(int cid, int uid) =>
            get(seat => seat.cid == cid && seat.uid == uid && seat.status == YukiSeatStatus.assigned);

        public List<YukiSeat> getSeatsByCid(int cid) =>
            getList(seat => seat.cid == cid);
        public List<YukiSeat> getSeatsByCidAndStatus(int cid, YukiSeatStatus status) =>
            getList(seat => seat.cid == cid && seat.status == status);
        public List<YukiSeat> getSeatByUid(int uid) =>
            getList(seat => seat.uid == uid && seat.status == YukiSeatStatus.assigned);

        public bool assignSeat(int sid,int uid)
        {
            var seat=getBySid(sid);
            if (seat == null) return false;
            if (seat.status == YukiSeatStatus.assigned) return false;
            if (getByUC(seat.cid, uid) != null) return false;
            seat.status = YukiSeatStatus.assigned;
            seat.uid = uid;
            return updateSeat(sid, seat);
        }
        public bool unassignSeat(int sid, int uid) =>
            update(seat => seat.sid == sid && seat.uid == uid && seat.status == YukiSeatStatus.assigned, s => s.status, YukiSeatStatus.unassigned);
        public bool updateSeat(int sid, YukiSeat seat) =>
            replace(s=>s.sid==sid, seat);
        public bool deleteSeat(int sid) =>
            delete(seat => seat.sid == sid);
    }
}
