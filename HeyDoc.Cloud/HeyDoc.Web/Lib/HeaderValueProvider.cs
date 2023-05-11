using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace HeyDoc.Web.Lib
{
    public class HeaderValueProvider : IValueProvider
    {
        private HttpRequestHeaders _headers;

        public HeaderValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            _headers = actionContext.Request.Headers;
        }
        public bool ContainsPrefix(string prefix)
        {
            return _headers.Contains(prefix);
        }

        public ValueProviderResult GetValue(string key)
        {
            if (_headers.TryGetValues(key, out var vals))
            {
                return new ValueProviderResult(vals, string.Join("; ", vals), CultureInfo.InvariantCulture);
            }
            return null;
        }
    }

    public class HeaderValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new HeaderValueProvider(actionContext);
        }
    }
}