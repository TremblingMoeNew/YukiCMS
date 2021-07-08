using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YukiCMS.Models;
using YukiCMS.Models.Response;
using YukiCMS.Service;


namespace YukiCMS.Controllers
{
    [Authorize]
    [Route("api/permission")]
    [ApiController]
    public class YukiPermissionController : ControllerBase
    {
        private readonly YukiPermissionService _service;
        private readonly YukiPermissionGroupService _groupService;
        private readonly YukiCommitteeService _committeeService;
        private readonly YukiFileGroupService _fgService;
        private readonly YukiUserService _userService;
        public YukiPermissionController(
                    YukiPermissionService service,
                    YukiPermissionGroupService groupService,
                    YukiCommitteeService committeeService,
                    YukiFileGroupService fgService,
                    YukiUserService userService
            )
        {
            _service = service;
            _groupService = groupService;
            _committeeService = committeeService;
            _fgService = fgService;
            _userService = userService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        private List<YukiPermissionResponse> getPermissionsByUid(int uid)
        {
            var res = new List<YukiPermissionResponse>();
            foreach (YukiPermissionType t in Enum.GetValues(typeof(YukiPermissionType)))
            {
                if (t == YukiPermissionType.Global_Root_Administrator || t == YukiPermissionType.Global_Core_Group_Member) continue;
                var ls = _service.getPermissionsByUT(uid, t);
                if (ls.Count == 0) continue;
                var resp = new YukiPermissionResponse
                {
                    type = t,
                    pObject = new List<YukiPermissionResponseObjectItem>()
                };
                var mark = 0;
                if (_service.isGlobalPermissionType(t)) mark = 1;
                else if (_service.isCommitteePermissionType(t)) mark = 2;
                else if (_service.isFileGroupPermissionType(t)) mark = 3;
                // TODO: More PermissionTypeGrps
                foreach (var p in ls)
                {
                    string name = "";
                    switch (mark)
                    {
                        case 2:
                            var c = _committeeService.getByCid(p.permission.pObjectId);
                            if (c != null) name = c.name;
                            break;
                        case 3:
                            var fg = _fgService.getByFGid(p.permission.pObjectId);
                            if (fg != null) name = fg.name;
                            break;
                            // TODO: More PermissionTypeGrps
                    }
                    var item = new YukiPermissionResponseObjectItem
                    {
                        pObjectId = p.permission.pObjectId,
                        pObjectName = name,
                        pGranterCount = p.grantFromGroup.Count,
                        customizeGranted = p.grantFromGroup.Count == 1 && p.grantFromGroup.Contains(0)
                    };
                    resp.pObject.Add(item);
                }
                res.Add(resp);
            }
            return res;
        }
        [HttpGet]
        public ActionResult<List<YukiPermissionResponse>> getSelfPermissions()
        {
            int actionUid = getUserId();
            return getPermissionsByUid(actionUid);
        }
        [HttpGet("u{uid:int}")]
        public IActionResult getPermissions(int uid)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_User_Permissions_List, 0)) return Unauthorized();
            return new JsonResult(
                new
                {
                    list = getPermissionsByUid(uid),
                    grantEnable = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_User_Permissions_Customize, 0)
                });
        }
        [HttpGet("u{uid:int}/{type:int}/{o:int}/granter")]
        public ActionResult<List<YukiPermissionGranterResponseItem>> getGranterList(int uid, YukiPermissionType type, int o)
        {
            int actionUid = getUserId();
            if (actionUid != uid && !_service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_User_Permissions_List, 0))
                return Unauthorized();
            var ls = _service.getPermission(uid, type, o);
            var res = new List<YukiPermissionGranterResponseItem>();
            foreach (var granter in ls.grantFromGroup)
            {
                string name = "独立添加";
                if (granter > 0)
                {
                    var g = _groupService.getByPGid(granter);
                    if (g == null) continue;
                    name = g.name;
                }
                res.Add(new YukiPermissionGranterResponseItem
                {
                    granterId = granter,
                    granterName = name
                });
            }
            return res;
        }
        [HttpPost("u{uid:int}/{type:int}/{o:int}")]
        public IActionResult grantCustomizedPermission(int uid, YukiPermissionType type, int o)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_User_Permissions_Customize, 0))
                return Unauthorized();
            if (type == YukiPermissionType.Global_Root_Administrator) return NotFound();
            if (type == YukiPermissionType.Global_Core_Group_Member && !_service.vertifyRoot(uid)) return NotFound();
            if (_service.addGranter(uid, type, o, 0)) return NoContent(); else return NotFound();
        }
        [HttpDelete("u{uid:int}/{type:int}/{o:int}")]
        public IActionResult revokeCustomizedPermission(int uid, YukiPermissionType type, int o)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_User_Permissions_Customize, 0))
                return Unauthorized();
            if (type == YukiPermissionType.Global_Root_Administrator) return NotFound();
            if (type == YukiPermissionType.Global_Core_Group_Member && !_service.vertifyRoot(uid)) return NotFound();
            var p = _service.getPermission(uid, type, o);
            if (p.grantFromGroup.Count == 1 && p.grantFromGroup.Contains(0))
            {
                if (_service.removeGranter(uid, type, o, 0)) return NoContent(); else return NotFound();
            }
            return NotFound();
        }

        [HttpGet("group/{gid:int}", Name = "getGroup")]
        public ActionResult<YukiPermissionGroupResponse> getPermissionGroup(int gid)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_Group_Permissions, 0))
                return Unauthorized();
            if (gid == 1 && !_service.vertifyRoot(actionUid)) return NotFound();
            var grp = _groupService.getByPGid(gid);
            if (grp == null) return NotFound();
            var res = new YukiPermissionGroupResponse
            {
                pgid = grp.pgid,
                name = grp.name,
                permissionList = new List<YukiPermissionGroupResponsePermissionItem>(),
                members = new List<YukiUserBriefResponse>(),
                isFreeGroup = grp.isFreeGroup
            };
            foreach (var pi in grp.permissionList)
            {
                int mark = 0;
                if (_service.isGlobalPermissionType(pi.ptype)) mark = 1;
                else if (_service.isCommitteePermissionType(pi.ptype)) mark = 2;
                else if (_service.isFileGroupPermissionType(pi.ptype)) mark = 3;
                string name = "";
                switch (mark)
                {
                    case 2:
                        var c = _committeeService.getByCid(pi.pObjectId);
                        if (c != null) name = c.name;
                        break;
                    case 3:
                        var fg = _fgService.getByFGid(pi.pObjectId);
                        if (fg != null) name = fg.name;
                        break;
                        // TODO: More PermissionTypeGrps
                }
                var item = new YukiPermissionGroupResponsePermissionItem
                {
                    ptype = pi.ptype,
                    pObjectId = pi.pObjectId,
                    pObjectName = name
                };
                res.permissionList.Add(item);
            }
            foreach (var uid in grp.members)
            {
                var usr = _userService.getByUid(uid);
                if (usr == null) continue;
                var item = new YukiUserBriefResponse
                {
                    uid = uid,
                    name = usr.info.name,
                    email = usr.email,
                    sex = usr.info.sex,
                    phoneNumber = usr.info.phoneNumber,
                    qqNumber = usr.info.qqNumber,
                    wechatNumber = usr.info.wechatNumber,
                    school = usr.info.school,
                };
                res.members.Add(item);
            }
            if (!grp.isFreeGroup && gid != 1)
            {
                var c = _committeeService.getByPGid(grp.pgid).FirstOrDefault();
                if (c != null)
                {
                    res.cid = c.cid;
                    res.cname = c.name;
                    res.fgrpid = c.fgrpid;
                }
            }
            return res;
        }
        [HttpGet("group")]
        public ActionResult<List<YukiPermissionGroup>> getGroups()
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_Group_Permissions, 0))
                return Unauthorized();
            if (_service.vertifyRoot(actionUid))
                return _groupService.getAllByRoot();
            else
                return _groupService.getAll();
        }
        [HttpGet("group/free")]
        public ActionResult<List<YukiPermissionGroup>> getFreeGroups()
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_Group_Permissions, 0))
                return Unauthorized();
            if (_service.vertifyRoot(actionUid))
                return _groupService.getFreeGroupByRoot();
            else
                return _groupService.getFreeGroup();
        }
        [HttpPost("group")]
        public IActionResult createFreeGroup([FromBody]string name)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Create_Group_Customize, 0))
                return Unauthorized();
            var grp = _groupService.createGroup(name, true);
            return CreatedAtRoute("getGroup", new { gid = grp.pgid }, grp);
        }
        [HttpDelete("group/{gid:int}")]
        public IActionResult deleteGroup(int gid)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Create_Group_Customize, 0))
                return Unauthorized();
            if (gid == 1) return NotFound();
            if (_groupService.removeGroup(gid)) return NoContent(); else return NotFound();
        }
        [HttpPost("group/{gid:int}")]
        public IActionResult addPermission(int gid, YukiPermissionItem item)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_Group_Permissions, 0))
                return Unauthorized();
            if (item.ptype == YukiPermissionType.Global_Root_Administrator || item.ptype == YukiPermissionType.Global_Core_Group_Member)
                return NotFound();
            if (_groupService.addPermission(gid, item)) return NoContent(); else return NotFound();
        }
        [HttpDelete("group/{gid:int}/{type:int}/{o:int}")]
        public IActionResult removePermission(int gid, YukiPermissionType type, int o)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_Group_Permissions, 0))
                return Unauthorized();
            if (type == YukiPermissionType.Global_Root_Administrator || type == YukiPermissionType.Global_Core_Group_Member)
                return NotFound();
            var grp = _groupService.getByPGid(gid);
            if (grp == null) return NotFound();
            if (!grp.isFreeGroup)
            {
                var coml = _committeeService.getByPGid(gid);
                if (coml == null) return NotFound();
                foreach (var com in coml)
                {
                    var clgpl = _groupService.createCLGDefaultPermissions(com.cid, com.fgrpid);
                    foreach (var p in clgpl)
                    {
                        if (p.ptype == type && p.pObjectId == o) return Forbid();
                    }
                }
            }

            if (_groupService.removePermission(gid, type, o)) return NoContent(); else return NotFound();
        }
        [HttpGet("group/{gid:int}/potential")]
        public ActionResult<List<YukiUserNameResponse>> getPotentialGroupMembers(int gid)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_Group_Permissions, 0)
                && !_service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_User_Permissions_Customize, 0))
                return Unauthorized();
            var res = new List<YukiUserNameResponse>();
            var grp = _groupService.getByPGid(gid);
            if (grp == null) return NotFound();
            var usrl = _userService.getUsersNotInList(grp.members);
            foreach (var u in usrl)
            {
                res.Add(new YukiUserNameResponse
                {
                    uid = u.uid,
                    name = u.info.name,
                    email = u.email
                });
            }
            return res;
        }
        [HttpPost("group/{gid:int}/u{uid:int}")]
        public IActionResult addMember(int gid, int uid)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revole_Group_To_User_Customize, 0))
                return Unauthorized();
            if (gid == 1 && !_service.vertifyRoot(actionUid))
                return Unauthorized();
            if (_groupService.addMember(gid, uid)) return NoContent(); else return NotFound();
        }
        [HttpDelete("group/{gid:int}/u{uid:int}")]
        public IActionResult removeMember(int gid, int uid)
        {
            int actionUid = getUserId();
            if (!_service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revole_Group_To_User_Customize, 0))
                return Unauthorized();
            if (gid == 1 && !_service.vertifyRoot(actionUid))
                return Unauthorized();
            if (_groupService.removeMember(gid, uid)) return NoContent(); else return NotFound();
        }
        [HttpGet("vertify/home")]
        public IActionResult getEnabledPages()
        {
            int actionUid = getUserId();
            var usr = _userService.getByUid(actionUid);
            if (usr.isWithdrawaled == true) return new JsonResult(new
            {
                withdrawed = true
            });
            else return new JsonResult(new
            {
                enableCommitteeManagement = _service.vertifyPermission(actionUid, YukiPermissionType.Global_View_Committee_Details, 0),
                enableAccManagement = _service.vertifyPermission(actionUid, YukiPermissionType.Global_View_Accommodations, 0),
                enablePaymentManagement = _service.vertifyPermission(actionUid, YukiPermissionType.Global_View_Payments, 0),
                enablePermissionGroupManagement = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_Group_Permissions, 0),
                enableGlobalSettings = _service.vertifySuperUser(actionUid),
                enableUserManagement = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_Users, 0)
            });
        }
        [HttpGet("vertify/committee/review/{cid:int}")]
        public IActionResult vertifyReviewPermissions(int cid)
        {
            int actionUid = getUserId();
            return new JsonResult(new
            {
                createReviews = _service.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Reviews, cid),
                pushReviews = _service.vertifyPermission(actionUid, YukiPermissionType.Committee_Push_Reviews, cid),
                generalReviewer = _service.vertifyPermission(actionUid, YukiPermissionType.Committee_General_Reviewer, cid),
                cancelReviews = _service.vertifyPermission(actionUid, YukiPermissionType.Committee_Cancel_Reviews, cid),
                createReviewTemplate = _service.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Review_Template, cid)
            });
        }
        [HttpGet("vertify/committee/seat/{cid:int}")]
        public IActionResult vertifySeatPermissions(int cid)
        {
            int actionUid = getUserId();
            return new JsonResult(new
            {
                createSeats = _service.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Seats, cid),
                assignSeats = _service.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, cid)
            });
        }
        [HttpGet("vertify/committee/")]
        public IActionResult vertifyCommitteeGlobalPermissions()
        {
            int actionUid = getUserId();
            return new JsonResult(new
            {
                editPaymentSettings = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Edit_Payment_Settings, 0),
                transferCommitteeMembers = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Transfer_Committee_Members, 0),
                createCommittee = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Create_Committee, 0)
            });
        }
        [HttpGet("vertify/committee/{cid:int}")]
        public IActionResult vertifyCommitteeManagementPermissions(int cid)
        {
            int actionUid = getUserId();
            return new JsonResult(new
            {
                editPaymentSettings = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Edit_Payment_Settings, 0),
                transferCommitteeMembers = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Transfer_Committee_Members, 0),
                createCommittee = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Create_Committee, 0),
                editInfo = _service.vertifyPermission(actionUid, YukiPermissionType.Committee_Edit_Info, cid)
            });
        }
        [HttpGet("vertify/payment")]
        public IActionResult vertifyPaymentPermissions()
        {
            int actionUid = getUserId();
            return new JsonResult(new
            {
                modifyPayment = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Payments, 0)
            });
        }
        [HttpGet("vertify/permissiongrp")]
        public IActionResult vertifyPermissionGroupPermissions()
        {
            int actionUid = getUserId();
            return new JsonResult(new
            {
                getGrps = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_Group_Permissions, 0),
                grantRevokePermissions = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_Group_Permissions, 0),
                createCusGroups = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Create_Group_Customize, 0),
                cusGroupsTransferMember = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revole_Group_To_User_Customize, 0),
            });
        }
        [HttpGet("vertify/usermanagement")]
        public IActionResult vertifyUserManagementPermissions()
        {
            int actionUid = getUserId();
            return new JsonResult(new
            {
                viewInfo = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_User_Info, 0),
                editInfo = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Edit_User_Info, 0),
                transferCommitteeMember = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Transfer_Committee_Members, 0),
                getUserPermissions = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Get_User_Permissions_List, 0),
                cusGroupsTransferMember = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revole_Group_To_User_Customize, 0),
                withdrawMembers = _service.vertifyPermission(actionUid, YukiPermissionType.Global_Withdrawal_Users, 0)
            });
        }
        [HttpGet("vertify/withdraw/{uid:int}")]
        public IActionResult vertifyUserWithdraw(int uid)
        {
            int actionUid = getUserId();
            var usr = _userService.getByUid(uid);
            return new JsonResult(new
            {
                withdrawed = usr == null ? false : usr.isWithdrawaled,
                canWithdraw = !_service.vertifyPermission(uid, YukiPermissionType.Global_Cannot_Withdrawal, 0)
            });
        }
    }
}   