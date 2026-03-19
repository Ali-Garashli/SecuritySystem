using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SecurityDashboard.Controllers
{
    public class UserController : Controller
    {
        public IActionResult AddUser() {
            return View();
        }

        public IActionResult ViewUsers() {
            return View();
        }
    }
}

