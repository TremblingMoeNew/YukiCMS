using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiQuestionService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiQuestion> _question;
        private readonly YukiGlobalService _globalService;
        public YukiQuestionService(IYukiDatabaseSettings settings, YukiGlobalService globalService)
        {
            _question = getMongoCollection<YukiQuestion>(settings.questionColleName, settings);
            _globalService = globalService;
        }

        public void create(YukiQuestion question)
        {
            _question.InsertOne(question);
        }
        public YukiQuestion get(Expression<Func<YukiQuestion, bool>> filter) =>
            _question.Find(filter).FirstOrDefault();
        public List<YukiQuestion> getList(Expression<Func<YukiQuestion, bool>> filter) =>
            _question.Find(filter).ToList();
        public bool update<TResult>(Expression<Func<YukiQuestion, bool>> filter,
                    Expression<Func<YukiQuestion, TResult>> updatedProperty,
                    TResult value
            ) =>
            _question.UpdateOne(
                filter,
                Builders<YukiQuestion>.Update.Set(updatedProperty, value)
            ).ModifiedCount > 0;
        public bool replace(Expression<Func<YukiQuestion, bool>> filter, YukiQuestion question) =>
            _question.ReplaceOne(filter, question).ModifiedCount > 0;

        public bool delete(Expression<Func<YukiQuestion, bool>> filter) =>
            _question.DeleteOne(filter).DeletedCount > 0;

        public YukiQuestion getByQid(int qid) =>
            get(question => question.qid == qid);

        public bool updateAnswer(int qid, string answer) =>
            update(question => question.qid == qid, q => q.answer, answer);

        public bool updateAttachment(int qid, string attachment) =>
            update(question => question.qid == qid, q => q.attachment, attachment);

        public int create(int uid,string question)
        {
            YukiQuestion q = new YukiQuestion
            {
                uid = uid,
                question = question,
                qid = _globalService.nextQid()
            };
            create(q);
            return q.qid;
        }
    }
}
