using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Web_Parent_Control.Connector;
using Web_Parent_Control.Database;
using Web_Parent_Control.Models;
using SiteModel = Web_Parent_Control.Models.SiteModel;
using System.Security.Policy;
using EFCore.BulkExtensions;

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
                    ViewBag.Color = "red";
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
                return Redirect(returnUrl ?? "~/Home/History");
            }
        }

        [HttpGet, Authorize]
        public IActionResult Logout() //Выход
        {
            var userName = HttpContext.User.Identity.Name;
            User user;

            using (var db = new MainContext())
            {
                user = db.Users.AsNoTracking().FirstOrDefault(x => x.Login == userName);
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
                    ViewBag.Error = "Данный логин зарегистрирован!";
                    ViewBag.Color = "red";
                    return View();
                }
            }
            var client = new HttpClient();
            try
            {

                var response = client.GetAsync($"{ip}/ParentSpy/echoGet").Result;
                //if (!response.IsSuccessStatusCode)     // Отсутствует подключение к Parent Spy 
                //{
                //    ViewBag.Error = "Отсутствует подключение к Parent Spy!";
                //    return View();
                //}

                using (var db = new MainContext())
                {
                    db.AddRange(new User {Login = username, Password = password, ClientPC = ip }); //Добавление нового пользователя в БД
                    db.SaveChangesAsync();

                    ViewBag.Error = "Регистрация пройдена. Авторизуйтесь!";
                    ViewBag.Color = "green";
                    return View("Authorization");
                }
            }
            catch (InvalidOperationException ex) // Проверка формата ip Parent Spy
            {
                ViewBag.Error = "Некорректный Parent Spy IP !";
                ViewBag.Color = "red";
                return View();
            }
            //catch (AggregateException ex) // Отсутствует подключение к Parent Spy 
            //{
            //    ViewBag.Error = "Некорректный Parent Spy IP !";
            //    ViewBag.Color = "red";
            //    return View();
            //}            
        }


        [HttpGet, Authorize]
        public IActionResult History() // Вывод сайтов
        {
            var userName = HttpContext.User.Identity.Name;

            using (var db = new MainContext())
            {
                var userId = db.Users.AsNoTracking().FirstOrDefault(x => x.Login == userName)?.Id;

                var allSites = db.Sites.AsNoTracking().Where(x => x.UserId == userId).Select(x => x ).ToList();
                var blockedItems = db.BlockedItems.AsNoTracking().Where(x => x.UserId == userId).Select(x => new {x.Site, x.Blocked}).ToList();
                var blockedSites = allSites.Join(blockedItems,
                                                a => a.Host,
                                                b => b.Site,
                                                (a, b) => new DTO
                                                {
                                                    Date = a.Date,
                                                    Content = a.Url,
                                                    Blocked = b.Blocked,
                                                    Site = b.Site
                                                }).ToList();
                var unblockedItems = allSites.Select(x => x.Host).Except(blockedItems.Select(x => x.Site));
                var unblockedSites = allSites.Join(unblockedItems,
                                                   a => a.Host,
                                                   b => b,
                                                   (a, b) => new DTO
                                                   {
                                                       Date = a.Date,
                                                       Content = a.Url,
                                                       Blocked = false,
                                                       Site = b
                                                   }).ToList();

                var sites = blockedSites.Union(unblockedSites).OrderByDescending(x => x.Date).ToList();
                                    
                var viewModel = new ViewModel
                {
                    Data = sites,
                    Username = userName
                };
                return View(viewModel);
            }           

        }

        [HttpGet, Authorize]
        public IActionResult Downloads() // Вывод файлов
        {
            var userName = HttpContext.User.Identity.Name;

            using (var db = new MainContext())
            {
                var userId = db.Users.AsNoTracking().FirstOrDefault(x => x.Login == userName)?.Id;

                var allFiles = db.Files.AsNoTracking().Where(x => x.UserId == userId).Select(x => x).ToList();
                var blockedItems = db.BlockedItems.AsNoTracking().Where(x => x.UserId == userId).Select(x => new { x.Site, x.Blocked }).ToList();
                var blockedFiles = allFiles.Join(blockedItems,
                                                a => (new Uri(a.Url)).Host,
                                                b => b.Site,
                                                (a, b) => new DTO
                                                {
                                                    Date = a.Date,
                                                    Content = a.Title,
                                                    Blocked = b.Blocked,
                                                    Site = b.Site
                                                }).ToList();
                var unblockedItems = allFiles.Select(x => (new Uri(x.Url)).Host).Except(blockedItems.Select(x => x.Site));
                var unblockedFiles = allFiles.Join(unblockedItems,
                                                   a => (new Uri(a.Url)).Host,
                                                   b => b,
                                                   (a, b) => new DTO
                                                   {
                                                       Date = a.Date,
                                                       Content = a.Title,
                                                       Blocked = false,
                                                       Site = b
                                                   }).ToList();

                var files = blockedFiles.Union(unblockedFiles).OrderByDescending(x => x.Date).ToList();

                var viewModel = new ViewModel
                {
                    Data = files,
                    Username = userName
                };
                return View(viewModel);               
            }
        }      

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("/block"), Authorize]
        public IActionResult Block(string site) // Отмечаем сайты на блокировку
        {           
            using (var db = new MainContext())
            {
                var userName = HttpContext.User.Identity.Name;
                var user = db.Users.FirstOrDefault(p => p.Login == userName);
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{user.ClientPC}/ParentSpy/block/{site}")
                };

                using (var response = client.Send(request))
                {
                    response.EnsureSuccessStatusCode();
                    var deletedItem = new BlockedItem  // Сохранение сайта в таблице блокированных сайтов
                    {
                        Blocked = true,
                        Site = site,
                        UserId = user.Id
                    };
                    db.BlockedItems.Add(deletedItem);
                    db.SaveChanges();
                }
            }

            return Ok();
        }

        [HttpPost("/unblock/{title?}"), Authorize]
        public IActionResult Unblock(string site) // Отмечаем сайты на разблокировку
        {
            
            using (var db = new MainContext())
            {
                var userName = HttpContext.User.Identity.Name;
                var user = db.Users.FirstOrDefault(p => p.Login == userName);
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{user.ClientPC}/ParentSpy/unblock/{site}")
                };

                using (var response = client.Send(request))
                {
                    response.EnsureSuccessStatusCode();
                    var blockedItems = db.BlockedItems.AsNoTracking().Where(x => x.Site == site).ToList(); //Удаление из таблицы блокированных сайтов
                                                                     
                    db.BulkDelete(blockedItems);                  
                }
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult GetNewData(string period, string action) // Фильтр по периоду
        {
            var userName = HttpContext.User.Identity.Name;

            using (var db = new MainContext())
            {
                var userId = db.Users.AsNoTracking().FirstOrDefault(x => x.Login == userName)?.Id;
                List<DTO> items = default;
                ViewModel viewModel = default;

                if (period == "month")
                {
                    if (action == "History")
                    {
                        items = db.Sites.AsNoTracking().Where(x => x.UserId == userId)
                                    .Select(x => new DTO { Date = x.Date, Content = x.Url, Blocked = false, Site =x.Host  }).OrderByDescending(x => x.Date).ToList();
                    }
                    else
                    {
                        items = db.Files.AsNoTracking().Where(x => x.UserId == userId)
                                    .Select(x => new DTO { Date = x.Date, Content = x.Title, Blocked = false, Site = x.Url  }).OrderByDescending(x => x.Date).ToList();
                    }                   
                }
                else if (period == "week")
                {
                    if (action == "History")
                    {
                        items = db.Sites.AsNoTracking().AsEnumerable().Where(x => x.UserId == userId && (DateTime.Now - x.Date).Days <= 7)
                                    .Select(x => new DTO { Date = x.Date, Content = x.Url, Blocked = false, Site = x.Host }).OrderByDescending(x => x.Date).ToList();
                    }
                    else items = db.Files.AsNoTracking().AsEnumerable().Where(x => x.UserId == userId && (DateTime.Now - x.Date).Days <= 7)
                                    .Select(x => new DTO { Date = x.Date, Content = x.Title, Blocked = false, Site = x.Url }).OrderByDescending(x => x.Date).ToList();                                 
                }
                else
                {
                    if (action == "History")
                    {
                        items = db.Sites.AsNoTracking().AsEnumerable().Where(x => x.UserId == userId && (DateTime.Now - x.Date).Days <= 1)
                                    .Select(x => new DTO { Date = x.Date, Content = x.Url, Blocked = false , Site = x.Host }).OrderByDescending(x => x.Date).ToList();
                    }
                    else items = db.Files.AsNoTracking().AsEnumerable().Where(x => x.UserId == userId && (DateTime.Now - x.Date).Days <= 1)
                                    .Select(x => new DTO { Date = x.Date, Content = x.Title, Blocked = false, Site = x.Url }).OrderByDescending(x => x.Date).ToList();                                     
                }

                viewModel = new ViewModel
                {
                    Data = items,
                    Username = userName
                };

                return View(action, viewModel);
            }
        }
    }    
    
}
