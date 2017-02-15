using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsWebSite.Models.Repository
{
    public interface INotifiactionsRepository
    {
        int Save(Notification notification);
        Notification GetById(int id);
        int GetLinesCount(int userId);
        IList<Notification> GetNotViewedList(int userId);
        IList<Notification> GetList(int userId);
        int ViewByContext(int userId, int commentId, int articleId);
        bool View(int userId, int notifiId);
    }
}
