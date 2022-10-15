using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using Web_Parent_Control.Connector;
using Web_Parent_Control.Database;
using Web_Parent_Control.Models;
using SiteModel = Web_Parent_Control.Models.SiteModel;

namespace Web_Parent_Control.Controllers
{
   
    public class HomeController : Controller
    {             
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;            
        }

        [HttpGet]
        public IActionResult Start(string returnUrl) // Вывод формы Аутентификации
        {
            return View("Authorization", returnUrl);
        }

        [HttpPost]
        public IActionResult Authorization(string returnUrl, string username, string password) // Аутентификация
        {
            using (var db = new MainContext())
            {
                var user = db.Users.FirstOrDefault(p => p.Login == username && p.Password == password); // Существует ли пользователь
                if (user == null)
                {
                    ViewBag.Error = "Данного пользователя не существует";                    
                    return View();
                    
                }
                if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated) // Аутентифицирован ли пользователь
                {
                    if (HttpContext.User.Identity.Name == username)
                    {
                        return Redirect("~/Home/History");
                    }
                    else
                    {
                        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                }
                var siteCount = db.Sites.AsNoTracking().Where(x => x.UserId == user.Id).Count(); // Существуют ли старые данные в бд пользователя
                var fileCount = db.Files.AsNoTracking().Where(x => x.UserId == user.Id).Count();
                var crud = new Crud();
                if (siteCount > 0 || fileCount > 0)
                {
                    crud.UpdateDB(user);
                }
                else 
                {
                    crud.CreateDB(user);
                }                              
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Login) };
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return Redirect(returnUrl ?? "~/Home/History");
            }
        }

        [HttpPost]
        public IActionResult Logout() //Выход
        {
            //var db = new Crud();            
            //db.DeleteDB()
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("Authorization");
        }

        [HttpGet]
        public IActionResult Registration() // Форма регистрации
        {
            return View();
        }


        [HttpGet, Authorize]
        public IActionResult History() // Вывод сайтов
        {       

            using (var db = new MainContext())
            {
                var sites = db.Sites.AsNoTracking().OrderByDescending(x => x.Date).ToList();                
                return View(sites);
            }           

        }

        [HttpGet, Authorize]
        public IActionResult Downloads() // Вывод файлов
        {            
            using (var db = new MainContext())
            {
                var files = db.Files.AsNoTracking().OrderByDescending(x => x.Date).ToList();
                return View(files);
            }
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

        //[HttpGet]
        //public IActionResult Action(Guid id) // Отмечаем сайты на блокировку
        //{

        //    var site = _mainContext.Sites.SingleOrDefault(x => x.Id == id);

        //    if (site.Flag == false)
        //    {
        //        site.Flag = true;
        //    }
        //    else site.Flag = false;

        //    _mainContext.SaveChangesAsync();

        //    var sites = _mainContext.Sites.Select(x => x).ToList();
        //    return View("Index", sites);
        //}

        //[HttpPost]
        //public IActionResult GetNewData(string period) // Фильтр по периоду
        //{
        //    if (period == "month")
        //    {
        //        var sites = _mainContext.Sites.ToList();
        //        return View("Index", sites);
        //    }
        //    else if (period == "week")
        //    {
        //        var sites = _mainContext.Sites.Where(x => (DateTime.Now - x.Date).Days <= 7).Select(x => x).ToList();
        //        return View("Index", sites);
        //    }
        //    else
        //    {
        //        var sites = _mainContext.Sites.Where(x => (DateTime.Now - x.Date).Days <= 1).Select(x => x).ToList();
        //        return View("Index", sites);
        //    }
        //}
    }    
    
}
