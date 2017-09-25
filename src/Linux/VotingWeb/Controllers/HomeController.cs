using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VotingWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["os"] = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
