using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YukiCMS.Models;
using YukiCMS.Service;
using YukiCMS.Models.PostRequest;
using YukiCMS.Models.Response;
namespace YukiCMS.Controllers
{
    [Authorize]
    [Route("api/review")]
    [ApiController]
    public class YukiReviewController : ControllerBase
    {
        private readonly YukiReviewService _service;
        private readonly YukiReviewTemplateService _templateService;
        private readonly YukiQuestionService _questionService;
        private readonly YukiPermissionService _permissionService;
        private readonly YukiCommitteeService _committeeService;
        private readonly YukiFileService _fileService;
        private readonly YukiEmailService _emailService;
        private readonly YukiUserService _userService;
        public YukiReviewController(
                YukiReviewService service,
                YukiReviewTemplateService templateService,
                YukiQuestionService questionService,
                YukiPermissionService permissionService,
                YukiCommitteeService committeeService,
                YukiFileService fileService,
                YukiEmailService emailService,
                YukiUserService userService
            )
        {
            _service = service;
            _templateService = templateService;
            _questionService = questionService;
            _permissionService = permissionService;
            _committeeService = committeeService;
            _fileService = fileService;
            _emailService = emailService;
            _userService = userService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        [HttpGet]
        public ActionResult<List<YukiReview>> getReviews()
        {
            int actionUid = getUserId();
            var ls = _service.getReviewsByUid(actionUid);
            foreach (var r in ls)
            {
                r.comment = null;
            }
            ls.Reverse();
            return ls;
        }
        [HttpGet("c{cid:int}/self")]
        public ActionResult<List<YukiReviewResponse>> getSelfReviewsInCommittee(int cid)
        {
            int actionUid = getUserId();
            var ls = _service.getReviewsByUC(actionUid, cid);
            foreach (var r in ls)
            {
                r.comment = null;
            }
            ls.Reverse();
            var res = new List<YukiReviewResponse>();
            foreach (var r in ls)
            {
                var qs = new List<string>();
                foreach (var qi in r.questions)
                {
                    var q = _questionService.getByQid(qi);
                    if (q != null) qs.Add(q.question);
                }
                res.Add(new YukiReviewResponse
                {
                    tid = r.tid,
                    uid = r.uid,
                    cid = r.cid,
                    admUid = r.admUid,
                    uname = _userService.getByUid(r.uid)?.info.name,
                    admName = _userService.getByUid(r.admUid)?.info.name,
                    comment = r.comment,
                    questions = qs,
                    ddl = r.ddl,
                    startAt = r.startAt,
                    status = r.status
                });
            }
            return res;
        }
        [HttpGet("c{cid:int}/self/active")]
        public ActionResult<List<YukiReviewResponse>> getActiveReviews(int cid)
        {
            int actionUid = getUserId();
            var ls = _service.getActiveReviewsByUC(actionUid,cid);
            foreach (var r in ls)
            {
                r.comment = null;
            }
            ls.Reverse();
            var res = new List<YukiReviewResponse>();
            foreach (var r in ls)
            {
                var qs = new List<string>();
                foreach (var qi in r.questions)
                {
                    var q = _questionService.getByQid(qi);
                    if (q != null) qs.Add(q.question);
                }
                res.Add(new YukiReviewResponse
                {
                    tid = r.tid,
                    uid = r.uid,
                    cid = r.cid,
                    admUid = r.admUid,
                    uname = _userService.getByUid(r.uid)?.info.name,
                    admName = _userService.getByUid(r.admUid)?.info.name,
                    comment = r.comment,
                    questions = qs,
                    ddl = r.ddl,
                    startAt = r.startAt,
                    status = r.status
                });
            }
            return res; ;
        }
        [HttpGet("c{cid:int}/archived")]
        public ActionResult<List<YukiReviewResponse>> getCommitteeReviews(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, cid))
                return Unauthorized();
            var ls = _service.getReviewsByCid(cid);
            var res = new List<YukiReviewResponse>();
            foreach (var r in ls)
            {
                var qs = new List<string>();
                foreach (var qi in r.questions)
                {
                    var q = _questionService.getByQid(qi);
                    if (q != null) qs.Add(q.question);
                }
                res.Add(new YukiReviewResponse
                {
                    tid = r.tid,
                    uid = r.uid,
                    cid = r.cid,
                    admUid = r.admUid,
                    uname = _userService.getByUid(r.uid)?.info.name,
                    admName = _userService.getByUid(r.admUid)?.info.name,
                    comment = r.comment,
                    questions = qs,
                    ddl = r.ddl,
                    startAt = r.startAt,
                    status = r.status
                });
            }
            return res;
        }

