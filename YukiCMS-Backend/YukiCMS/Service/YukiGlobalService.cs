using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;
using YukiCMS.Service;
namespace YukiCMS.Service
{
    public class YukiGlobalService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiGlobal> _global;
        public YukiGlobalService(IYukiDatabaseSettings settings)
        {
            _global = getMongoCollection<YukiGlobal>(settings.globalColleName, settings);
        }
        public bool update<TResult>(Expression<Func<YukiGlobal, TResult>> updatedProperty, TResult value) =>
            _global.UpdateOne(
                g => true,
                Builders<YukiGlobal>.Update.Set(updatedProperty, value)
            ).ModifiedCount > 0;


        public YukiGlobalSettings getSettings() =>
            _global.Find(g => true).FirstOrDefault().settings;

        public bool updateSettings(YukiGlobalSettings settings) =>
            update(g => g.settings, settings);

        public YukiGlobalIdCounter getCounter() =>
            _global.Find(g => true).FirstOrDefault().counter;
        public int nextUid()
        {
            var id = getCounter().nextUid;
            update(g => g.counter.nextUid, id + 1);
            return id;
        }
        public int nextCid()
        {
            var id = getCounter().nextCid;
            update(g => g.counter.nextCid, id + 1);
            return id;
        }
        public int nextSid()
        {
            var id = getCounter().nextSid;
            update(g => g.counter.nextSid, id + 1);
            return id;
        }
        public int nextTid()
        {
            var id = getCounter().nextTid;
            update(g => g.counter.nextTid, id + 1);
            return id;
        }
        public int nextTTid()
        {
            var id = getCounter().nextTTid;
            update(g => g.counter.nextTTid, id + 1);
            return id;
        }
        public int nextQid()
        {
            var id = getCounter().nextQid;
            update(g => g.counter.nextQid, id + 1);
            return id;
        }
        public int nextPGid()
        {
            var id = getCounter().nextPGid;
            update(g => g.counter.nextPGid, id + 1);
            return id;
        }
        public int nextFGid()
        {
            var id = getCounter().nextFGid;
            update(g => g.counter.nextFGid, id + 1);
            return id;
        }
        public int nextBillId()
        {
            var id = getCounter().nextBillId;
            update(g => g.counter.nextBillId, id + 1);
            return id;
        }
        public int nextBudgetId()
        {
            var id = getCounter().nextBudgetId;
            update(g => g.counter.nextBudgetId, id + 1);
            return id;
        }
        public int nextACCAId()
        {
            var id = getCounter().nextACCAId;
            update(g => g.counter.nextACCAId, id + 1);
            return id;
        }
    }
}
