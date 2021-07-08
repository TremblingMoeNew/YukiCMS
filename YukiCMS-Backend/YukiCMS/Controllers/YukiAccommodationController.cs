using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YukiCMS.Models;
using YukiCMS.Service;
using YukiCMS.Models.PostRequest;
using YukiCMS.Models.Response;
using Microsoft.AspNetCore.Authorization;

namespace YukiCMS.Controllers
{
    [Authorize]
    [Route("api/accommodation")]
    [ApiController]
    public class YukiAccommodationController : ControllerBase
    {
        private readonly YukiAccommodationService _service;
        private readonly YukiPermissionService _permissionService;
        private readonly YukiUserService _userService;
        private readonly YukiCommitteeService _committeeService;

        public YukiAccommodationController(
                YukiAccommodationService service,
                YukiPermissionService permissionService,
                YukiUserService userService,
                YukiCommitteeService committeeService
            )
        {
            _service = service;
            _permissionService = permissionService;
            _userService = userService;
            _committeeService = committeeService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        [HttpGet]
        public ActionResult<List<YukiAccAssignment>>getAssignments()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Accommodations, 0))
                return Unauthorized();
            return _service.getAll();
        }
        [HttpPost]
        public IActionResult createAssignment(List<int> assignment)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Accommodations, 0))
                return Unauthorized();
            if (_service.create(assignment)) return NoContent(); else return Conflict();
        }
        [HttpGet("{accaid:int}")]
        public ActionResult<YukiAccAssignment> getAccAById(int accaid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Accommodations, 0))
                return Unauthorized();
            return _service.getByACCAId(accaid);
        }
        private YukiUserBriefWithConfResponse getUBCR(YukiUser usr)
        {
            var ubcr = new YukiUserBriefWithConfResponse
            {
                uid = usr.uid,
                email = usr.email,
                name = usr.info.name,
                sex = usr.info.sex,
                phoneNumber = usr.info.phoneNumber,
                qqNumber = usr.info.qqNumber,
                wechatNumber = usr.info.wechatNumber,
                confs = new List<YukiCommitteeNameItemResponse>()
            };
            var cls = _committeeService.getCommitteesByMember(usr.uid);
            foreach (var c in cls)
            {
                ubcr.confs.Add(new YukiCommitteeNameItemResponse
                {
                    cid = c.cid,
                    name = c.name
                });
            }
            return ubcr;
        }
        [HttpGet("roommate")]
        public ActionResult<YukiUserBriefWithConfResponse>getRoommate()
        {
            int actionUid = getUserId();
            var acca = _service.getByUid(actionUid);
            if (acca == null) return NotFound();
            if (acca.assignment.Count == 1) return new JsonResult ( new { singleAcc = true } );
            acca.assignment.Remove(actionUid);
            int rmuid = acca.assignment.FirstOrDefault();
            var usr = _userService.getByUid(rmuid);
            if (usr == null) return new JsonResult(new { singleAcc = true });
            return getUBCR(usr);
        }
        [HttpGet("unapplied")]
        public ActionResult<List<YukiUserBriefWithConfResponse>> getUnappliedUsers()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Accommodations, 0))
                return Unauthorized();
            var usrls = _userService.getUsersByGA(true);
            var res = new List<YukiUserBriefWithConfResponse>();
            foreach(var usr in usrls)
            {
                if (usr.isWithdrawaled) continue;
                if (_service.getByUid(usr.uid) != null) continue;
                res.Add(getUBCR(usr));
            }
            return res;
        }
        [HttpGet("unapplied/{cid:int}")]
        public ActionResult<List<YukiUserBriefWithConfResponse>> getUnappliedUsersByConf(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Accommodations, 0))
                return Unauthorized();
            var com = _committeeService.getByCid(cid);
            if (com == null) return NotFound();
            var res = new List<YukiUserBriefWithConfResponse>();
            foreach (var uid in com.members)
            {
                var usr = _userService.getByUid(uid);
                if (!usr.accommodation.isGA) continue;
                if (usr.isWithdrawaled) continue;
                if (_service.getByUid(usr.uid) != null) continue;
                res.Add(getUBCR(usr));
            }
            return res;
        }
    }
}