using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CQSAirborne.Model
{
    public class LoginViewModel : BaseValidateModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        //[Required(ErrorMessage ="enter captcha")]
        public string CaptchaCode { get; set; } = "";
    }

    public class ApplicationUser : IdentityUser
    {
    }

    public class SSORequest
    {
        public string authenticationId { get; set; } = "";
        public string secretKey { get; set; }
        public string userName { get; set; }
        public string name { get; set; }
        public string OrgRole { get; set; }
        public bool IsDirect { get; set; } = false;
    }

}
