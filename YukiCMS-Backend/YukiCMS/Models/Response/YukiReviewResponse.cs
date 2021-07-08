using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models.Response
{
    public class YukiReviewResponse
    {
        public int tid { get; set; }
        public int uid { get; set; }
        public string uname { get; set; }
        public int cid { get; set; }
        public int admUid { get; set; }
        public string admName { get; set; }
        public DateTime startAt { get; set; }
        public DateTime ddl { get; set; }
        public List<string> questions { get; set; }
        public string comment { get; set; }
        public YukiReviewStatus status { get; set; }
    }
}
