﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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

        [NonAction]
        public void CreateDB() // Получение данных и формирование БД
        {
            //var result = HttpContext.User.Identity;
            var connector = new HttpConnector();

            var allSites = connector.GetData<List<SiteModel>>("http://localhost:5100/ParentSpy/sites");
            var allFiles = connector.GetData<List<FileModel>>("http://localhost:5100/ParentSpy/files");

            using (var db = new MainContext())
            {
               var user = new User { Login = "Roman", Password = "Pass", ClientPC = "http://localhost:5100" };          

               user.Sites = allSites.Where(x => (DateTime.Now - x.Date).Days <= 31).Select(x => x).OrderByDescending(x => x.Date).ToList();
               user.Files = allFiles.Where(x => (DateTime.Now - x.Date).Days <= 31).Select(x => x).OrderByDescending(x => x.Date).ToList();                

               db.AddRange(user);               
               db.SaveChangesAsync();
            }              
                       
        }

        [HttpGet]
        public IActionResult History() // Вьюха сайта
        {
            CreateDB();

            using (var db = new MainContext())
            {
                var sites = db.Sites.AsNoTracking().OrderByDescending(x => x.Date).ToList();                
                return View(sites);
            }           

        }

        [HttpGet]
        public IActionResult Downloads() // Вьюха сайта
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
