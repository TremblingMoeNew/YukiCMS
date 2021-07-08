using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiEmailSettings : IYukiEmailSettings
    {
        public string emailServerSMTPHost { get; set; }
        public int emailServerSMTPPort { get; set; }
        public bool emailServerSMTPUseSSL { get; set; }
        public string emailServerUsername { get; set; }
        public string emailServerPassword { get; set; }
        public string emailServerSenderAddress { get; set; }
        public string emailServerSenderClaimedName { get; set; }
        public string emailConferenceNamePerfix { get; set; }
        public string emailConferenceName { get; set; }
        public string emailConferenceNameSuffix { get; set; }
        public string emailConferenceFrontEndLink { get; set; }
    }

    public interface IYukiEmailSettings
    {
        string emailServerSMTPHost { get; set; }
        int emailServerSMTPPort { get; set; }
        bool emailServerSMTPUseSSL { get; set; }
        string emailServerUsername { get; set; }
        string emailServerPassword { get; set; }
        string emailServerSenderAddress { get; set; }
        string emailServerSenderClaimedName { get; set; }
        string emailConferenceNamePerfix { get; set; }
        string emailConferenceName { get; set; }
        string emailConferenceNameSuffix { get; set; }
        string emailConferenceFrontEndLink { get; set; }
    }
}
