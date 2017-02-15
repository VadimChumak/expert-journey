using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsWebSite.Models.Repository
{
    public class NotifiactionsRepository : INotifiactionsRepository
    {
        readonly ISessionFactory sessionFactory;

        public NotifiactionsRepository(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }




        public Notification GetById(int id)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.Get<Notification>(id);
            }
        }

        public int GetNotViewedCount(int userId)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.CreateCriteria<Notification>()
                    .SetProjection(Projections.RowCount())
                    .Add(Restrictions.Eq("UserId", userId))
                    .Add(Restrictions.Eq("Viewed", false))
                    .UniqueResult<int>();
            }
        }

        public int ViewByContext(int userId, int commentId, int articleId)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    var count = session.CreateCriteria<Notification>()
                        .Add(Restrictions.Eq("UserId", userId))
                        .Add(Restrictions.Eq("CommentId", commentId))
                        .Add(Restrictions.Eq("ArticleId", articleId))
                        .Add(Restrictions.Eq("Viewed", false))
                        .SetProjection(Projections.RowCount())
                        .UniqueResult<int>();
                    if (count > 0)
                    {
                       session.CreateQuery(
                           "update Notification set Viewed = :true where UserId = :userid and CommentId = :commenid and ArticleId = :articleid and Viewed = :false")
                           .SetParameter("true", true)
                           .SetParameter("userid", userId)
                           .SetParameter("commenid", commentId)
                           .SetParameter("articleid", articleId)
                           .SetParameter("false", false)
                           .ExecuteUpdate();
                    }
                    t.Commit();
                    return count;
                }
            }
        }

        public bool View(int userId, int notifiId)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    var item = session.Get<Notification>(notifiId);
                    var isValid = item.UserId == userId && !item.Viewed;
                    if (isValid)
                    {
                        item.Viewed = true;
                        session.SaveOrUpdate(item);
                    }
                    t.Commit();
                    return isValid;
                }
            }
        }

        public IList<Notification> GetNotViewedList(int userId)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.CreateCriteria<Notification>()
                     .Add(Restrictions.Eq("UserId", userId))
                     .Add(Restrictions.Eq("Viewed", false))
                     .List<Notification>();
            }
        }

        public IList<Notification> GetList(int userId)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.CreateCriteria<Notification>()
                    .Add(Restrictions.Eq("UserId", userId))
                    .AddOrder(Order.Desc("Id"))
                    .List<Notification>();
            }
        }

        public int Save(Notification notification)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    session.SaveOrUpdate(notification);
                    t.Commit();
                    return notification.Id;
                }
            }
        }

        public int GetLinesCount(int userId)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.CreateCriteria<Notification>()
                    .SetProjection(Projections.RowCount())
                    .Add(Restrictions.Eq("UserId", userId))
                    .Add(Restrictions.Eq("Viewed", false))
                    .UniqueResult<int>();
            }
        }
    }

    public class NotificationsCriteria
    {
        public NotificationsCriteria(int userId, int startFrom, int lastId, int count)
        {
            UserId = userId;
            StartFrom = startFrom;
            LastId = lastId;
            Count = count;
        }
        public NotificationsCriteria() { }

        public int UserId { get; set; }
        public int StartFrom { get; set; }
        public int LastId { get; set; }
        public int Count { get; set; }
    }
}