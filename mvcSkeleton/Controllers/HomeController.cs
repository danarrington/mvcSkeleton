using System.Web.Mvc;

namespace mvcSkeleton.Controllers
{
    public class HomeController : Controller
    {
         public ActionResult Index()
         {
             return View();
         }
    }
}