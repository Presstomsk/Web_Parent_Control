using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text.Json;
using System.Threading.Tasks;
using Web_Parent_Control.Database;
using Web_Parent_Control.Models;
using Site = Web_Parent_Control.Models.Site;

namespace Web_Parent_Control.Controllers
{
   
    public class HomeController : Controller
    {
        private MainContext _mainContext;         
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, MainContext mainContext)
        {
            _logger = logger;
            _mainContext = mainContext;
        }

        [HttpGet]
        public IActionResult Index() // Вьюха сайта
        {
            var data = _mainContext.Sites.Select(x => x).ToList();
            return View(data);            
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

        [HttpGet]
        public IActionResult Action(Guid id) // TODO : Блокировка сайта
        {

            var site = _mainContext.Sites.SingleOrDefault(x => x.Id == id);
            if (site.Flag == false) site.Flag = true;
            else site.Flag = false;
            _mainContext.SaveChangesAsync();
            var sites = _mainContext.Sites.Select(x => x).ToList();
            return View("Index", sites);
        }

        [HttpPost]
        public IActionResult GetNewData(string period) // TODO : Запрос актуальных данных
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://localhost:44328/ParentSpy/sites")                
            };
            using (var response = client.Send(request))
            {
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsStringAsync().Result;
                var data = JsonSerializer.Deserialize<List<Site>>(result);
                var monthData = data.Where(x => (DateTime.Now - x.Date).Days <= 120).Select(x => x).OrderByDescending(x => x.Date).ToList();
                _mainContext.AddRange(monthData);
                _mainContext.SaveChangesAsync();
                return View("Index", monthData);
            }
            
        }
    }    
    
}
