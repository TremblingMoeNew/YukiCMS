using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiFileGroupService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiFileGroup> _group;
        private readonly YukiGlobalService _globalService;
        private readonly YukiFileService _fileService;
        public YukiFileGroupService(IYukiDatabaseSettings settings, YukiGlobalService globalService, YukiFileService fileService)
        {
            _group = getMongoCollection<YukiFileGroup>(settings.fileGroupColleName, settings);
            _globalService = globalService;
            _fileService = fileService;
        }
        public void create(YukiFileGroup group)
        {
            _group.InsertOne(group);
        }
        public YukiFileGroup get(Expression<Func<YukiFileGroup, bool>> filter) =>
            _group.Find(filter).FirstOrDefault();
        public List<YukiFileGroup> getList(Expression<Func<YukiFileGroup, bool>> filter) =>
            _group.Find(filter).ToList();
        public bool update<TResult>(Expression<Func<YukiFileGroup, bool>> filter,
                    Expression<Func<YukiFileGroup, TResult>> updatedProperty,
                    TResult value
            ) =>
            _group.UpdateOne(
                filter,
                Builders<YukiFileGroup>.Update.Set(updatedProperty, value)
            ).ModifiedCount > 0;
        public bool replace(Expression<Func<YukiFileGroup, bool>> filter, YukiFileGroup group) =>
            _group.ReplaceOne(filter, group).ModifiedCount > 0;

        public bool delete(Expression<Func<YukiFileGroup, bool>> filter) =>
            _group.DeleteOne(filter).DeletedCount > 0;

        public int create(string name)
        {
            var fg = new YukiFileGroup
            {
                fgid = _globalService.nextFGid(),
                name = name,
                files = new List<string>()
            };
            create(fg);
            return fg.fgid;
        }
        public YukiFileGroup getByFGid(int fgid) =>
            get(grp => grp.fgid == fgid);
        public bool addFile(int fgid, string fileStorageName)
        {
            var grp = getByFGid(fgid);
            if (grp == null) return false;
            grp.files.Add(fileStorageName);
            if (!_fileService.increaseQuote(fileStorageName)) return false;
            return replace(g => g.fgid == fgid, grp);
        }
        public bool deleteFile(int fgid, string fileStorageName)
        {
            var grp = getByFGid(fgid);
            if (grp == null) return false;
            if (!grp.files.Remove(fileStorageName)) return false;
            if (!_fileService.decreaseQuote(fileStorageName)) return false;
            return replace(g => g.fgid == fgid, grp);
        }
        public bool editGroupName(int fgid, string name) =>
            update(group => group.fgid == fgid, grp => grp.name, name);

    }
}
