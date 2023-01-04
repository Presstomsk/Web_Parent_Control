using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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

        public List<DTO> GetBlockedSites(string username)
        {
            using (var db = new MainContext())
            {
                var userId = db.Users.AsNoTracking().FirstOrDefault(x => x.Login == username)?.Id;

                var blockedItems = db.BlockedItems.AsNoTracking().Where(x => x.UserId == userId).Select(x => new { x.BlockDate, x.Site, x.Blocked }).ToList();

                return blockedItems.Select(x => new DTO { Date = x.BlockDate, Content = x.Site, Blocked = x.Blocked, Site = x.Site }).ToList();
            }
        }

        public int GetFileCount(Guid userId)
        {
            using (var db = new MainContext())
            {
                return db.Files.AsNoTracking().Where(x => x.UserId == userId).Count();
            }
        }

        public List<DTO> GetFilteredData(User user, string period, string action)
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
                    var sites = GetActualSites(user.Login);
                    return sites.Where(x => (DateTime.Now - x.Date).Days <= dict[period]).OrderByDescending(x => x.Date).ToList();
                }
                else if (action == "Downloads")
                {
                    var files = GetActualFiles(user.Login);
                    return files.Where(x => (DateTime.Now - x.Date).Days <= dict[period]).OrderByDescending(x => x.Date).ToList();
                }
                else 
                {
                    var blocked = GetBlockedSites(user.Login);
                    return blocked.Where(x => (DateTime.Now - x.Date).Days <= dict[period]).OrderByDescending(x => x.Date).ToList();
                }               
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

        public void UpdateUserInDb(string userName, string hashPass)
        {
            using (var db = new MainContext())
            {
                var user = db.Users.FirstOrDefault(x => x.Login == userName);
                user.Password = hashPass;
                db.SaveChanges();
            }
        }
    }
}
