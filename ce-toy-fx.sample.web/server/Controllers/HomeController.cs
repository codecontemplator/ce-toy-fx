using ce_toy_fx.sample.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ce_toy_fx.sample.Dynamic;
using Newtonsoft.Json.Linq;

namespace ce_toy_fx.sample.web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }

    public class ApiController : Controller
    {
        [HttpPost]
        public IActionResult Evaluate([FromBody] object jtoken)
        {
            var json = jtoken.ToString();
            var root = JsonParser.ParseMRule(json);
            return Json("Ok!");
        }
    }
}