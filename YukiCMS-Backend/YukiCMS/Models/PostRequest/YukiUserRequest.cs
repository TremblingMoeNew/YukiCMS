using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models.PostRequest
{
    public class YukiUserLoginRequest
    {
        public string email { get; set; }
        public string password { get; set; }
    }
    public class YukiUserRegisterRequest
    {
        public string email { get; set; }
        public string password { get; set; }
        public YukiUserInfo info { get; set; }
    }
    public class YukiUserChangePasswordRequest
    {
        public string opassword { get; set; }
        public string npassword { get; set; }
    }
    public class YukiUserResetPasswordRequest
    {
        public string token { get; set; }
        public string npassword { get; set; }
    }
}
