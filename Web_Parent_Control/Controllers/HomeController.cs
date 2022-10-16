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
using System.Security.Policy;
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
        public IActionResult Authorization(string returnUrl) // Вывод формы Аутентификации
        {
            return View(returnUrl);
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
                    ViewBag.Color = "red";
                    return View();
                    
                }
                if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated) // Аутентифицирован ли пользователь
                {
                    if (HttpContext.User.Identity.Name == username)
                    {
                        return Redirect($"~/Home/History?userId={user.Id}");
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
                    var connector = new HttpConnector();
                    var allSites = connector.GetData<List<SiteModel>>($"{user.ClientPC}/ParentSpy/sites");
                    var allFiles = connector.GetData<List<FileModel>>($"{user.ClientPC}/ParentSpy/files");                   
                    
                    crud.UpdateDB(user, allSites, allFiles);                   
                }
                else 
                {                   
                    var connector = new HttpConnector();
                    var allSites = connector.GetData<List<SiteModel>>($"{user.ClientPC}/ParentSpy/sites");
                    var allFiles = connector.GetData<List<FileModel>>($"{user.ClientPC}/ParentSpy/files");
                   
                    crud.CreateDB(user, allSites, allFiles);                    
                }                              
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Login) };
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return Redirect($"~/Home/History?userId={user.Id}");
            }
        }

        [HttpPost]
        public IActionResult Logout(Guid? userId) //Выход
        {
            User user;

            using (var db = new MainContext())
            {
                user = db.Users.FirstOrDefault(x => x.Id == userId);
            }
           
            new Crud().DeleteDB(user);
           
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("Authorization");
        }

        [HttpGet]
        public IActionResult Registration() // Форма регистрации
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(string username, string password, string repeatPassword, string ip) // Регистрация
        {
            if (password != repeatPassword) //Пароль неккоректный
            {
                ViewBag.Error = "Пароли не идентичны!";
                ViewBag.Color = "red";
                return View();
            }
            using (var db = new MainContext())
            {
                var user = db.Users.FirstOrDefault(p => p.Login == username); // Пользователь уже существует
                if (user != null)
                {
                    ViewBag.Error = "Пользователь с таким логином уже существует!";
                    ViewBag.Color = "red";
                    return View();
                }
            }
            var client = new HttpClient();
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{ip}/ParentSpy/echo")
                };
                var response = client.Send(request);
                if (!response.IsSuccessStatusCode)     // Отсутствует подключение к Parent Spy 
                {
                    ViewBag.Error = "Отсутствует подключение к Parent Spy!";
                    return View();
                }

                using (var db = new MainContext())
                {
                    db.AddRange(new User {Login = username, Password = password, ClientPC = ip }); //Добавление нового пользователя в БД
                    db.SaveChangesAsync();

                    ViewBag.Error = "Регистрация пройдена. Авторизуйтесь!";
                    ViewBag.Color = "green";
                    return View("Authorization");
                }
            }
            catch (UriFormatException ex) // Проверка формата ip Parent Spy
            {
                ViewBag.Error = "Формат Parent Spy IP некорректный!";
                ViewBag.Color = "red";
                return View();
            }
            catch (HttpRequestException ex) // Отсутствует подключение к Parent Spy 
            {
                ViewBag.Error = "Отсутствует подключение к Parent Spy!";
                ViewBag.Color = "red";
                return View();
            }            
        }


        [HttpGet,Authorize]
        public IActionResult History(Guid? userId) // Вывод сайтов
        {       

            using (var db = new MainContext())
            {
                var sites = db.Sites.AsNoTracking().Where(x => x.UserId == userId)
                                    .Select(x => new DTO {Date = x.Date, Content = x.Url, UserId = userId }).OrderByDescending(x => x.Date).ToList();                      
                return View(sites);
            }           

        }

        [HttpGet,Authorize]
        public IActionResult Downloads(Guid? userId) // Вывод файлов
        {            
            using (var db = new MainContext())
            {
                var files = db.Files.AsNoTracking().Where(x => x.UserId == userId)
                                    .Select(x => new DTO { Date = x.Date, Content = x.Title, UserId = userId }).OrderByDescending(x => x.Date).ToList();                
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
