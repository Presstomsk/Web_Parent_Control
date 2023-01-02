using System;
using System.Collections.Generic;
using Web_Parent_Control.Models;

namespace Web_Parent_Control.Services.Abstractions
{
    public interface IDb
    {
        User GetUserFromDb(string username, string password);
        User GetUserFromDb(string username);
        void AddUserToDb(string username, string password, string ip);
        int GetSiteCount(Guid userId);
        int GetFileCount(Guid userId);
        List<DTO> GetActualSites(string username);
        List<DTO> GetActualFiles(string username);
        void AddToBlockList(string site, User user);
        void RemoveFromBlockList(string site);
        List<DTO> GetFilteredData(Guid? userId, string period, string action);
    }
}
