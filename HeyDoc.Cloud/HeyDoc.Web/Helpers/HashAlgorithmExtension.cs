using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HeyDoc.Web.Helpers
{
    public static class HashAlgorithmExtension
    {
        /**
         *  Returns the hex string of the hash of the UTF-8 representation of the input string
         */
        public static string ComputeStringHash(this HashAlgorithm hashAlg, string input)
        {
            var hashBytes = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(input));
            var stringBuilder = new StringBuilder(hashBytes.Length * 2);

            foreach (var b in hashBytes)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}