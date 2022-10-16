using EFCore.BulkExtensions;
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
        public void CreateDB(User user, List<SiteModel> allSites, List<FileModel> allFiles) // Получение данных и формирование БД
        { 
            using (var db = new MainContext())
            {       
                var monthSites = allSites.Where(x => (DateTime.Now - x.Date).Days <= 31).Select(x => x).ToList();
                var monthFiles = allFiles.Where(x => (DateTime.Now - x.Date).Days <= 31).Select(x => x).ToList();

                monthSites.ForEach(x => x.UserId = user.Id);
                monthFiles.ForEach(x => x.UserId = user.Id);

                db.BulkInsertAsync(monthSites);
                db.BulkInsertAsync(monthFiles);
            }

        }        
        public void DeleteDB(User user) // Удаление данных из БД
        {
            using (var db = new MainContext())
            {
                var allSites = db.Sites.AsNoTracking().Where(x => x.UserId == user.Id).ToList();
                var allFiles = db.Files.AsNoTracking().Where(x => x.UserId == user.Id).ToList();

                db.BulkDeleteAsync(allSites);
                db.BulkDeleteAsync(allFiles);
            }

        }
       
        public void UpdateDB(User user, List<SiteModel> allSites, List<FileModel> allFiles) // Обновление данных в БД
        {
            DeleteDB(user);
            CreateDB(user, allSites, allFiles);
        }
    }
}
