using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace ExpenseTracker.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int GetRoutedId() {
            if (int.TryParse(RouteData.Values["id"].ToString(), out int id)) {
                return id;
            }
            return -1;
        }

        protected string GetRoutedAction() => RouteData.Values["action"].ToString();

        protected string GetRoutedController() => RouteData.Values["controller"].ToString();
    }
}