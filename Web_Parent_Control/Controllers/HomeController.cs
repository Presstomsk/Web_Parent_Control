using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Web_Parent_Control.Connector;
using Web_Parent_Control.Database;
using Web_Parent_Control.Models;
using SiteModel = Web_Parent_Control.Models.SiteModel;
using Web_Parent_Control.Services.Abstractions;

namespace Web_Parent_Control.Controllers
{
   
    public class HomeController : Controller
    {             
        private readonly ILogger<HomeController> _logger;
        private readonly IAuth _auth;
        private readonly IDb _db;

        public HomeController(ILogger<HomeController> logger, IAuth auth, IDb db)
        {
            _logger = logger;
            _auth = auth;
            _db = db;
        }

        [HttpGet]
        public IActionResult Authorization(string returnUrl) // Вывод формы Аутентификации
        {
            return View("Authorization", returnUrl);
        }

        [HttpPost]
        public IActionResult Authorization(string returnUrl, string username, string password) // Аутентификация
        {
            var passHash = _auth.GetHash(password); //Хешируем пароль MD5 
            var user = _db.GetUserFromDb(username, passHash); // Существует ли пользователь
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
                    _auth.Logout(this);
                }
            }
            var siteCount = _db.GetSiteCount(user.Id); // Существуют ли старые данные в бд пользователя
            var fileCount = _db.GetFileCount(user.Id);
            var crud = new Crud();
            var connector = new HttpConnector();
            var allSites = connector.GetData<List<SiteModel>>($"{user.ClientPC}/ParentSpy/sites");
            var allFiles = connector.GetData<List<FileModel>>($"{user.ClientPC}/ParentSpy/files");

            if (siteCount > 0 || fileCount > 0)
            {
                crud.UpdateDB(user, allSites, allFiles);
            }
            else
            {
                crud.CreateDB(user, allSites, allFiles);
            }
            _auth.Authorization(user.Login, this);
            return Redirect(returnUrl ?? "~/Home/History");
        }

        [HttpGet, Authorize]
        public IActionResult Logout() //Выход
        {
            var userName = HttpContext.User.Identity.Name;
            
            var user = _db.GetUserFromDb(userName);
           
            new Crud().DeleteDB(user);

            _auth.Logout(this);

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
            var user = _db.GetUserFromDb(username); // Пользователь уже существует
            if (user != null)
            {
                ViewBag.Error = "Данный логин зарегистрирован!";
                ViewBag.Color = "red";
                return View();
            }

            var client = new HttpClient();
            try
            {

                var response = client.GetAsync($"{ip}/ParentSpy/echoGet").Result;

                var passHash = _auth.GetHash(password); //Хешируем пароль MD5 
                _db.AddUserToDb(username, passHash, ip); //Добавление нового пользователя в БД                   

                ViewBag.Error = "Регистрация пройдена. Авторизуйтесь!";
                ViewBag.Color = "green";
                return View("Authorization");
            }
            catch (InvalidOperationException ex) // Проверка формата ip Parent Spy
            {
                ViewBag.Error = "Некорректный Parent Spy IP !";
                ViewBag.Color = "red";
                return View();
            }                  
        }


        [HttpGet, Authorize]
        public IActionResult History() // Вывод сайтов
        {
            var userName = HttpContext.User.Identity.Name;

            var sites = _db.GetActualSites(userName);

            var viewModel = new ViewModel
            {
                Data = sites,
                Username = userName
            };
            return View(viewModel);
        }

        [HttpGet, Authorize]
        public IActionResult Downloads() // Вывод файлов
        {
            var userName = HttpContext.User.Identity.Name;

            var files = _db.GetActualFiles(userName);

            var viewModel = new ViewModel
            {
                Data = files,
                Username = userName
            };
            return View(viewModel);
        }      

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("/block"), Authorize]
        public IActionResult Block(string site) // Отмечаем сайты на блокировку
        {
            var userName = HttpContext.User.Identity.Name;
            var user = _db.GetUserFromDb(userName);
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{user.ClientPC}/ParentSpy/block/{site}")
            };

            using (var response = client.Send(request))
            {
                response.EnsureSuccessStatusCode();

                _db.AddToBlockList(site, user); // Сохранение сайта в таблице блокированных сайтов                   
            }

            return Ok();
        }

        [HttpPost("/unblock/{title?}"), Authorize]
        public IActionResult Unblock(string site) // Отмечаем сайты на разблокировку
        {
            var userName = HttpContext.User.Identity.Name;
            var user = _db.GetUserFromDb(userName);
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{user.ClientPC}/ParentSpy/unblock/{site}")
            };

            using (var response = client.Send(request))
            {
                response.EnsureSuccessStatusCode();
                _db.RemoveFromBlockList(site); //Удаление из таблицы блокированных сайтов                                  
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult GetNewData(string period, string action) // Фильтр по периоду
        {
            var userName = HttpContext.User.Identity.Name;

            using (var db = new MainContext())
            {
                var userId = _db.GetUserFromDb(userName)?.Id;                            

                var items = _db.GetFilteredData(userId, period, action);

                var viewModel = new ViewModel
                {
                    Data = items,
                    Username = userName
                };

                return View(action, viewModel);
            }
        }
    }    
    
}
