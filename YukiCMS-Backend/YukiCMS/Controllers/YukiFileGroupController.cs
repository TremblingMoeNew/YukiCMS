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
    [Route("api/filegroup")]
    [ApiController]
    public class YukiFileGroupController : ControllerBase
    {
        private readonly YukiFileGroupService _service;
        private readonly YukiFileService _fileService;
        private readonly YukiPermissionService _permissionService;
        public YukiFileGroupController(
                    YukiFileGroupService service,
                    YukiFileService fileService,
                    YukiPermissionService permissionService
            )
        {
            _service = service;
            _fileService = fileService;
            _permissionService = permissionService;
        }
        public int getUserId() =>
            Convert.ToInt32(HttpContext.User.Identity.Name);

        [HttpGet]
        public ActionResult<List<YukiFileGroup>>getAvailableFileGroups()
        {
            int actionUid = getUserId();
            if(_permissionService.vertifySuperUser(actionUid))
            {
                return _service.getList(fg => true);
            }
            var res = new List<YukiFileGroup>();
            var ls=_permissionService.getPermissionsByUT(actionUid, YukiPermissionType.FileGroup_Download_File);
            foreach(var p in ls)
            {
                var fg = _service.getByFGid(p.permission.pObjectId);
                if (fg != null) res.Add(fg);
            }
            return res;
        }
        [HttpGet("upload")]
        public ActionResult<List<YukiFileGroup>> getUploadAvailableFileGroups()
        {
            int actionUid = getUserId();
            if (_permissionService.vertifySuperUser(actionUid))
            {
                return _service.getList(fg => true);
            }
            var res = new List<YukiFileGroup>();
            var ls = _permissionService.getPermissionsByUT(actionUid, YukiPermissionType.FileGroup_Upload_File);
            foreach (var p in ls)
            {
                var fg = _service.getByFGid(p.permission.pObjectId);
                if (fg != null) res.Add(fg);
            }
            return res;
        }
        [HttpGet("list")]
        public ActionResult<List<YukiFileGroupNameResponse>> getFileGroupNames()
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.Global_Grant_Revoke_Group_Permissions, 0)
                && !_permissionService.vertifyPermission(actionUid,YukiPermissionType.Global_Grant_Revoke_User_Permissions_Customize,0))
                return Unauthorized();
            var grps = _service.getList(g => true);
            var res = new List<YukiFileGroupNameResponse>();
            foreach(var g in grps)
            {
                res.Add(new YukiFileGroupNameResponse
                {
                    fgid = g.fgid,
                    name = g.name
                });
            }
            return res;
        }
        [HttpGet("{fgid:int}")]
        public ActionResult<YukiFileGroup>getGroup(int fgid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.FileGroup_Download_File, fgid)) return Unauthorized();
            var grp = _service.getByFGid(fgid);
            if (grp == null) return NotFound();
            return grp;
        }
        [HttpGet("{fgid:int}/file")]
        public ActionResult<List<YukiFileNameResponse>>getFileNames(int fgid)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.FileGroup_Download_File, fgid)
                && !_permissionService.vertifyPermission(actionUid, YukiPermissionType.FileGroup_Upload_File, fgid))
                return Unauthorized();
            var grp = _service.getByFGid(fgid);
            var res = new List<YukiFileNameResponse>();
            foreach (var s in grp.files)
            {
                var f = _fileService.getFile(s);
                var r = new YukiFileNameResponse
                {
                    fileClaimedName = f.fileClaimedName,
                    fileOriginalName = f.fileOriginalName,
                    fileStorageName = f.fileStorageName
                };
                res.Add(r);
            }
            return res;
        }
        [HttpPost("{fgid:int}/file")]
        public IActionResult uploadFile(int fgid, IFormFile file)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.FileGroup_Upload_File, fgid)) return Unauthorized();
            string originalName=_fileService.create(file);
            if (_service.addFile(fgid, originalName)) return Ok(); else return NotFound();
        }
        [HttpGet("{fgid:int}/file/{name}")]
        public IActionResult downloadFile(int fgid,string name)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.FileGroup_Download_File, fgid)) return Unauthorized();
            var grp = _service.getByFGid(fgid);
            if (grp == null) return NotFound();
            if (!grp.files.Contains(name)) return NotFound();
            var file = _fileService.getFile(name);
            if (file == null) return NotFound();
            return File(file.blob, file.fileContentType, file.fileOriginalName);
        }
        [HttpDelete("{fgid:int}/file/{name}")]
        public IActionResult deleteFile(int fgid, string name)
        {
            int actionUid = getUserId();
            if (!_permissionService.vertifyPermission(actionUid, YukiPermissionType.FileGroup_Upload_File, fgid)) return Unauthorized();
            if (_service.deleteFile(fgid,name)) return NoContent(); else return NotFound();
        }
        [HttpGet("uploadlist")]
        public ActionResult<List<YukiFileGroup>> getUploadPermissionList()
        {
            int actionUid = getUserId();
            if (_permissionService.vertifySuperUser(actionUid))
            {
                return _service.getList(fg => true);
            }
            var res = new List<YukiFileGroup>();
            var ls = _permissionService.getPermissionsByUT(actionUid, YukiPermissionType.FileGroup_Upload_File);
            foreach (var p in ls)
            {
                var fg = _service.getByFGid(p.permission.pObjectId);
                if (fg != null) res.Add(fg);
            }
            return res;
        }
    }
}