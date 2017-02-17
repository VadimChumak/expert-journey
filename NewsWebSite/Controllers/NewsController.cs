using NewsWebSite.Models.ViewModel;
using NewsWebSite.Models;
using Newtonsoft.Json;
using NHibernate.Criterion;
using NHibernate.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Microsoft.AspNet.Identity;
using NewsWebSite.Models.Repository;
using NewsWebSite.Models.Services;
using System.Runtime.Caching;

namespace NewsWebSite.Controllers
{
    public class NewsController : Controller
    {
        readonly int NumberOfItemsOnPage = int.Parse(ConfigurationManager.AppSettings["NumberOfItemsOnPage"]);
        readonly IArticleRepository articleRepository;
        readonly ICommentsRepository commentsRepository;
        readonly INotifiactionsRepository notificationRepo;
        readonly ITagRepository tagRepo;
        readonly IUserRepository userRepo;

        readonly ArticleService articleService;
        readonly NotificationsCountService notifiCountCache;
        public NewsController(
            NotificationsCountService notifiCountCache,
            IArticleRepository repo,
            IUserRepository userRepo,
            ITagRepository tagRepo,
            ICommentsRepository commentsRepository,
            ArticleService articleService,
        INotifiactionsRepository notifiRepo)
        {
            this.notifiCountCache = notifiCountCache;
            this.articleService = articleService;
            //service = new ArticleService(repo);
            notificationRepo = notifiRepo;
            this.userRepo = userRepo;
            this.tagRepo = tagRepo;
            articleRepository = repo;
            this.commentsRepository = commentsRepository;
        }

        #region ForDebug
        public string ClearCache()
        {
            var cache = MemoryCache.Default;
            if (cache.Contains("allNewsList"))
            {
                cache.Remove("allNewsList");
                return "contains";
            }
            return "nocache";
        }

        [HttpGet]
        public ActionResult CreateNoImageLines(int n = 0)
        {

            for (int i = 1; i <= n; i++)
            {
                var a = new Article();
                a.Title = i.ToString();
                a.FullDescription = a.Title;
                a.UserId = 11;
                a.Image = null;
                articleService.AddArticle(a);

            }
            return Content("ok");
        }

        [HttpGet]
        public ActionResult CreateLines(int n = 0)
        {

            for (int i = 1; i <= n; i++)
            {
                var a = new Article();
                a.Title = i.ToString();
                a.FullDescription = a.Title;
                a.Image = null;
                a.UserId = 11;
                articleService.AddArticle(a);

            }
            return Content("ok");
        }

        #endregion

        [HttpGet]
        public ActionResult Index(bool isUserNews = false, bool isInterestingNews = false)
        {
            //System.Text.RegularExpressions.Regex.Replace()
            var list = new PagedList<Article>();
            int userId = 0;
            if (!User.Identity.IsAuthenticated && (isInterestingNews || isUserNews))
            {
                return RedirectToRoute("AllNews");
            }
            AppUser currentUser = userRepo.GetById(User.Identity.GetUserId<int>());
            if (!isInterestingNews)
            {
                if (isUserNews) userId = currentUser.Id;
                list = articleService.GetArticleList(new ArticleCriteria() { StartFrom = 0, UserId = userId, Count = NumberOfItemsOnPage, LastId = 0 });
            }
            else
            {
                list = articleRepository.GetArticleByTags(currentUser.Tags, new ArticleCriteria()
                {
                    StartFrom = 0,
                    UserId = 0,
                    Count = NumberOfItemsOnPage,
                    LastId = 0
                });
            }
            var model = new ArticleListModel();

            //model.UsierId = userId;
            model.Type = "default";
            if (isInterestingNews) model.Type = "tags";
            else if (isUserNews) model.Type = "my";
            model.ArticleList = list;
            model.FirstPageLastId = model.ArticleList.Count > 0 ? model.ArticleList[model.ArticleList.Count - 1].Id : 0;
            return View(model);
        }

