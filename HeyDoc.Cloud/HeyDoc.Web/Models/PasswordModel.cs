using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PasswordModel
    {
        public string Password { get; set; }

        public PasswordModel(string password)
        {
            Password = password;
        }

        public void Validate()
        {
            (var pass, var errorMessage) = CheckPassword(Password);
            if (!pass)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, errorMessage));
            }
        }

        public static (bool pass, string errorMessage) CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, Account.ErrorPasswordNull);
            }
            // Checks for at least 1 of each character class and minimum of 8 characters
            //var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$");
            // As per request Password now require only min lenght is 6
            var passwordRegex = new Regex(@".{6,}$");
            if (!passwordRegex.Match(password).Success)
            {
                return (false, Account.ErrorPasswordInvalidFormat);
            }

            return (true, string.Empty);
        }
    }
}

//var letter = /[a - zA - Z] /;
//var number = /[0 - 9] /;