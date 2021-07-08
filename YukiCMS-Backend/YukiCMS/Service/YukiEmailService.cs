using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiEmailService
    {
        private readonly IYukiEmailSettings _settings;
        private readonly YukiUserService _userService;
        private readonly ResourceManager _manager;
        public YukiEmailService(
                IYukiEmailSettings settings,
                YukiUserService userService
            )
        {
            _settings = settings;
            _userService = userService;
            _manager=new ResourceManager("YukiCMS.Resource", this.GetType().Assembly);
        }

        public void sendRegisterNotification(int uid)
        {
            var user = _userService.getByUid(uid);
            string reg = _manager.GetObject("mail_reg") as string;
            var body = String.Format(reg, _settings.emailConferenceName,_settings.emailConferenceNameSuffix, user.info.name);
            var message = createMessage(user.email, "注册确认", body);
            sendEmail(message);
        }
        public void sendJoinComNotification(int uid,string cname)
        {
            try
            {
                var user = _userService.getByUid(uid);
                string reg = _manager.GetObject("mail_joincom") as string;
                var body = String.Format(reg, _settings.emailConferenceName, _settings.emailConferenceNameSuffix, user.info.name, cname);
                var message = createMessage(user.email, "加入会场确认", body);
                sendEmail(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }
        public void sendRevieweeNotification(int uid, string cname)
        {
            try
            {
                var user = _userService.getByUid(uid);
                string reg = _manager.GetObject("mail_reviewee") as string;
                var body = String.Format(reg, _settings.emailConferenceName, _settings.emailConferenceNameSuffix, user.info.name, cname);
                var message = createMessage(user.email, "新的学测", body);
                sendEmail(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }
        public void sendSeatNotification(int uid, string cname,string seatname)
        {
            try
            {
                var user = _userService.getByUid(uid);
                string reg = _manager.GetObject("mail_seat") as string;
                var body = String.Format(reg, _settings.emailConferenceName, _settings.emailConferenceNameSuffix, user.info.name, seatname, cname);
                var message = createMessage(user.email, "席位分配确认", body);
                sendEmail(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }
        public void sendRegBillNotification(int uid, double price)
        {
            try
            {
                var user = _userService.getByUid(uid);
                string reg = _manager.GetObject("mail_regbill") as string;
                var body = String.Format(reg, _settings.emailConferenceName, _settings.emailConferenceNameSuffix, user.info.name, price);
                var message = createMessage(user.email, "账单生成确认", body);
                sendEmail(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }
        public void sendResetPassNotification(int uid, string token)
        {
            var user = _userService.getByUid(uid);
            string reg = _manager.GetObject("mail_resetpass") as string;
            string link = String.Format("{0}/forgetpassword/reset/{1}", _settings.emailConferenceFrontEndLink, token).Replace("//", "/");
            var body = String.Format(reg, _settings.emailConferenceName, _settings.emailConferenceNameSuffix, user.info.name, link);
            var message = createMessage(user.email, "重置密码", body);
            sendEmail(message);
        }
        public void sendReviewerNotification(int uid, string cname,string revieweename)
        {
            try
            {
                var user = _userService.getByUid(uid);
                string reg = _manager.GetObject("mail_reviewee") as string;
                var body = String.Format(reg, _settings.emailConferenceName, _settings.emailConferenceNameSuffix, user.info.name, cname, revieweename);
                var message = createMessage(user.email, "新的学测任务", body);
                sendEmail(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }
        public void sendWithdrawNotification(int uid)
        {
            try
            { 
                var user = _userService.getByUid(uid);
                string reg = _manager.GetObject("mail_withdraw") as string;
                var body = String.Format(reg, _settings.emailConferenceName, _settings.emailConferenceNameSuffix, user.info.name);
                var message = createMessage(user.email, "退会确认", body);
                sendEmail(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
}

        private MimeMessage createMessage(string receiverEmail,string title,string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.emailServerSenderClaimedName, _settings.emailServerSenderAddress));
            message.To.Add(MailboxAddress.Parse(receiverEmail));
            message.Subject = String.Format("{0} - {1}",_settings.emailConferenceNamePerfix,title);
            string style = _manager.GetObject("mail_style") as string;
            message.Body = new TextPart("html")
            {
                Text = body + style
            };
            return message;
        }
        private void sendEmail(MimeMessage message)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(_settings.emailServerSMTPHost, _settings.emailServerSMTPPort, _settings.emailServerSMTPUseSSL);
                client.Authenticate(_settings.emailServerUsername, _settings.emailServerPassword);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
