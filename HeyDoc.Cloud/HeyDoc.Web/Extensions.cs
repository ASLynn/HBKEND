using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web
{
    public static class Extensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null)
            {
                return "-";
            }
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static HttpStatusCode GetHttpStatusCode(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            HttpStatusCodeAttribute[] attributes = (HttpStatusCodeAttribute[])fi.GetCustomAttributes(typeof(HttpStatusCodeAttribute), false);
            return (attributes.Length > 0) ? attributes[0].StatusCode : HttpStatusCode.OK;
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Length <= maxLength ? value : (value.Substring(0, maxLength) + "...");
            }
            else
            {
                return value;
            }
        }

        public static SelectList ToSelectList<TEnum>(this TEnum enumObj)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                         select new { Id = e, Name = e.ToString() };
            return new SelectList(values, "Id", "Name", enumObj);
        }

        public static IQueryable<T> ConditionalWhere<T>(this IQueryable<T> source, bool shouldFilter, Expression<Func<T, bool>> predicate)
        {
            if (shouldFilter)
            {
                return source.Where(predicate);
            }
            return source;
        }

        public static bool IsWithinNvarcharLimit(this string inString, int maxLength)
        {
            // NVARCHAR has the size set in number of UTF-16 byte-pairs
            return System.Text.Encoding.Unicode.GetByteCount(inString) <= (maxLength * 2);
        }
    }
}