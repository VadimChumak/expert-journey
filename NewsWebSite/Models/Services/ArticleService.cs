using NewsWebSite.Models.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;


namespace NewsWebSite.Models.Services
{
    public class ArticleService
    {
        public ArticleService(IArticleRepository aR)
        {
            memoryCache = MemoryCache.Default;
            articleRepo = aR;
            ItemsOnPageCount = int.Parse(ConfigurationManager.AppSettings["NumberOfItemsOnPage"]);
        }
        readonly int ItemsOnPageCount;
        readonly IArticleRepository articleRepo;
        readonly MemoryCache memoryCache;
        public PagedList<Article> GetArticleList(ArticleCriteria cr)
        {
            var key = "allNewsList";
            if (cr.UserId > 0)
            {
                return articleRepo.GetList(cr);
            }

            if (!memoryCache.Contains(key))
            {
                var list = articleRepo.GetList(cr);
                if (cr.LastId == 0)
                {
                    var value = PagedList.Create(list.Select(i => i.Id), list.PageCount, list.LinesCount);
                    memoryCache.Add(key, value, DateTime.Now.AddMinutes(60));
                }
                foreach (var a in list) SaveToCache("article" + a.Id, a);
                return list;
            }
            var linkerList = memoryCache.Get(key) as PagedList<int>;
            var lastIdIndex = linkerList.IndexOf(cr.LastId);
            if (lastIdIndex != -1)
            {
                //0 1 2 3 4 5 6 7 8 9
                var existedInListCount = linkerList.Count - lastIdIndex - 1;
                if (existedInListCount > 9)
                {
                    return new PagedList<Article>(MakeArticleListFromCache(linkerList, lastIdIndex + 1, ItemsOnPageCount));
                }
                if (existedInListCount >= 0)
                {
                    var result = new List<Article>();
                    if (existedInListCount > 0)
                    {
                        result.AddRange(MakeArticleListFromCache(linkerList, lastIdIndex + 1, existedInListCount));
                    }
                    result.AddRange(articleRepo.GetList(new ArticleCriteria()
                    {
                        StartFrom = 0,
                        UserId = 0,
                        LastId = linkerList.Last(),
                        Count = ItemsOnPageCount - existedInListCount
                    }));
                    foreach (var a in result) SaveToCache("article" + a.Id, a);
                    linkerList.AddRange(result.Select(i => i.Id));
                    memoryCache.Set(key, linkerList, DateTime.Now.AddMinutes(60));
                    return new PagedList<Article>(result);
                    // 0 1 2 3 4 5 6 7 8 9
                }
            }
            var res = articleRepo.GetList(cr);
            foreach (var a in res) SaveToCache("article" + a.Id, a);
            return res;

            //throw new NotImplementedException();
        }
        public bool SaveToCache(string key, object item)
        {
            if (!memoryCache.Contains(key))
            {
                memoryCache.Add(key, item, DateTime.Now.AddMinutes(20));
                return true;
            }
            //  memoryCache.Set(key, item, DateTime.Now.AddMinutes(20));
            return false;
        }
        public List<Article> MakeArticleListFromCache(PagedList<int> linkList, int startIndex, int count)
        {
            List<Article> list = new List<Article>();
            for (int i = startIndex; i < startIndex + count && i < linkList.Count; i++)
            {
                list.Add(GetArticleById(linkList[i]));
            }
            return list;
        }


        public Article GetArticleById(int id)
        {
            var key = "article" + id;
            if (!memoryCache.Contains(key))
            {
                var article = articleRepo.GetItem(id);
                memoryCache.Add(key, article, DateTime.Now.AddMinutes(20));
                return article;
            }
            return memoryCache.Get(key) as Article;
        }
        public void AddArticle(int id)
        {
            if (!memoryCache.Contains("allNewsList")) return;
            var linkerList = memoryCache.Get("allNewsList") as PagedList<int>;
            linkerList.Insert(0, id);
            linkerList.LinesCount++;
            linkerList.PageCount = (int)Math.Ceiling(linkerList.LinesCount / (double)ItemsOnPageCount);
            memoryCache.Set("allNewsList", linkerList, DateTime.Now.AddMinutes(60));
        }
        public void DeleteArticle(int id)
        {
            if (!memoryCache.Contains("allNewsList")) return;
            var linkerList = memoryCache.Get("allNewsList") as PagedList<int>;
            linkerList.Remove(id);
            linkerList.LinesCount--;
            linkerList.PageCount = (int)Math.Ceiling(linkerList.LinesCount / (double)ItemsOnPageCount);
            memoryCache.Set("allNewsList", linkerList, DateTime.Now.AddMinutes(60));
        }
    }
}

//0 1 2 3 4 5 6 7 8 9 
