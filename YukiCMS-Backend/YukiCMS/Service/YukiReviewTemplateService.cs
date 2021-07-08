using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiReviewTemplateService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiReviewTemplate> _template;
        private readonly YukiGlobalService _globalService;
        public YukiReviewTemplateService(IYukiDatabaseSettings settings, YukiGlobalService globalService)
        {
            _template = getMongoCollection<YukiReviewTemplate>(settings.taskTemplateColleName, settings);
            _globalService = globalService;
        }

        public int create(YukiReviewTemplate template)
        {
            template.ttid = _globalService.nextTTid();
            _template.InsertOne(template);
            return template.ttid;
        }
        public YukiReviewTemplate get(Expression<Func<YukiReviewTemplate, bool>> filter) =>
            _template.Find(filter).FirstOrDefault();
        public List<YukiReviewTemplate> getList(Expression<Func<YukiReviewTemplate, bool>> filter) =>
            _template.Find(filter).ToList();
        public void update<TResult>(Expression<Func<YukiReviewTemplate, bool>> filter,
                    Expression<Func<YukiReviewTemplate, TResult>> updatedProperty,
                    TResult value
            ) =>
            _template.UpdateOne(
                filter,
                Builders<YukiReviewTemplate>.Update.Set(updatedProperty, value)
            );
        public void replace(Expression<Func<YukiReviewTemplate, bool>> filter, YukiReviewTemplate template) =>
            _template.ReplaceOne(filter, template);

        public bool delete(Expression<Func<YukiReviewTemplate, bool>> filter) =>
            _template.DeleteOne(filter).DeletedCount > 0;

        public YukiReviewTemplate getByTTid(int ttid) =>
            get(tmpl => tmpl.ttid == ttid);
        public List<YukiReviewTemplate> getTemplatesByCid(int cid) =>
            getList(tmpl => tmpl.cid == cid);
        public List<String> getQuestions(int ttid) =>
            getByTTid(ttid).questions;

        public bool delete(int ttid) =>
            delete(tmpl => tmpl.ttid == ttid);

    }
}