        [HttpGet]
        public ActionResult Article(int id = 0, int commentId = -1)
        {
            if (id < 1) return HttpNotFound();

            if (commentId >= 0)
            {
                var count = notificationRepo.ViewByContext(User.Identity.GetUserId<int>(), commentId, id);
                if (count > 0)
                    notifiCountCache.Update(User.Identity.GetUserId<int>(), -count);
            }

            var article = articleService.GetArticleById(id);
            if (article == null) return HttpNotFound();
            if (article.IsDeleted == true) return View("ArticleDeleted");
            var viewArticle = new ArticleForView(article);

            if (User.Identity.IsAuthenticated)
            {
                if (commentId >= 0)
                {
                    var count = notificationRepo.ViewByContext(User.Identity.GetUserId<int>(), commentId, id);
                    if (count > 0)
                        notifiCountCache.Update(User.Identity.GetUserId<int>(), -count);
                    viewArticle.CommentId = commentId;
                }
                else viewArticle.CommentId = 0;
                viewArticle.CurUserName = User.Identity.Name.Split('@')[0];
                if (article.UserId == User.Identity.GetUserId<int>())
                    viewArticle.Editable = true;
                viewArticle.CurUserId = User.Identity.GetUserId<int>();
                var userImage = userRepo.GetUserImage(viewArticle.CurUserId);
                if (string.IsNullOrEmpty(userImage)) viewArticle.CurUserImage = "profile.png";
                else viewArticle.CurUserImage = viewArticle.CurUserId + "/" + userImage;
            }
            else
            {
                viewArticle.CurUserImage = "";
                viewArticle.CurUserName = "";
                viewArticle.CurUserId = 0;
            }
            ViewBag.MaxCommentLength = int.Parse(ConfigurationManager.AppSettings["MaxCommentLength"]);
            return View(viewArticle);

        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateArticle()
        {
            CreateArticleModel createArticle = new CreateArticleModel { AllTags = tagRepo.GetAllTags() };
            return View(createArticle);
        }


        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateArticle(CreateArticleModel article, string[] tags)
        {
            if (!ModelState.IsValid) return View(article);
            tags = tags ?? new string[0];
            Article newArticle = new Article
            {
                Title = article.Title,
                ShortDescription = article.ShortDescription,
                FullDescription = article.FullDescription,
                UserId = User.Identity.GetUserId<int>()
            };

            if (article.Image != null)
            {
                newArticle.Image = article.Image.FileName;
            }
            else newArticle.Image = null;
            newArticle.Tags.Clear();
            IEnumerable<Tag> articleTags = TagsHelper.CreateTagList(tags, tagRepo);
            TagsHelper.SetTagForModel(newArticle, articleTags);
            newArticle.Url = UrlHelper.ValiateUrl(newArticle.Title);
            var id = articleService.AddArticle(newArticle);
            if (newArticle.Image != null)
            {
                FileHelper fileHelper = new FileHelper();
                fileHelper.SaveFIle(Server.MapPath(ConfigurationManager.AppSettings["ArticleImagesFolder"]), article.Image, id);
            }

            return RedirectToAction("Article", new { Title = newArticle.Url, Id = id });
        }


        [HttpGet]
        [Authorize]
        public ActionResult EditArticle(int id = 0)
        {
            if (id < 1) return HttpNotFound();
            var article = articleRepository.GetItem(id);
            if (article == null) return HttpNotFound();
            EditArticleModel editArticle = new EditArticleModel(article);
            editArticle.AllTags = tagRepo.GetAllTags();
            if (article == null || article.UserId != User.Identity.GetUserId<int>()) return HttpNotFound();
            return View(editArticle);
        }


        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult EditArticle(EditArticleModel edited, string[] tags, string imageState)
        {

            if (!ModelState.IsValid) return View(edited);
            var baseArticle = articleRepository.GetItem(edited.Id);

            if (baseArticle == null || baseArticle.UserId != User.Identity.GetUserId<int>()) return HttpNotFound();

            var changesExist = false;
            if (imageState == "Empty")
            {
                baseArticle.Image = null;
                changesExist = true;
            }


            if (edited.Image != null)
            {
                var fileHelper = new FileHelper();
                var isChanged = fileHelper.SaveFIle(Server.MapPath(ConfigurationManager.AppSettings["ArticleImagesFolder"]), edited.Image, baseArticle.Id);
                if (isChanged)
                {
                    baseArticle.Image = edited.Image.FileName;
                    changesExist = true;
                }
            }
            if (baseArticle.Title != edited.Title)
            {
                baseArticle.Title = edited.Title;
                changesExist = true;
            }
            if (baseArticle.ShortDescription != edited.ShortDescription)
            {
                baseArticle.ShortDescription = edited.ShortDescription;
                changesExist = true;
            }
            if (baseArticle.FullDescription != edited.FullDescription)
            {
                baseArticle.FullDescription = edited.FullDescription;
                changesExist = true;
            }
            baseArticle.Tags.Clear();
            if (tags != null)
            {
                IEnumerable<Tag> newTags = TagsHelper.CreateTagList(tags, tagRepo);
                TagsHelper.SetTagForModel(baseArticle, newTags);
                changesExist = true;
            }
            if (changesExist)
            {
                articleService.EditArticle(baseArticle);
            }
            return RedirectToAction("Article", new { Title = baseArticle.Url, Id = edited.Id });
        }

      

        [HttpGet]
        [Authorize]
        public ActionResult Delete(int id)
        {
            var authorId = articleRepository.GetUserId(id);
            if (authorId == User.Identity.GetUserId<int>())
            {
                var article = articleRepository.GetItem(id);
                //articleRepository.Delete(article);
                articleService.DeleteArticle(article);
            }
            return View("Restore", id);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Restore(int id)
        {
            var authorId = articleRepository.GetUserId(id);
            if (authorId == User.Identity.GetUserId<int>())
            {
                var article = articleRepository.GetItem(id);
                articleRepository.Restore(article);
                return View("Article", new ArticleForView(article));
            }
            return View("Index");
        }
        #region ForAjaxRequests

        [HttpPost]
        public string GetArticles(int page = 1, int n = 1, int lastId = 0, int userId = 0, string type = "")
        {
            if (page < 1) return "";
            var cr = new ArticleCriteria() { StartFrom = page * NumberOfItemsOnPage, UserId = 0, Count = n * NumberOfItemsOnPage, LastId = lastId };
            var byTags = false;
            var list = new PagedList<Article>();
            if (User.Identity.IsAuthenticated)
            {
                var userIdentityId = User.Identity.GetUserId<int>();
                if (type == "tags")
                {
                    byTags = true;
                    cr.UserId = userIdentityId;
                    var currentUser = userRepo.GetById(userIdentityId);
                    var tags = currentUser.Tags;
                    list = articleRepository.GetArticleByTags(tags, cr);

                }
                else
                if (type == "my")
                {
                    cr.UserId = userIdentityId;
                }
            }
            if (!byTags) list = articleService.GetArticleList(cr);
            return JsonConvert.SerializeObject(list.Select(a => new
            {
                a.Id,
                a.Title,
                a.UserId,
                a.Image,
                a.ShortDescription,
                a.CreateDate,
                a.LastUpdateDate,
                a.Url
            }));
        }

        [HttpPost]
        public string GetComments(int articleId)
        {
            var list = commentsRepository.GetList(articleId);
            return JsonConvert.SerializeObject(list);
        }

        #endregion
    }
}