using System.Web;
using System.Web.Mvc;

namespace rqq_management_portal_asp_net
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