        [HttpGet("c{cid:int}")]
        public ActionResult<List<YukiReviewResponse>> getReviewsAsReviewer(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, cid))
                return Unauthorized();
            var ls = _service.getReviewsByCA(cid, actionUid);
            var res = new List<YukiReviewResponse>();
            foreach(var r in ls)
            {
                var qs = new List<string>();
                foreach(var qi in r.questions)
                {
                    var q = _questionService.getByQid(qi);
                    if (q != null) qs.Add(q.question);
                }
                res.Add(new YukiReviewResponse
                {
                    tid = r.tid,
                    uid = r.uid,
                    cid = r.cid,
                    admUid = r.admUid,
                    uname = _userService.getByUid(r.uid)?.info.name,
                    admName = _userService.getByUid(r.admUid)?.info.name,
                    comment = r.comment,
                    questions = qs,
                    ddl = r.ddl,
                    startAt = r.startAt,
                    status = r.status
                });
            }
            return res;
        }

        [HttpGet("c{cid:int}/general")]
        public ActionResult<List<YukiReviewResponse>> getCommitteeGeneralReviews(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, cid))
                return Unauthorized();
            var ls = _service.getReviewsByCidWOAdm(cid);
            var res = new List<YukiReviewResponse>();
            foreach (var r in ls)
            {
                var qs = new List<string>();
                foreach (var qi in r.questions)
                {
                    var q = _questionService.getByQid(qi);
                    if (q != null) qs.Add(q.question);
                }
                res.Add(new YukiReviewResponse
                {
                    tid = r.tid,
                    uid = r.uid,
                    cid = r.cid,
                    admUid = r.admUid,
                    uname = _userService.getByUid(r.uid)?.info.name,
                    admName = _userService.getByUid(r.admUid)?.info.name,
                    comment = r.comment,
                    questions = qs,
                    ddl = r.ddl,
                    startAt = r.startAt,
                    status = r.status
                });
            }
            return res;
        }

        [HttpGet("{tid:int}", Name = "GetReview")]
        public ActionResult<YukiReview> getReview(int tid)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, review.cid)
                && review.uid != actionUid && review.admUid != actionUid) return Unauthorized();
            if (review.uid != actionUid) review.comment = null;
            return review;
        }

        [HttpGet("c{cid:int}/count")]
        public IActionResult getReviewsCount(int cid)
        {
            int actionUid = getUserId();
            int reviewsByAdmActive = _service.getActiveReviewsByCA(cid, actionUid).Count;
            int reviewsByAdmCompleted = _service.getActiveReviewsByCAS(cid, actionUid, YukiReviewStatus.completed).Count;
            int reviewsGeneralActive = 0;
            int reviewsTotal = 0;
            if (_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, cid))
            {
                reviewsGeneralActive = _service.getActiveReviewsByCidWOAdm(cid).Count;
                reviewsTotal = _service.getReviewsByCid(cid).Count;
            }
            return new JsonResult(new { reviewsByAdmActive, reviewsByAdmCompleted, reviewsGeneralActive, reviewsTotal });
        }


        [HttpPost("{tid:int}")]
        public IActionResult submitReview(int tid)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (review.uid != actionUid) return Unauthorized();
            _service.submit(tid);
            return NoContent();
        }
        [HttpPut("{tid:int}")]
        public IActionResult completeReview(int tid)   // Problem: Can't get comment
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!(_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, review.cid)
                && review.admUid == 0) && review.admUid != actionUid) return Unauthorized();
            _service.complete(tid);
            return NoContent();
        }
        [HttpPut("{tid:int}/comment")]
        public IActionResult commentReview(int tid, [FromBody]string comment)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, review.cid)
                && review.admUid != actionUid) return Unauthorized();
            _service.updateCommment(tid,comment);
            return NoContent();
        }
        [HttpDelete("{tid:int}")]
        public IActionResult cancelReview(int tid)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Cancel_Reviews, review.cid))
                return Unauthorized();
            _service.cancel(tid);
            return NoContent();
        }
        [HttpPost("c{cid:int}/u{uid:int}")]
        public IActionResult createReview(int cid, int uid, YukiReviewRequest request)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Reviews, cid))
                return Unauthorized();
            if (!_permissionService.vertifyPermission(request.admUid, YukiPermissionType.Committee_General_Reviewer, cid))
                return Unauthorized();
            var com = _committeeService.getByCid(cid);
            if (com == null) return NotFound();
            var usr = _userService.getByUid(uid);
            if (usr == null) return NotFound();
            YukiReviewTemplate tmpl = _templateService.getByTTid(request.ttid);
            if (tmpl == null) tmpl = new YukiReviewTemplate
            {
                cid = cid,
                duration = request.duration,
                questions = request.questions
            };
            YukiReview review;
            if (request.startAt == DateTime.MinValue) review = _service.create(uid, request.admUid, tmpl);
            else review = _service.create(uid, request.admUid, request.startAt, tmpl);
            _emailService.sendRevieweeNotification(uid, com.name);
            _emailService.sendReviewerNotification(request.admUid, com.name, usr.info.name);
            return CreatedAtRoute("GetReview", new { review.tid }, review);
        }
        [HttpGet("c{cid:int}/reviewer")]
        public ActionResult<List<YukiUserNameResponse>> getCommitteeReviewersList(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Reviews, cid))
                return Unauthorized();
            var pls = _permissionService.getPermissionsByTO(YukiPermissionType.Committee_General_Reviewer, cid);
            var res = new List<YukiUserNameResponse>();
            foreach (var p in pls)
            {
                var u = _userService.getByUid(p.uid);
                if (u == null) continue;
                res.Add(new YukiUserNameResponse
                {
                    uid = u.uid,
                    name = u.info.name,
                    email = u.email
                });
            }
            return res;
        }
        [HttpGet("{tid:int}/reviewer")]
        public ActionResult<YukiUserBriefResponse> getReviewerBrief(int tid)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, review.cid)
                && review.uid != actionUid && review.admUid != actionUid) return Unauthorized();
            var adm = _userService.getByUid(review.admUid);
            if (adm == null) return NotFound();
            return new YukiUserBriefResponse
            {
                uid = adm.uid,
                name = adm.info.name,
                email = adm.email,
                sex = adm.info.sex,
                phoneNumber = adm.info.phoneNumber,
                qqNumber = adm.info.qqNumber,
                wechatNumber = adm.info.wechatNumber,
                school = adm.info.school
            };
        }
        [HttpGet("{tid:int}/reviewee")]
        public ActionResult<YukiUserBriefResponse> getRevieweeBrief(int tid)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, review.cid)
                && review.uid != actionUid && review.admUid != actionUid) return Unauthorized();
            var usr = _userService.getByUid(review.uid);
            if (usr == null) return NotFound();
            return new YukiUserBriefResponse
            {
                uid = usr.uid,
                name = usr.info.name,
                email = usr.email,
                sex = usr.info.sex,
                phoneNumber = usr.info.phoneNumber,
                qqNumber = usr.info.qqNumber,
                wechatNumber = usr.info.wechatNumber,
                school = usr.info.school
            };
        }


        [HttpGet("{tid:int}/question/{qid:int}")]
        public ActionResult<YukiQuestion> getQuestion(int tid, int qid)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, review.cid)
                && review.uid != actionUid && review.admUid != actionUid) return Unauthorized();
            if (!review.questions.Contains(qid)) return NotFound();
            var q = _questionService.getByQid(qid);
            if (q == null) return NotFound();
            return q;
        }
        [HttpPost("question/{qid:int}/answer")]
        public IActionResult autosaveAnswer(int qid, [FromBody] string answer)
        {
            int actionUid = getUserId();
            var question = _questionService.getByQid(qid);
            if (question == null) return NotFound();
            if (question.uid != actionUid) return Unauthorized();

            if (_questionService.updateAnswer(qid, answer)) return NoContent(); else return NotFound();
        }
        [HttpPost("{tid:int}/question/{qid:int}/attachment")]
        public IActionResult autosaveAttachment(int tid, int qid, IFormFile file)
        { 
            int actionUid = getUserId();
            var question = _questionService.getByQid(qid);
            if (question == null) return NotFound();
            if (question.uid != actionUid) return Unauthorized();

            string nname = _fileService.create(file);
            if (question.attachment != null) _fileService.decreaseQuote(question.attachment);
            _fileService.increaseQuote(nname);
            _questionService.updateAttachment(qid, nname);
            return new JsonResult(new { name = nname });
        }
        [HttpGet("{tid:int}/question/{qid:int}/attachment")]
        public IActionResult downloadAttachment(int tid, int qid)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, review.cid)
                && review.uid != actionUid && review.admUid != actionUid) return Unauthorized();
            if (!review.questions.Contains(qid)) return NotFound();
            var question = _questionService.getByQid(qid);
            if (question == null) return NotFound();;
            var file = _fileService.getFile(question.attachment);
            if (file == null) return NotFound();
            return File(file.blob, file.fileContentType, file.fileOriginalName);
        }
        [HttpGet("{tid:int}/question/{qid:int}/attachment/name")]
        public IActionResult getAttachmentOriginalName(int tid,int qid)
        {
            int actionUid = getUserId();
            var review = _service.getByTid(tid);
            if (review == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, review.cid)
                && review.uid != actionUid && review.admUid != actionUid) return Unauthorized();
            if (!review.questions.Contains(qid)) return NotFound();
            var question = _questionService.getByQid(qid);
            if (question == null) return NotFound(); ;
            var fileOriginalName = _fileService.getFile(question.attachment).fileOriginalName;
            return new JsonResult(new { fileOriginalName });
        }
        [HttpGet("template/{ttid:int}",Name ="GetReviewTemplate")]
        public ActionResult<YukiReviewTemplate> getTemplate(int ttid)
        {
            int actionUid = getUserId();
            var template = _templateService.getByTTid(ttid);
            if (template == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Reviews, template.cid))
                return Unauthorized();
            return template;
        }
        [HttpDelete("template/{ttid:int}")]
        public IActionResult deleteTemplate(int ttid)
        {
            int actionUid = getUserId();
            var template = _templateService.getByTTid(ttid);
            if (template == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Reviews, template.cid))
                return Unauthorized();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Push_Reviews, template.cid))
                return Unauthorized();
            _committeeService.removeAutoPushedTask(template.cid, ttid);
            if (_templateService.delete(ttid)) return NoContent();
            else return NotFound();
        }
        [HttpGet("template/c{cid:int}")]
        public ActionResult<List<YukiReviewTemplate>>getTemplates(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Reviews, cid))
                return Unauthorized();
            return _templateService.getTemplatesByCid(cid);
        }
        [HttpPost("template/c{cid:int}")]
        public IActionResult createTemplate(int cid,YukiReviewTemplate template)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Review_Template, cid))
                return Unauthorized();
            int ttid=_templateService.create(template);
            return CreatedAtRoute("GetReviewTemplate", new { ttid }, template);
        }
        [HttpPost("template/{ttid:int}/push")]
        public IActionResult pushTemplate(int ttid)
        {
            int actionUid = getUserId();
            var template = _templateService.getByTTid(ttid);
            if (template == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Push_Reviews, template.cid))
                return Unauthorized();
            if (_committeeService.addAutoPushedTask(template.cid, ttid)) return NoContent();
            else return NotFound();
        }
        [HttpDelete("template/{ttid:int}/push")]
        public IActionResult unpushTemplate(int ttid)
        {
            int actionUid = getUserId();
            var template = _templateService.getByTTid(ttid);
            if (template == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Push_Reviews, template.cid))
                return Unauthorized();
            if (_committeeService.removeAutoPushedTask(template.cid, ttid)) return NoContent();
            else return NotFound();
        }
    }
}