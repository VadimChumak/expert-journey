using NewsWebSite.Models.ViewModel;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsWebSite.Models.Repository
{
    public class ArticleRepository : IArticleRepository
    {
        readonly ISessionFactory sessionFactory;

        public ArticleRepository(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public int Save(Article a)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    var timeNow = DateTime.Now;
                    if (a.CreateDate == DateTime.MinValue)
                        a.CreateDate = timeNow;
                    a.LastUpdateDate = timeNow;
                    session.SaveOrUpdate(a);
                    t.Commit();
                    return a.Id;
                }
            }
        }

        public Article GetItem(int id)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.Get<Article>(id);
            }
        }

        public int GetUserId(int id)
        {
            using (var session = sessionFactory.OpenSession())
            {
                return session.CreateCriteria<Article>()
                     .SetProjection(Projections.Property("UserId"))
                     .Add(Restrictions.IdEq(id))
                     .UniqueResult<int>();
            }
        }

        public bool IsExist(int id)
        {
            using (var session = sessionFactory.OpenSession())
            {
                var count = session.CreateCriteria<Article>()
                    .SetProjection(Projections.RowCount())
                    .Add(Restrictions.IdEq(id))
                    .UniqueResult<int>();
                return count == 1;
            }
        }

        public PagedList<Article> GetList(ArticleCriteria criteria)
        {
            using (var session = sessionFactory.OpenSession())
            {
                var filter = session.CreateCriteria<Article>()
                    .Add(Restrictions.Eq("IsDeleted", false));
                if (criteria.LastId > 0) filter.Add(Restrictions.Lt("Id", criteria.LastId));
                else filter.SetFirstResult(criteria.StartFrom);

                if (criteria.UserId > 0) filter.Add(Restrictions.Eq("UserId", criteria.UserId));

                var countCreteria = (ICriteria)filter.Clone();
                var LinesCount = countCreteria.SetProjection(Projections.RowCount()).UniqueResult<int>();
                var PageCount = (int)Math.Ceiling(LinesCount / (double)criteria.Count);
                var results = PagedList.Create(filter
                .AddOrder(Order.Desc("Id"))
                .SetMaxResults(criteria.Count)

                .List<Article>(), PageCount, LinesCount);

                return results;
            }
        }

        public PagedList<DemoArticle> GetDemoList(ArticleCriteria criteria)
        {
            using (var session = sessionFactory.OpenSession())
            {
                var filter = session.CreateCriteria<Article>()
                    .Add(Restrictions.Eq("IsDeleted", false));
                if (criteria.LastId > 0) filter.Add(Restrictions.Lt("Id", criteria.LastId));
                else filter.SetFirstResult(criteria.StartFrom);

                if (criteria.UserId > 0) filter.Add(Restrictions.Eq("UserId", criteria.UserId));

                var countCreteria = (ICriteria)filter.Clone();

                var LinesCount = countCreteria.SetProjection(Projections.RowCount()).UniqueResult<int>();
                var PageCount = (int)Math.Ceiling(LinesCount / (double)criteria.Count);

                var results = PagedList.Create(filter
                .SetProjection(Projections.ProjectionList()
                .Add(Projections.Id(), "Id")
                .Add(Projections.Property("Title"), "Title")
                .Add(Projections.Property("Image"), "Image")
                .Add(Projections.Property("ShortDescription"), "ShortDescription")
                .Add(Projections.Property("CreateDate"), "CreateDate")
                .Add(Projections.Property("LastUpdateDate"), "LastUpdateDate"))
                .AddOrder(Order.Desc("Id"))
                .SetMaxResults(criteria.Count)
                .SetResultTransformer(Transformers.AliasToBean<DemoArticle>())
                .List<DemoArticle>(), PageCount, LinesCount);
               
                return results;
            }
        }


        public int GetCountOfLines()
        {
            using (var session = sessionFactory.OpenSession())
            {
                var count = session.QueryOver<Article>().Select(Projections.RowCount()).FutureValue<int>().Value;
                return count;
            }
        }

        public PagedList<Article> GetArticleByTags(IEnumerable<Tag> tags, ArticleCriteria cr)
        {
            using (var session = sessionFactory.OpenSession())
            {
                ICriteria filter = session.CreateCriteria<Article>()
                    .Add(Restrictions.Eq("IsDeleted", false));
                if (cr.LastId > 0)
                    filter.Add(Restrictions.Lt("Id", cr.LastId));

                else filter.SetFirstResult(cr.StartFrom);
                filter.CreateAlias("Tags", "tag");
                filter.Add(Restrictions.In("tag.Id", tags.Select(m => m.Id).ToArray()));

                //var result = new PagedList<DemoArticle>();
                var countCreteria = (ICriteria)filter.Clone();
                var LinesCount = countCreteria.SetProjection(Projections.RowCount()).UniqueResult<int>();
                var PageCount = (int)Math.Ceiling(LinesCount / (double)cr.Count);
                var result = PagedList.Create(filter
                //    .SetProjection(Projections.Distinct(Projections.ProjectionList()
                //   .Add(Projections.Id(), "Id")
                //.Add(Projections.Property("Title"), "Title")
                //.Add(Projections.Property("Image"), "Image")
                //.Add(Projections.Property("ShortDescription"), "ShortDescription")
                //.Add(Projections.Property("CreateDate"), "CreateDate")
                //.Add(Projections.Property("LastUpdateDate"), "LastUpdateDate")))
                .AddOrder(Order.Desc("Id"))
                .SetMaxResults(cr.Count)
                .List<Article>(), PageCount, LinesCount);
                
                return result;
            }
        }

        public void Delete(Article a)
        {
            a.IsDeleted = true;
            using (var session = sessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    session.SaveOrUpdate(a);
                    t.Commit();
                }
            }
        }

        public void Restore(Article a)
        {
            a.IsDeleted = false;
            using (var session = sessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    session.SaveOrUpdate(a);
                    t.Commit();
                }
            }
        }
    }
}



public class ArticleCriteria
{
    public int StartFrom { get; set; }
    public int Count { get; set; }
    public int LastId { get; set; }
    public int UserId { get; set; }
    public ArticleCriteria()
    {
        Count = 10;
        UserId = 0;
    }
}
