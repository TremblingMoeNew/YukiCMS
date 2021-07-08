using Pomelo.AspNetCore.TimedJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukiCMS.Service;

namespace YukiCMS.Jobs
{
    public class YukiExpireJob:Job
    {
        private readonly YukiReviewService _reviewService;
        private readonly YukiFindPasswordTokenService _tokenService;
        public YukiExpireJob(YukiReviewService reviewService,YukiFindPasswordTokenService tokenService)
        {
            _reviewService = reviewService;
            _tokenService = tokenService;
        }
        [Invoke(Begin ="2020-10-1 00:00:00",Interval = 1000 * 60 * 20 * 1, SkipWhileExecuting=true)]
        public void checkExpires()
        {
            _reviewService.expireReviews(DateTime.Now);
            _tokenService.expire();
        }
    }
}
