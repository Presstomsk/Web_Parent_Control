using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Web_Parent_Control.Database;
using Web_Parent_Control.Models;
using Web_Parent_Control.Services.Abstractions;

namespace Web_Parent_Control.Services
{
    public class Db : IDb
    {
        public void AddToBlockList(string site, User user)
        {
            using (var db = new MainContext())
            {
                var deletedItem = new BlockedItem  
                {
                    Blocked = true,
                    Site = site,
                    UserId = user.Id
                };
                db.BlockedItems.Add(deletedItem);
                db.SaveChanges();
            }
        }

        public void AddUserToDb(string username, string password, string ip)
        {
            using (var db = new MainContext())
            {
                db.AddRange(new User { Login = username, Password = password, ClientPC = ip }); 
                db.SaveChangesAsync();
            }
        }

        public List<DTO> GetActualFiles(string username)
        {
            using (var db = new MainContext())
            {
                var userId = db.Users.AsNoTracking().FirstOrDefault(x => x.Login == username)?.Id;

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

                return files;
            }
        }

        public List<DTO> GetActualSites(string username)
        {
            using (var db = new MainContext())
            {
                var userId = db.Users.AsNoTracking().FirstOrDefault(x => x.Login == username)?.Id;

                var allSites = db.Sites.AsNoTracking().Where(x => x.UserId == userId).Select(x => x).ToList();
                var blockedItems = db.BlockedItems.AsNoTracking().Where(x => x.UserId == userId).Select(x => new { x.Site, x.Blocked }).ToList();
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

                return sites;
            }
        }

        public int GetFileCount(Guid userId)
        {
            using (var db = new MainContext())
            {
                return db.Files.AsNoTracking().Where(x => x.UserId == userId).Count();
            }
        }

        public List<DTO> GetFilteredData(Guid? userId, string period, string action)
        {
            var dict = new Dictionary<string, int>
            {
               { "month", 31 },
               { "week", 7 },
               { "day", 1 }
            };
            using (var db = new MainContext())
            {
                if (action == "History")
                {
                    return db.Sites.AsNoTracking().AsEnumerable().Where(x => x.UserId == userId && (DateTime.Now - x.Date).Days <= dict[period])
                                .Select(x => new DTO { Date = x.Date, Content = x.Url, Blocked = false, Site = x.Host }).OrderByDescending(x => x.Date).ToList();
                }
                else return db.Files.AsNoTracking().AsEnumerable().Where(x => x.UserId == userId && (DateTime.Now - x.Date).Days <= dict[period])
                                .Select(x => new DTO { Date = x.Date, Content = x.Title, Blocked = false, Site = x.Url }).OrderByDescending(x => x.Date).ToList();
            }

        }

        public int GetSiteCount(Guid userId)
        {
            using (var db = new MainContext())
            {
                return db.Sites.AsNoTracking().Where(x => x.UserId == userId).Count();
            }
        }

        public User GetUserFromDb(string username, string password) 
        {
            using (var db = new MainContext())
            {
                return db.Users.FirstOrDefault(p => p.Login == username && p.Password == password); 
            }
        }

        public User GetUserFromDb(string username)
        {
            using (var db = new MainContext())
            {
                return db.Users.FirstOrDefault(p => p.Login == username);
            }
        }

        public void RemoveFromBlockList(string site)
        {
            using (var db = new MainContext())
            {
                var blockedItems = db.BlockedItems.AsNoTracking().Where(x => x.Site == site).ToList();
                db.BulkDelete(blockedItems);
            }
        }
    }
}
