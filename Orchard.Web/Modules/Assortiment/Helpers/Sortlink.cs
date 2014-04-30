using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;

namespace Assortiment.Helpers
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString SortLink(
            this HtmlHelper helper,
            string linkText,
            int sortBy, 
            bool isAsc
        )
        {
            var query = helper.ViewContext.HttpContext.Request.QueryString;
            var values = query.AllKeys.ToDictionary(key => key, key => (object)query[key]);
            values["sortBy"] = sortBy;
            values["isAsc"] = isAsc;

            var routeValues = new RouteValueDictionary(values);
            return helper.ActionLink(linkText, "Index", routeValues);
        }
    }
}