using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YukiCMS.Models;
using YukiCMS.Models.PostRequest;
using YukiCMS.Models.Response;
using YukiCMS.Service;
using YukiCMS.Service.JWT;
namespace YukiCMS.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class YukiUserController : ControllerBase
    {
        private readonly YukiUserService _service;
        private readonly JwtTokenGenService _tokenService;
        private readonly YukiPermissionService _permissionService;
        private readonly YukiCommitteeService _committeeService;
        private readonly YukiPaymentService _paymentService;
        private readonly YukiAccommodationService _accommodationService;
        private readonly YukiPermissionGroupService _pgService;
        private readonly YukiReviewService _reviewService;
        private readonly YukiSeatService _seatService;
        private readonly YukiEmailService _emailService;
        private readonly YukiFindPasswordTokenService _findPasswordService;
        public YukiUserController(
                YukiUserService service,
                JwtTokenGenService tokenService,
                YukiPermissionService permissionService,
                YukiCommitteeService committeeService,
                YukiPaymentService paymentService,
                YukiAccommodationService accommodationService,
                YukiPermissionGroupService pgService,
                YukiReviewService reviewService,
                YukiSeatService seatService,
                YukiEmailService emailService,
                YukiFindPasswordTokenService findPasswordService
            )
        {
            _service = service;
            _tokenService = tokenService;
            _permissionService = permissionService;
            _committeeService = committeeService;
            _paymentService = paymentService;
            _accommodationService = accommodationService;
            _pgService = pgService;
            _reviewService = reviewService;
            _seatService = seatService;
            _emailService = emailService;
            _findPasswordService = findPasswordService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        [HttpPost("login")]
        public IActionResult login(YukiUserLoginRequest ur)
        {
            var user=_service.login(ur.email, ur.password);
            if (user == null) return new JsonResult(new { success = false });
            return new JsonResult(new { success = true, token = _tokenService.CreateToken(user.uid) });
        }
        [HttpPost]
        public IActionResult register(YukiUserRegisterRequest ur)
        {
            var uid = _service.create(ur.email, ur.password, ur.info);
            // TODO: Resident ID Calculate
            if (uid == -1) return new JsonResult(new { success = false, emailconflict = true });
            try
            {
                _emailService.sendRegisterNotification(uid);
            }
            catch (SmtpCommandException e)
            {
                _service.deleteByUid(uid);
                return new JsonResult(new { success = false, notifEmailSendFailed = true });
            }
            return new JsonResult(new { success = true, token = _tokenService.CreateToken(uid) });
        }

        [Authorize]
        [HttpGet]
        public ActionResult<List<YukiUserDetailedResponse>>getUsers()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Get_Users, 0))
                return Unauthorized();
            var usrs=_service.getList(u => true);
            var res = new List<YukiUserDetailedResponse>();
            foreach(var u in usrs)
            {
                res.Add(new YukiUserDetailedResponse
                {
                    uid = u.uid,
                    name = u.info.name,
                    email = u.email,
                    school = u.info.school,
                    sex = u.info.sex,
                    isinSchoolGroup = u.info.isinSchoolGroup,
                    isRegPaid = u.isRegPaid,
                    isGA = u.accommodation.isGA,
                    isWithdrawaled = u.isWithdrawaled
                });
            }
            return res;
        }

        [Authorize]
        [HttpGet("info")]
        public ActionResult<YukiUserInfo> getSelfInfo()
        {
            int actionUid = getUserId();
            return _service.getInfoByUid(actionUid);
        }
        [Authorize]
        [HttpGet("info/u{uid:int}")]
        public ActionResult<YukiUserInfo> getUserInfo(int uid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Get_User_Info, 0))
                return Unauthorized(); 
            return _service.getInfoByUid(uid);
        }
        [Authorize]
        [HttpPut("info/u{uid:int}")]
        public IActionResult updateUserInfo(int uid,YukiUserInfo info)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid,YukiPermissionType.Global_Edit_User_Info, 0))
                return Unauthorized(); 
            if (_service.updateInfo(uid, info)) return Ok();
            else return NotFound();
        }
        [Authorize]
        [HttpPut("info")]
        public IActionResult updateSelfInfo(YukiUserInfo info)
        {
            int actionUid = getUserId();
            if (_service.updateInfo(actionUid, info)) return Ok();
            else return NotFound();
        }

        [Authorize]
        [HttpGet("home/general")]
        public ActionResult<YukiUserGeneralResponse> getHomePageGeneralInfo()
        {
            int actionUid = getUserId();
            var usr = _service.getByUid(actionUid);
            var coms = _committeeService.getCommitteesByMember(actionUid);
            var revs = _reviewService.getActiveReviewsByUid(actionUid);
            var seats = _seatService.getSeatByUid(actionUid);
            var acca = _accommodationService.getByUid(actionUid);
            if (usr == null) return NotFound();
            return new YukiUserGeneralResponse
            {
                uid = usr.uid,
                isRegPaid = usr.isRegPaid,
                isGA = usr.accommodation.isGA,
                comCount = coms.Count,
                activeReviewsCount = revs.Count,
                seatsCount = seats.Count,
                isAccAssigned = (acca != null),
                accExtendedDays = usr.accommodation.aheadDays + usr.accommodation.extendDays,
                isWithdrawed = usr.isWithdrawaled
            };
        }


        // TODO: Resident ID Check


        [Authorize]
        [HttpPut("password")]
        public IActionResult changePassword(YukiUserChangePasswordRequest r)
        {
            int actionUid = getUserId();
            if (_service.resetPassword(actionUid, r.npassword)) return Ok();
            else return NotFound();
        }



        // TODO: Forget Password
        [HttpPost("forgetpassword")]
        public IActionResult forgetPassword(YukiUserLoginRequest request)
        {
            var usr = _service.getByEmail(request.email);
            if (usr == null) return NoContent();
            var token = _findPasswordService.create(usr.uid);
            _emailService.sendResetPassNotification(usr.uid, token);
            return NoContent();
        }
        [HttpGet("forgetpassword/{token}")]
        public IActionResult vertifyToken(string token)
        {
            if (_findPasswordService.checkUid(token) > -1) return NoContent(); else return NotFound();
        }
        [HttpPost("forgetpassword/reset")]
        public IActionResult resetPasswordByToken(YukiUserResetPasswordRequest request)
        {
            int uid = _findPasswordService.checkUid(request.token);
            if (uid == -1) return NotFound();

            if (_service.resetPassword(uid, request.npassword))
            {
                _findPasswordService.invalidateToken(request.token);
                return NoContent();
            }
            else return NotFound();
        }



        [Authorize]
        [HttpGet("name")]
        public ActionResult<YukiUserNameResponse>getUserName()
        {
            int actionUid = getUserId();
            var usr = _service.getByUid(actionUid);
            if (usr == null) return Unauthorized();
            return new YukiUserNameResponse
            {
                uid = usr.uid,
                name = usr.info.name,
                email = usr.email
            };
        }
        [Authorize]
        [HttpGet("name/{uid:int}")]
        public ActionResult<YukiUserNameResponse> getUserNameByUid(int uid)
        {
            var usr = _service.getByUid(uid);
            if (usr == null) return Unauthorized();
            return new YukiUserNameResponse
            {
                uid = usr.uid,
                name = usr.info.name,
                email = usr.email
            };
        }
        [Authorize]
        [HttpGet("paymentStatus")]
        public ActionResult<YukiUserPaymentStatusResponse> getSelfUserPaymentStatus()
        {
            var actionUid = getUserId();;
            var usr = _service.getByUid(actionUid);
            return new YukiUserPaymentStatusResponse
            {
                uid = usr.uid,
                accommodation = usr.accommodation,
                isRegPaid = usr.isRegPaid,
                isWithdrawaled = usr.isWithdrawaled,
            };
        }
        [Authorize]
        [HttpGet("paymentStatus/{uid:int}")]
        public ActionResult<YukiUserPaymentStatusResponse> getUserPaymentStatus(int uid)
        {
            var actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Get_User_Info, 0))
                return Forbid();
            var usr = _service.getByUid(uid);
            return new YukiUserPaymentStatusResponse{
                uid = usr.uid,
                accommodation = usr.accommodation,
                isRegPaid = usr.isRegPaid,
                isWithdrawaled = usr.isWithdrawaled,
            };
        }
        [Authorize]
        [HttpGet("committee")]
        public ActionResult<List<YukiCommitteeBriefResponse>> getCommitteesSelfAsMember()
        {
            var actionUid = getUserId();
            var coms = _committeeService.getCommitteesByMember(actionUid);
            if (coms == null) return NotFound();
            var res = new List<YukiCommitteeBriefResponse>();
            foreach (var c in coms)
            {
                var resp = new YukiCommitteeBriefResponse
                {
                    cid = c.cid,
                    name = c.name,
                    ctype = c.ctype,
                    cdesc = c.cdesc,
                    applyDDL = c.applyDDL
                };
                res.Add(resp);
            }
            return res;
        }
        [Authorize]
        [HttpGet("committee/{uid:int}")]
        public ActionResult<List<YukiCommitteeBriefResponse>>getCommitteesAsMember(int uid)
        {
            var actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Get_Users, 0))
                return Unauthorized();
            var coms = _committeeService.getCommitteesByMember(uid);
            if (coms == null) return NotFound();
            var res = new List<YukiCommitteeBriefResponse>();
            foreach (var c in coms)
            {
                var resp = new YukiCommitteeBriefResponse
                {
                    cid = c.cid,
                    name = c.name,
                    ctype = c.ctype,
                    cdesc = c.cdesc,
                    applyDDL = c.applyDDL
                };
                res.Add(resp);
            }
            return res;
        }
        [Authorize]
        [HttpPut("acc")]
        public IActionResult updateSelfAccInfo(YukiUserAccInfo info)
        {
            var actionUid = getUserId();
            var usr = _service.getByUid(actionUid);
            if (usr == null) return NotFound();
            if (usr.isRegPaid == true)
            {
                usr.accommodation.appliedRoommateName = info.appliedRoommateName;
                info = usr.accommodation;
            }
            if (_accommodationService.updateAccInfo(actionUid, info)) return NoContent(); else return NotFound();
        }
        [Authorize]
        [HttpPut("acc/{uid:int}")]
        public IActionResult updateAccInfo(int uid, YukiUserAccInfo info)
        {
            var actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Edit_User_Info, 0)) return Unauthorized();
            if (_accommodationService.updateAccInfo(uid, info)) return NoContent(); else return NotFound();
        }
        [Authorize]
        [HttpPost("withdraw")]
        public IActionResult withdrawBySelf()
        {
            var actionUid = getUserId();
            if (withdrawal(actionUid)) return NoContent(); else return NotFound();
        }
        [Authorize]
        [HttpPost("withdraw/{uid:int}")]
        public IActionResult withdrawUserByAdm(int uid)
        {
            var actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Withdrawal_Users, 0))
                return Unauthorized();
            if (withdrawal(uid)) return NoContent(); else return NotFound();
        }
        [Authorize]
        [HttpDelete("withdraw/{uid:int}")]
        public IActionResult rejoinConference(int uid)
        {
            var actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Withdrawal_Users, 0))
                return Unauthorized();
            if (_service.updateWD(uid, false))
            {
                _service.updateRegPaid(uid, false);
                _service.updateAccPaid(uid, false);
                return NoContent();
            }
            else return NotFound();
        }


        public bool withdrawal(int uid)
        {
            if (_permissionService.vertifyPermission(uid, YukiPermissionType.Global_Cannot_Withdrawal, 0)) return false;
            if (!_paymentService.withdrawByUid(uid)) return false;
            var cls = _committeeService.getCommitteesByMember(uid);
            foreach(var c in cls)
            {
                _committeeService.removeMember(c.cid, uid);
            }
            var pgls = _pgService.getPGByMember(uid);
            foreach(var pg in pgls)
            {
                _pgService.removeMember(pg.pgid, uid);
            }
            _reviewService.cancelByUid(uid);
            var sls = _seatService.getSeatByUid(uid);
            foreach(var s in sls)
            {
                _seatService.unassignSeat(s.sid, s.uid);
            }
            var acc = _accommodationService.getByUid(uid);
            if (acc != null)
            {
                _accommodationService.delete(acc.accaId);
                acc.assignment.Remove(uid);
                if (acc.assignment.Count > 0) _accommodationService.create(acc.assignment);
            }
            return true;
        }
    }
}