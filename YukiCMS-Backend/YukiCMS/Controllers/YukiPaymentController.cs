using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YukiCMS.Models;
using YukiCMS.Service;

namespace YukiCMS.Controllers
{
    [Authorize]
    [Route("api/payment")]
    [ApiController]
    public class YukiPaymentController : ControllerBase
    {
        private readonly YukiPaymentService _service;
        private readonly YukiGlobalService _globalService;
        private readonly YukiPermissionService _permissionService;
        public YukiPaymentController(
                YukiPaymentService service,
                YukiGlobalService globalService,
                YukiPermissionService permissionService
            )
        {
            _service = service;
            _globalService = globalService;
            _permissionService = permissionService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        [HttpGet]
        public ActionResult<List<YukiBill>>getBills()
        {
            int actionUid = getUserId();
            var res = _service.getBillsByUid(actionUid);
            res.Reverse();
            return res;
        }
        [HttpGet("active")]
        public ActionResult<List<YukiBill>> getActiveBills()
        {
            int actionUid = getUserId();
            var bil = _service.getActivatedRegBillByUid(actionUid);
            var res = new List<YukiBill>();
            if (bil != null) res.Add(bil);
            return res;
        }
        [HttpGet("reg")]
        public ActionResult<List<YukiBill>>getRegBills()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Payments, 0))
                return Unauthorized();
            return _service.getRegBills();
        }
        [HttpGet("reg/active")]
        public ActionResult<List<YukiBill>>getActivatedRegBills()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Payments, 0))
                return Unauthorized();
            return _service.getActivatedRegBills();
        }
        [HttpGet("reg/completed")]
        public ActionResult<List<YukiBill>> getCompletedRegBills()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Payments, 0))
                return Unauthorized();
            return _service.getCompletedRegBills();
        }
        [HttpGet("wdl")]
        public ActionResult<List<YukiBill>> getWithdrawaledBills()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Payments, 0))
                return Unauthorized();
            return _service.getWithdrawalBills();
        }
        [HttpGet("wdl/active")]
        public ActionResult<List<YukiBill>> getActivatedWithdrawaledBills()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Payments, 0))
                return Unauthorized();
            return _service.getActivatedWithdrawalBills();
        }
        [HttpGet("wdl/completed")]
        public ActionResult<List<YukiBill>> getCompletedWithdrawaledBills()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Payments, 0))
                return Unauthorized();
            return _service.getCompletedWithdrawalBills();
        }
        [HttpGet("{bid:int}")]
        public ActionResult<YukiBill>getBillById(int bid)
        {
            int actionUid = getUserId();
            var bill = _service.getBillById(bid);
            if (actionUid != bill.uid && !_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_View_Payments, 0)) 
                return Unauthorized();
            return bill;
        }
        [HttpGet("sc/{sc:int}")]
        public ActionResult<YukiBill> getBillBysc(int sc)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Payments, 0))
                return Unauthorized();
            var bill = _service.getActivatedBillBySC(sc);
            return bill;
        }
        [HttpPost("{bid:int}")]
        public IActionResult completeBillById(int bid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Payments, 0))
                return Unauthorized();
            if (_service.completeBillById(bid)) return NoContent(); else return NotFound();
        }
        [HttpPost("sc/{sc:int}")]
        public IActionResult completeBillBySC(int sc)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Payments, 0))
                return Unauthorized();
            if (_service.completeBillBySC(sc)) return NoContent(); else return NotFound();
        }
        [HttpPut("{bid:int}")]
        public IActionResult completeBillByIdWithModify(int bid, [FromBody]double price)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Payments, 0))
                return Unauthorized();
            if (_service.completeBillByIdWithPriceModified(bid, price)) return NoContent(); else return NotFound();
        }
        [HttpPut("sc/{sc:int}")]
        public IActionResult completeBillBySCWithModify(int sc, [FromBody]double price)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Payments, 0))
                return Unauthorized();
            if (_service.completeBillBySCWithPriceModified(sc, price)) return NoContent(); else return NotFound();
        }
        [HttpDelete("{bid:int}")]
        public IActionResult cancelBillById(int bid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Payments, 0))
                return Unauthorized();
            if (_service.cancelBillById(bid)) return NoContent(); else return NotFound();
        }
        [HttpDelete("sc/{sc:int}")]
        public IActionResult cancelBillBySC(int sc)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Modify_Payments, 0))
                return Unauthorized();
            if (_service.cancelBillBySC(sc)) return NoContent(); else return NotFound();
        }
    }
}