using MLeave.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MLeave.Controllers
{
    public class LeaveTypesController : ControllerBase
    {
        // GET: LeaveTypes
        public async Task<ActionResult> Index()
        {
            var leaveTypeRepository = new LeaveTypeRepository();
            var allLeaveTypes = await leaveTypeRepository.FindAll();

            return JsonNet(allLeaveTypes);
        }
    }
}