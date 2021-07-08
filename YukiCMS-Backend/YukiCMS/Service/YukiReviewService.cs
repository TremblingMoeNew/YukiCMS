using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiReviewService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiReview> _review;
        private readonly YukiGlobalService _globalService;
        private readonly YukiReviewTemplateService _templateService;
        private readonly YukiQuestionService _questionService;
        public YukiReviewService(IYukiDatabaseSettings settings, YukiGlobalService globalService,
                                    YukiReviewTemplateService template, YukiQuestionService question)
        {
            _review = getMongoCollection<YukiReview>(settings.taskColleName, settings);
            _globalService = globalService;
            _templateService = template;
            _questionService = question;
        }

        public void create(YukiReview review)
        {
            _review.InsertOne(review);
        }
        public YukiReview get(Expression<Func<YukiReview, bool>> filter) =>
            _review.Find(filter).FirstOrDefault();
        public List<YukiReview> getList(Expression<Func<YukiReview, bool>> filter) =>
            _review.Find(filter).ToList();
        public void update<TResult>(Expression<Func<YukiReview, bool>> filter,
                    Expression<Func<YukiReview, TResult>> updatedProperty,
                    TResult value
            ) =>
            _review.UpdateOne(
                filter,
                Builders<YukiReview>.Update.Set(updatedProperty, value)
            );
        public void replace(Expression<Func<YukiReview, bool>> filter, YukiReview review) =>
            _review.ReplaceOne(filter, review);

        public void delete(Expression<Func<YukiReview, bool>> filter) =>
            _review.DeleteOne(filter);

        public YukiReview getByTid(int tid) =>
            get(review => review.tid == tid);

        public List<YukiReview> getReviewsByCid(int cid) =>
            getList(review => review.cid == cid);
        public List<YukiReview> getReviewsByUid(int uid) =>
            getList(review => review.uid == uid);
        public List<YukiReview> getReviewsByAdmUid(int admuid) =>
            getList(review => review.admUid == admuid);
        public List<YukiReview> getReviewsByStatus(YukiReviewStatus status) =>
            getList(review => review.status == status);
        public List<YukiReview> getReviewsWithoutAdm() =>
            getList(review => review.admUid == 0);
        public List<YukiReview> getReviewsByUC(int uid, int cid) =>
            getList(review => review.uid == uid && review.cid == cid);
        public List<YukiReview> getReviewsByCA(int cid, int admuid) =>
            getList(review => review.cid == cid & review.admUid == admuid);
        public List<YukiReview> getActiveReviewsByUid(int uid) =>
            getList(review => review.uid == uid && review.status != YukiReviewStatus.cancelled && review.status != YukiReviewStatus.completed);

        public List<YukiReview> getActiveReviewsByUC(int uid, int cid) =>
           getList(review => review.uid == uid && review.cid == cid && review.status != YukiReviewStatus.cancelled && review.status != YukiReviewStatus.completed);
        public List<YukiReview> getActiveReviewsByCA(int cid, int admuid) =>
            getList(review => review.cid == cid & review.admUid == admuid && review.status != YukiReviewStatus.cancelled && review.status != YukiReviewStatus.completed);
        public List<YukiReview> getActiveReviewsByCAS(int cid, int admuid, YukiReviewStatus status) =>
            getList(review => review.cid == cid & review.admUid == admuid && review.status != YukiReviewStatus.cancelled && review.status != YukiReviewStatus.completed && review.status == status);
        public List<YukiReview> GetReviewsByCS(int cid, YukiReviewStatus status) =>
            getList(review => review.cid == cid && review.status == status);
        public List<YukiReview> getReviewsByCidWOAdm(int cid) =>
            getList(review => review.cid == cid && review.admUid == 0);
        public List<YukiReview> getActiveReviewsByCidWOAdm(int cid) =>
            getList(review => review.cid == cid && review.admUid == 0 && review.status != YukiReviewStatus.cancelled && review.status != YukiReviewStatus.completed);

        public void expireReviews(DateTime now) =>
            _review.UpdateMany(
                review => review.status == YukiReviewStatus.incompleted && review.ddl < now,
                Builders<YukiReview>.Update.Set(r => r.status, YukiReviewStatus.expired)
            );

        public void updateAdmUid(int tid, int admuid) =>
            update(review => review.tid == tid, r => r.admUid, admuid);
        public void updateStatus(int tid, YukiReviewStatus status) =>
            update(review => review.tid == tid, r => r.status, status);
        public void updateCommment(int tid, string comment) =>
           update(review => review.tid == tid, r => r.comment, comment);
        public void cancelByUC(int uid, int cid) =>
            update(
                review => review.uid == uid && review.cid == cid && review.status != YukiReviewStatus.completed, 
                r => r.status, 
                YukiReviewStatus.cancelled
            );
        public void cancelByUid(int uid)=>
            update(
                review => review.uid == uid  && review.status != YukiReviewStatus.completed,
                r => r.status,
                YukiReviewStatus.cancelled
            );
        public void submit(int tid) =>
            updateStatus(tid, YukiReviewStatus.submitted);
        public void complete(int tid) =>
           updateStatus(tid, YukiReviewStatus.completed);
        public void expire(int tid) =>
          updateStatus(tid, YukiReviewStatus.expired);
        public void cancel(int tid) =>
            updateStatus(tid, YukiReviewStatus.cancelled);
        public YukiReview create(int uid,int cid,int admUid,DateTime startAt,int duration,List<String>questions)
        {
            var review = new YukiReview
            {
                tid = _globalService.nextTid(),
                uid = uid,
                cid = cid,
                admUid = admUid,
                startAt = startAt,
                ddl = startAt.AddDays(duration),
                questions = new List<int>()
            };
            foreach (var q in questions)
            {
                review.questions.Add(_questionService.create(uid, q));
            }
            review.status = YukiReviewStatus.incompleted;
            create(review);
            return review;
        }
        public YukiReview create(int uid, int cid, int admUid, int duration, List<String> questions) =>
            create(uid, cid, admUid, DateTime.Now, duration, questions);
        public YukiReview create(int uid, int cid, int duration, List<String> questions) =>
            create(uid, cid, 0, DateTime.Now, duration, questions);
        public YukiReview create(int uid, int cid, DateTime startAt,int duration, List<String> questions) =>
            create(uid, cid, 0, startAt, duration, questions);
        public YukiReview create(int uid, int admUid, DateTime startAt, YukiReviewTemplate template) =>
            create(uid, template.cid, admUid, startAt, template.duration, template.questions);
        public YukiReview create(int uid, int admUid, YukiReviewTemplate template) =>
            create(uid, template.cid, admUid, DateTime.Now, template.duration, template.questions);
        public YukiReview create(int uid, YukiReviewTemplate template) =>
            create(uid, template.cid, 0, DateTime.Now, template.duration, template.questions);
        public YukiReview create(int uid, int admUid, DateTime startAt, int templateid) =>
            create(uid, admUid, startAt, _templateService.getByTTid(templateid));
        public YukiReview create(int uid, int admUid, int templateid) =>
            create(uid, admUid, _templateService.getByTTid(templateid));
        public YukiReview create(int uid, int templateid) =>
            create(uid, _templateService.getByTTid(templateid));

        public void delete(int tid) =>
            delete(review => review.tid == tid);

        public bool isActive(YukiReviewStatus status) =>
            status != YukiReviewStatus.cancelled && status != YukiReviewStatus.completed;
    }
}
