using NewsWebSite.App_Start;
using NewsWebSite.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace NewsWebSite.Models.Services
{
    public class NotificationsCountService
    {

        public static NotificationsCountService Instance
        {
            get { return (NotificationsCountService)NinjectWebCommon.bootstrapper.Kernel.GetService(typeof(NotificationsCountService)); }
        }
        readonly INotifiactionsRepository notifiRepo;
        public NotificationsCountService(INotifiactionsRepository notifiRepo)
        {
            this.notifiRepo = notifiRepo;
        }

        public int GetValue(int id)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains("UserNitificationsCount" + id.ToString())) return (int)memoryCache.Get("UserNitificationsCount" + id.ToString());
            var val = notifiRepo.GetLinesCount(id);
            memoryCache.Add(id.ToString(), val, DateTime.Now.AddMinutes(20));
            return val;
        }

        public void Set(int userId, int value)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (!memoryCache.Contains(userId.ToString()))
            {
                memoryCache.Add("UserNitificationsCount" + userId.ToString(), value, DateTime.Now.AddMinutes(20));
            }
            else
            {
                memoryCache.Set("UserNitificationsCount" + userId.ToString(), value, DateTime.Now.AddMinutes(20));
            }
        }

        public void Update(int userId, int value)
        {
            var newVal = GetValue(userId) + value;
            if (newVal < 1) newVal = 0;
            Set(userId, newVal);
        }

        public void Delete(int id)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(id.ToString()))
            {
                memoryCache.Remove("UserNitificationsCount" + id.ToString());
            }
        }
    }
}