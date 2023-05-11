using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyDoc.Web.Lib
{
    public interface IJwtTokenManager
    {
        string Authenticate(string phnum);
    }
}
