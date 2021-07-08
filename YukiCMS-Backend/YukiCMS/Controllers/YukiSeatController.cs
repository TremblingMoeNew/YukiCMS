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
    [Route("api/seat")]
    [ApiController]
    public class YukiSeatController : ControllerBase
    {
        private readonly YukiSeatService _service;
        private readonly YukiCommitteeService _committeeService;
        private readonly YukiEmailService _emailService;
        private readonly YukiPermissionService _permissionService;
        public YukiSeatController(
                YukiSeatService service, 
                YukiCommitteeService committeeService,
                YukiEmailService emailService,
                YukiPermissionService permissionService
            )
        {
            _service = service;
            _committeeService = committeeService;
            _emailService = emailService;
            _permissionService = permissionService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        [HttpGet]
        public ActionResult<List<YukiSeat>> getSeats()
        {
            int actionUid = getUserId();
            return _service.getSeatByUid(actionUid);
        }
        [HttpPost]
        public IActionResult createSeat(YukiSeat seat)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Seats, seat.cid))
                return Unauthorized();
            var com = _committeeService.getByCid(seat.cid);
            if (com == null) return NotFound();
            var sid=_service.create(seat);
            return CreatedAtRoute("GetSeat", new { sid }, seat);
        }
        [HttpGet("c{cid:int}")]
        public ActionResult<YukiSeat> getSelfSeatInCommittee(int cid)
        {
            int actionUid = getUserId();
            return _service.getByUC(cid,actionUid);
        }
        [HttpGet("u{uid:int}/c{cid:int}")]
        public ActionResult<YukiSeat> getUserSeatInCommittee(int uid,int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, cid)
                && !_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Committee_Details, 0)
                && !_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Get_Members, cid)
                && uid != actionUid
            )
                return Unauthorized();
            return _service.getByUC(cid, uid);
        }
        [HttpGet("c{cid:int}/list")]
        public ActionResult<List<YukiSeat>> getSeatsInCommittee(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, cid))
                return Unauthorized();
            return _service.getSeatsByCid(cid);
        }

        [HttpGet("c{cid:int}/status")]
        public IActionResult getSeatsAssignmentCountInCommittee(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, cid))
                return Unauthorized();
            return new JsonResult(new
            {
                unassigned = _service.getSeatsByCidAndStatus(cid, YukiSeatStatus.unassigned).Count,
                assigned = _service.getSeatsByCidAndStatus(cid, YukiSeatStatus.assigned).Count
            });
        }

        [HttpGet("c{cid:int}/unassigned")]
        public ActionResult<List<YukiSeat>> getUnassignedSeatsInCommittee(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, cid))
                return Unauthorized();
            return _service.getSeatsByCidAndStatus(cid, YukiSeatStatus.unassigned);
        }
            
        [HttpGet("c{cid:int}/assigned")]
        public ActionResult<List<YukiSeat>> getAssignedSeatsInCommittee(int cid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, cid))
                return Unauthorized();
            return _service.getSeatsByCidAndStatus(cid, YukiSeatStatus.assigned);
        }
            

        [HttpGet("s{sid:int}",Name ="GetSeat")]
        public ActionResult<YukiSeat> getSeat(int sid)
        {
            int actionUid = getUserId();
            var seat=_service.getBySid(sid);
            if (seat == null) return NotFound();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, seat.cid)
                && !(seat.sid == actionUid && seat.status == YukiSeatStatus.assigned))
                return Unauthorized();
            return seat;
        }

        [HttpPost("s{sid:int}/u{uid:int}")]
        public IActionResult assignSeat(int sid,int uid)
        {
            int actionUid = getUserId();
            var seat = _service.getBySid(sid);
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, seat.cid))
                return Unauthorized();
            if (_service.assignSeat(sid, uid))
            {
                var com = _committeeService.getByCid(seat.cid);
                _emailService.sendSeatNotification(uid, com.name, seat.name);
                return NoContent();
            }
            else return NotFound();
        }
        [HttpDelete("s{sid:int}/u{uid:int}")]
        public IActionResult unassignSeat(int sid,int uid)
        {
            int actionUid = getUserId();
            var seat = _service.getBySid(sid);
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Assign_Seats, seat.cid))
                return Unauthorized();
            if (_service.unassignSeat(sid,uid)) return NoContent(); else return NotFound();
        }
        [HttpDelete("s{sid:int}")]
        public IActionResult deleteSeat(int sid)
        {
            int actionUid = getUserId();
            var seat = _service.getBySid(sid);
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Committee_Create_Seats, seat.cid))
                return Unauthorized();
            if (_service.deleteSeat(sid)) return NoContent(); else return NotFound();
        }
    }
}