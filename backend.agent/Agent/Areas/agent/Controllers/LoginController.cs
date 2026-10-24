
using Repository;
using System.Web.Mvc;
using Repository.Models.Interface;
using Repository.Models;
using Repository.Models.Repository;

namespace Agent.Areas.agent.Controllers
{
    public class LoginController : Controller
    {
        private IRepository<agent_total> agentTotalRepository;

        public LoginController()
        {
            this.agentTotalRepository = new GenericRepository<agent_total>();
        }
        // GET: agenttotal/Login
        public ActionResult Index()
        {
            return View();
        }

        // GET: agenttotal/Login/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            return View();
        }
    }
}