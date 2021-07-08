using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models.Response
{
    public class YukiFileNameResponse
    {
        public string fileOriginalName { get; set; }
        public string fileStorageName { get; set; }
        public string fileClaimedName { get; set; }
    }
    public class YukiFileGroupNameResponse
    {
        public int fgid { get; set; }
        public string name { get; set; }
    }
}
