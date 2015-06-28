using MLeave.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MLeave.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var userRepository = new UserRepository();
            var users = await userRepository.FindAll();

            return View(users);
        }
    }
}
