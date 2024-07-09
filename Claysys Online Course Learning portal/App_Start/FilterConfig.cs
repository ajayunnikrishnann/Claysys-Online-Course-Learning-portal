using System.Web;
using System.Web.Mvc;

namespace Claysys_Online_Course_Learning_portal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
