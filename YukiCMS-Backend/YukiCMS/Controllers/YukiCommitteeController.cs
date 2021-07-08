using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YukiCMS.Models;
using YukiCMS.Service;
using YukiCMS.Models.Response;
namespace YukiCMS.Controllers
{
    [Authorize]
    [Route("api/committee")]
    [ApiController]
    public class YukiCommitteeController : ControllerBase
    {
        private readonly YukiCommitteeService _service;
        private readonly YukiPermissionService _permissionService;
        private readonly YukiPaymentService _paymentService;
        private readonly YukiSeatService _seatService;
        private readonly YukiUserService _userService;
        public YukiCommitteeController(
                YukiCommitteeService service,
                YukiPermissionService permissionService,
                YukiPaymentService paymentService,
                YukiSeatService seatService,
                YukiUserService userService
            )
        {
            _service = service;
            _permissionService = permissionService;
            _paymentService = paymentService;
            _seatService = seatService;
            _userService = userService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        [HttpPost]
        public IActionResult create(YukiCommittee committee)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Create_Committee, 0))
                return Unauthorized();
            return CreatedAtRoute("GetCommittee", new { cid = _service.create(committee) }, committee);
        }


        [HttpGet("c{cid:int}",Name ="GetCommittee")]
        public ActionResult<YukiCommittee> getCommittee(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_View, cid)) return Unauthorized();
            return _service.getByCid(cid);
        }
        [HttpGet("c{cid:int}/brief")]
        public ActionResult<YukiCommitteeBriefResponse>getBrief(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_View, cid)) return Unauthorized();
            var com = _service.getByCid(cid);
            return new YukiCommitteeBriefResponse
            {
                cid = com.cid,
                name = com.name,
                ctype = com.ctype,
                cdesc = com.cdesc,
                applyDDL = com.applyDDL
            };
        }
        [HttpPost("c{cid:int}")]
        public IActionResult applyCommittee(int cid)
        {
            int actionUid = getUserId();
            var committee = _service.getByCid(cid);
            if (committee == null) return NotFound();
            if (committee.ctype == YukiCommitteeType.unappliable) return NotFound();
            if (committee.ctype == YukiCommitteeType.appliableNormalMutualCommittee)
            {
                var mutual = _service.getMutualCommitteesByMember(actionUid);
                if (mutual.Count > 0) return NotFound();
            }
            if (_service.addMember(cid, actionUid))
            {
                _paymentService.createRegBill(actionUid);
                return NoContent();
            }
            else return NotFound();
        }
        [HttpGet("c{cid:int}/members")]
        public ActionResult<List<YukiUserNameResponse>> getMembers(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Get_Members, cid))
                return Unauthorized();
            var com = _service.getByCid(cid);
            if (com == null) return NotFound();
            var res = new List<YukiUserNameResponse>();
            foreach(var uid in com.members)
            {
                var usr = _userService.getByUid(uid);
                if (usr == null) continue;
                res.Add(new YukiUserNameResponse
                {
                    uid = uid,
                    name = usr.info.name,
                    email = usr.email
                });
            }
            return res;
        }
        [HttpPost("c{cid:int}/members/{uid:int}")]
        public IActionResult addMember(int cid, int uid) 
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Transfer_Committee_Members, 0))
                return Unauthorized();

            if (_service.addMember(cid, uid))
            {
                _paymentService.createRegBill(uid);
                return NoContent();
            }
            else return NotFound();
        }
        [HttpGet("c{cid:int}/members/unassigned")]
        public ActionResult<List<YukiUserNameResponse>> getUnassignedMembers(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Get_Members, cid))
                return Unauthorized();
            var com = _service.getByCid(cid);
            if (com == null) return NotFound();
            var res = new List<YukiUserNameResponse>();
            foreach (var uid in com.members)
            {
                var usr = _userService.getByUid(uid);
                if (usr == null) continue;
                if (_seatService.getByUC(cid, uid) != null) continue;
                res.Add(new YukiUserNameResponse
                {
                    uid = uid,
                    name = usr.info.name,
                    email = usr.email
                });
            }
            return res;
        }
        [HttpDelete("c{cid:int}/members/{uid:int}")]
        public IActionResult removeMember(int cid, int uid)  // TODO: Payment Recheck
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Transfer_Committee_Members, 0))
                return Unauthorized();
            if (_service.removeMember(cid, uid))
            {
                var com = _service.getByCid(cid);
                if (com.paymentSettings.paymentEnabled) _paymentService.cancelRegBill(uid);
                return NoContent();
            }
            else return NotFound();
        }
        [HttpPut("c{cid:int}/info")]
        public IActionResult editCommitteeInfo(int cid, YukiCommitteeBriefResponse info)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Edit_Info, cid)) return Unauthorized();
            if (_service.editInfo(cid, info)) return NoContent(); else return NotFound();
        }
        [HttpPut("c{cid:int}/type")]
        public IActionResult editCommitteeType(int cid, [FromBody]YukiCommitteeType type)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Edit_Info, cid)) return Unauthorized();
            if (_service.changeType(cid, type)) return NoContent(); else return NotFound();
        }
        [HttpGet("c{cid:int}/push")]
        public ActionResult<List<int>>getAutoPushedTasks(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Push_Reviews, cid)
                && !_permissionService.vertifyPermission(actionUid,YukiPermissionType.Committee_Create_Review_Template,cid))
                return Unauthorized();
            return _service.getAutoPushedTasks(cid);
        }
        [HttpGet("detailed")]
        public ActionResult<List<YukiCommitteeDetailedResponse>> getCommitteeDetailedList()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Committee_Details, 0))
                return Unauthorized();
            var res = new List<YukiCommitteeDetailedResponse>();
            var cls = _service.getList(c => true);
            foreach (var c in cls)
            {
                var item = new YukiCommitteeDetailedResponse
                {
                    cid = c.cid,
                    name = c.name,
                    ctype = c.ctype,
                    cdesc = c.cdesc,
                    autoPushedTasksCount = c.autoPushedTasks.Count,
                    membersCount = c.members.Count,
                    paymentSettings = c.paymentSettings,
                    applyDDL = c.applyDDL,
                    paidMembersCount = _userService.getUsersInListByRegPayment(true, c.members).Count
                };
                res.Add(item);
            }
            return res;
        }
        [HttpGet]
        public ActionResult<List<YukiCommitteeNameItemResponse>> getCommitteeBriefList()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_Group_Permissions, 0)
                && !_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_User_Permissions_Customize, 0))
                return Unauthorized();
            var res = new List<YukiCommitteeNameItemResponse>();
            var cls = _service.getList(c => true);
            foreach (var c in cls)
            {
                var item = new YukiCommitteeNameItemResponse
                {
                    cid = c.cid,
                    name = c.name
                };
                res.Add(item);
            }
            return res;
        }
        [HttpGet("view")]
        public ActionResult<List<YukiCommitteeNamePermissionResponse>> getVisibleCommittee()
        {
            int actionUid = getUserId();
            var res = new List<YukiCommitteeNamePermissionResponse>();
            if (_permissionService.vertifySuperUser(actionUid))
            {
                var cls = _service.getList(c => true);
                foreach (var c in cls)
                {
                    var item = new YukiCommitteeNamePermissionResponse
                    {
                        cid = c.cid,
                        name = c.name,
                        enableSeatManagement = true
                    };
                    res.Add(item);
                }
                return res;
            }
            var visibleCommitteePermission = _permissionService.getPermissionsByUT(actionUid, YukiPermissionType.Committee_View);
            foreach (var p in visibleCommitteePermission)
            {
                var item = new YukiCommitteeNamePermissionResponse
                {
                    cid = p.permission.pObjectId,
                    name = _service.getByCid(p.permission.pObjectId).name,
                    enableSeatManagement = _permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, p.permission.pObjectId)
                };
                res.Add(item);
            }
            return res;
        }
        [HttpGet("available")]
        public ActionResult<List<YukiCommitteeBriefResponse>> getAvailableCommittee()
        {
            int actionUid = getUserId();
            var adm = _service.getCommitteesByTypeWithoutMember(YukiCommitteeType.appliableAdmCommittee, actionUid);
            bool mutualNotSelected = _service.getMutualCommitteesByMember(actionUid).Count == 0;
            var res = new List<YukiCommitteeBriefResponse>();
            if (mutualNotSelected)
            {
                var mutual = _service.getMutualCommittees();
                foreach (var c in mutual)
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
            }
            foreach (var c in adm)
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
        [HttpGet("available/{uid:int}")]
        public ActionResult<List<YukiCommitteeBriefResponse>> getAvailableCommitteeForUser(int uid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Transfer_Committee_Members, 0))
                return Unauthorized();
            var adm = _service.getCompaibleCommitteesWithoutMember(uid);
            bool mutualNotSelected = _service.getMutualCommitteesByMember(uid).Count == 0;
            var res = new List<YukiCommitteeBriefResponse>();
            if (mutualNotSelected)
            {
                var mutual = _service.getMutualCommittees();
                foreach (var c in mutual)
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
            }
            foreach (var c in adm)
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
        [HttpGet("settings/payments")]
        public ActionResult<List<YukiCommitteePaymentSettingsResponse>> getPaymentSettings()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Edit_Payment_Settings, 0))
                return Unauthorized();
            var cls = _service.getList(c => true);
            var res = new List<YukiCommitteePaymentSettingsResponse>();
            foreach (var com in cls)
            {
                res.Add(new YukiCommitteePaymentSettingsResponse
                {
                    cid = com.cid,
                    name = com.name,
                    price = com.paymentSettings.price,
                    refund = com.paymentSettings.refund,
                    paymentEnabled = com.paymentSettings.paymentEnabled
                });
            }
            return res;
        }
        [HttpGet("settings/payments/{cid:int}")]
        public ActionResult<YukiCommitteePaymentSettingsResponse> getCommitteePaymentSettings(int cid)
        {
            var com = _service.getByCid(cid);
            if (com == null) return NotFound();
            return new YukiCommitteePaymentSettingsResponse
            {
                cid = com.cid,
                name = com.name,
                price = com.paymentSettings.price,
                refund = com.paymentSettings.refund,
                paymentEnabled = com.paymentSettings.paymentEnabled
            };
        }
        [HttpPost("settings/payments/{cid:int}")]
        public IActionResult enablePayment(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Edit_Payment_Settings, 0))
                return Unauthorized();
            if (_paymentService.enableCommitteePayment(cid)) return NoContent(); else return NotFound();
        }
        [HttpPut("settings/payments/{cid:int}")]
        public IActionResult editPrice(int cid, YukiCommitteePaymentSettings settings)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Edit_Payment_Settings, 0))
                return Unauthorized();
            if (_paymentService.editCommitteeRegPrice(cid, settings)) return NoContent(); else return NotFound();
        }
        [HttpDelete("settings/payments/{cid:int}")]
        public IActionResult disablePayment(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Edit_Payment_Settings, 0))
                return Unauthorized();
            if (_paymentService.disableCommitteePayment(cid)) return NoContent(); else return NotFound();
        }

    }
}