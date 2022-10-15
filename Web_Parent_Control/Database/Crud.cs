using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Web_Parent_Control.Connector;
using Web_Parent_Control.Models;

namespace Web_Parent_Control.Database
{
    public class Crud
    {
        public void CreateDB(User user) // Получение данных и формирование БД
        {            
            var connector = new HttpConnector();

            var allSites = connector.GetData<List<SiteModel>>("http://localhost:5100/ParentSpy/sites");
            var allFiles = connector.GetData<List<FileModel>>("http://localhost:5100/ParentSpy/files");

            using (var db = new MainContext())
            {
                var monthSites = allSites.Where(x => (DateTime.Now - x.Date).Days <= 31).Select(x => x).ToList();
                var monthFiles = allFiles.Where(x => (DateTime.Now - x.Date).Days <= 31).Select(x => x).ToList();

                monthSites.ForEach(x => x.UserId = user.Id);
                monthFiles.ForEach(x => x.UserId = user.Id);
               
                db.AddRange(monthSites);
                db.AddRange(monthFiles);
                db.SaveChanges();
            }

        }        
        public void DeleteDB(User user) // Удаление данных из БД
        {
            using (var db = new MainContext())
            {
                var allSites = db.Sites.AsNoTracking().Where(x => x.UserId == user.Id);
                var allFiles = db.Files.AsNoTracking().Where(x => x.UserId == user.Id);
                db.RemoveRange(allSites);
                db.RemoveRange(allFiles);
                db.SaveChangesAsync();
            }

        }
       
        public void UpdateDB(User user) // Обновление данных в БД
        {
            DeleteDB(user);
            CreateDB(user);
        }
    }
}
