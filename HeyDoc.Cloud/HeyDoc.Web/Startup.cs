using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Text;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(HeyDoc.Web.Startup))]

namespace HeyDoc.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseJwtBearerAuthentication(
               new JwtBearerAuthenticationOptions
               {
                   AuthenticationMode = AuthenticationMode.Active,
                   TokenValidationParameters = new TokenValidationParameters()
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = "http://mysite.com",
                       ValidAudience = "http://mysite.com",
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("asdf_JKL_123_$%^"))
                   }
               });
        }
    }
}
