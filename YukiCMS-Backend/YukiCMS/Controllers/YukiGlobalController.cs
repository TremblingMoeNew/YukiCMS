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
    [Route("api/global")]
    [ApiController]
    [Authorize]
    public class YukiGlobalController : ControllerBase
    {
        private readonly YukiGlobalService _service;
        private readonly YukiPermissionService _permissionService;
        private readonly YukiUserService _userService;
        private readonly YukiPaymentService _paymentService;
        private readonly YukiExcelService _excelService;
        public YukiGlobalController(
                YukiGlobalService service,
                YukiPermissionService permissionService,
                YukiUserService userService,
                YukiPaymentService paymentService,
                YukiExcelService excelService
            )
        {
            _service = service;
            _permissionService = permissionService;
            _userService = userService;
            _paymentService = paymentService;
            _excelService = excelService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        [HttpGet("settings")]
        public IActionResult getSettings() =>
            new JsonResult(new { settings = _service.getSettings(), editable = _permissionService.vertifySuperUser(getUserId()) });

        [HttpPost("settings")]
        public IActionResult updateSettings(YukiGlobalSettings settings)
        {
            var actionUid = getUserId();
            if (!_permissionService.vertifySuperUser(actionUid)) return Unauthorized();
            _service.updateSettings(settings);
            _paymentService.recalculateActiveRegBills();
            return NoContent();
        }
        [HttpGet("export")]
        public IActionResult downloadUsersExcel()
        {
            var actionUid = getUserId();
            if (!_permissionService.vertifySuperUser(actionUid)) return Unauthorized();
            var blob = _excelService.exportToExcel();
            return File(blob, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","general.xlsx");
        }
    }
}