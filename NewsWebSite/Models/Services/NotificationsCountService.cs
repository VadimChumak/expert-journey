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
            if (memoryCache.Contains("UserNotificationsCount" + id))
            {
                var count = (int)memoryCache.Get("UserNotificationsCount" + id);
                return count;
            }
            var val = notifiRepo.GetLinesCount(id);
            memoryCache.Add("UserNotificationsCount" + id, val, DateTime.Now.AddMinutes(20));
            return val;
        }

        public void Set(int userId, int value)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (!memoryCache.Contains("UserNotificationsCount" + userId))
            {
                memoryCache.Add("UserNotificationsCount" + userId, value, DateTime.Now.AddMinutes(20));
            }
            else
            {
                memoryCache.Set("UserNotificationsCount" + userId, value, DateTime.Now.AddMinutes(20));
            }
        }

        public void Update(int userId, int value)
        {
            var newVal = GetValue(userId) + value;
            if (newVal < 0) newVal = notifiRepo.GetLinesCount(userId);
            Set(userId, newVal);
        }

        public void Delete(int id)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(id.ToString()))
            {
                memoryCache.Remove("UserNotificationsCount" + id);
            }
        }
    }
}