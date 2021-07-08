using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models.PostRequest
{
    public class YukiReviewRequest
    {
        public int ttid { get; set; }
        public int duration { get; set; }
        public List<String> questions { get; set; }
        public int admUid { get; set; }

        public DateTime startAt { get; set; }
    }
}
