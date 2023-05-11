using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using HeyDoc.Web.WebApi;
using Microsoft.IdentityModel.Tokens;

namespace HeyDoc.Web.Controllers.Api
{
    public class ZoomController : ApiController
    {
        static readonly char[] padding = { '=' };

        [HttpGet]
        public string getZoomSignature(string chatroomId, string roleId, string accessToken)
        {
            string apiKey = "FNDkyihiNMC1TuzxWX5o2bhKW1LIojFkA6W7";
            string apiSecret = "Ht5g4EmbfAkCQlNuxzqCQqNH18QcCU17D46c";
            string meetingNumber = chatroomId;
            String ts = (ToTimestamp(DateTime.UtcNow.ToUniversalTime()) - 30000).ToString();
            string role = roleId;
            string token = GenerateToken(apiKey, apiSecret, meetingNumber, ts, role);
            return token;
        }
        [HttpGet]
        public string getZoomSignatureJWT(string chatroomId, string roleId, string accessToken)
        {

            // Token will be good for 20 minutes
            DateTime Expiry = DateTime.UtcNow.AddMinutes(45);
            string ApiKey = "FNDkyihiNMC1TuzxWX5o2bhKW1LIojFkA6W7";
            string ApiSecret = "Ht5g4EmbfAkCQlNuxzqCQqNH18QcCU17D46c";
            int ts = (int)(Expiry - new DateTime(1970, 1, 1)).TotalSeconds;
            // Create Security key  using private key above:
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiSecret));
            // length should be >256b
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            //Finally create a Token
            var header = new JwtHeader(credentials);
            //Zoom Required Payload
            var payload = new JwtPayload
            {
                { "app_key", ApiKey},
                { "version", 1 },
                { "user_identity", roleId },              
                { "exp", ts },
                { "tpc", chatroomId },
            };

            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();
            // Token to String so you can use it in your client
            var tokenString = handler.WriteToken(secToken);
            return tokenString;
        }
        public static long ToTimestamp(DateTime value)
        {
            long epoch = (value.Ticks - 621355968000000000) / 10000;
            return epoch;
        }

        public static string GenerateToken(string apiKey, string apiSecret, string meetingNumber, string ts, string role)
        {
            string message = String.Format("{0}{1}{2}{3}", apiKey, meetingNumber, ts, role);
            apiSecret = apiSecret ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(apiSecret);
            byte[] messageBytesTest = encoding.GetBytes(message);
            string msgHashPreHmac = System.Convert.ToBase64String(messageBytesTest);
            byte[] messageBytes = encoding.GetBytes(msgHashPreHmac);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                string msgHash = System.Convert.ToBase64String(hashmessage);
                string token = String.Format("{0}.{1}.{2}.{3}.{4}", apiKey, meetingNumber, ts, role, msgHash);
                var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
                return System.Convert.ToBase64String(tokenBytes).TrimEnd(padding);
            }
        }
    }
}