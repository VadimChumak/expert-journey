using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using NewsWebSite.Models.SignalrModels;
using Microsoft.AspNet.Identity;
using System.Threading;
using NewsWebSite.Models;
using NewsWebSite.Models.Repository;
using Microsoft.Security.Application;
using System.Threading.Tasks;
using System;
using NewsWebSite.Models.Services;
using System.IO;

namespace NewsWebSite.Hubs
{
    public class CommentsHub : Hub
    {
        readonly IArticleRepository articleRepository;
        readonly IUserRepository usersRepository;
        readonly ICommentsRepository commentsRepository;
        readonly INotifiactionsRepository notoficationsRepository;
        readonly NotificationsCountService notifiCountCache;
        readonly CommentsHelper commentsHelper;

        public CommentsHub(IArticleRepository articleRepo, IUserRepository usersRepo, ICommentsRepository commentsRepo, INotifiactionsRepository notifyRepo)
        {
            commentsHelper = new CommentsHelper();
            notifiCountCache = new NotificationsCountService(notifyRepo);
            articleRepository = articleRepo;
            usersRepository = usersRepo;
            commentsRepository = commentsRepo;
            notoficationsRepository = notifyRepo;
        }

        [Authorize]
        public void Edit(int commentId, string text)
        {
            text = text.Trim();
            if (text.Length <= 0 || text.Length > 250) return;

            var comment = commentsRepository.GetById(commentId);
            if (comment == null || comment.Deleted) return;
            var userIdentityId = Context.User.Identity.GetUserId<int>();
            if (comment.UserId != userIdentityId) return;
            if (comment.Text != text)
            {
                comment.Text = text;
                comment.Created = DateTime.Now;
                commentsRepository.Save(comment);
                Clients.Group("article-" + comment.ArticleId).Edit(commentId, comment.Text, comment.Created.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        [Authorize]
        public void Delete(int commentId)
        {
            var comment = commentsRepository.GetById(commentId);
            if (comment == null || comment.Deleted || Context.User.Identity.GetUserId<int>() != comment.UserId) return;
            comment.Text = null;
            comment.UserId = 0;
            comment.UserName = null;
            comment.Deleted = true;
            comment.UserImage = null;
            commentsRepository.Save(comment);
            Clients.Group("article-" + comment.ArticleId).Delete(commentId);
        }


        //[Authorize]
        public void Send(int articleId = 0, int replyCommentId = 0, string text = "", string sendId = "")
        {
            text = text.Trim();
            if (replyCommentId < 0 || !commentsHelper.ValidateText(text))
            {
                Clients.Caller.Result(0, sendId, "error1", "");
                return;
            }
            var commentDepth = 0;
            var userIdentityId = Context.User.Identity.GetUserId<int>();
            var name = Context.User.Identity.Name.Split('@')[0];
            var UserImage = usersRepository.GetUserImage(userIdentityId);
            if (UserImage == "Default") UserImage = "profile.png";
            else UserImage = userIdentityId.ToString() + "/" + UserImage;
            if (replyCommentId != 0)
            {
                var replyComment = commentsRepository.GetCommentInfo(replyCommentId);
                if (replyComment == null || replyComment.ArticleId != articleId)
                {
                    Clients.Caller.Result(0, sendId, "error2", "");
                    return;
                }
                commentDepth = replyComment.Depth + 1;
                if (userIdentityId != replyComment.UserId)
                {

                    var notificationId = notoficationsRepository.Save(new Notification()
                    {
                        CommentId = replyCommentId,
                        FromWho = name,
                        ArticleId = articleId,
                        Message = text,
                        UserId = replyComment.UserId
                    });

                    Clients.Group("user-" + replyComment.UserId.ToString()).Notify(replyCommentId, name, text, articleId, notificationId);
                    notifiCountCache.Update(replyComment.UserId, +1);
                }
            }
            else
            {
                var authorId = articleRepository.GetUserId(articleId);
                if (authorId == 0)
                {
                    Clients.Caller.Result(0, sendId, "error3", "");
                    return;
                }
                if (authorId != userIdentityId)
                {

                    var notificationId = notoficationsRepository.Save(new Notification()
                    {
                        CommentId = 0,
                        FromWho = name,
                        ArticleId = articleId,
                        Message = text,
                        UserId = authorId
                    });
                    Clients.Group("user-" + authorId.ToString()).Notify(0, name, text, articleId, notificationId);
                    notifiCountCache.Update(authorId, +1);
                }
            }
            Comment comment = new Comment();
            comment.UserId = userIdentityId;
            comment.ArticleId = articleId;
            comment.Text = text;
            comment.UserName = Context.User.Identity.Name.Split('@')[0];
            comment.Depth = commentDepth;
            comment.Created = DateTime.Now;
            comment.ReplyCommentId = replyCommentId;
            comment.Deleted = false;
            comment.UserImage = UserImage;
            var id = commentsRepository.Save(comment);
                                                                                //id, userId, name, message, date, reply, userimage
            Clients.OthersInGroup("article-" + articleId).addMessage(id, userIdentityId, comment.UserName, comment.Text, comment.Created.ToString("yyyy-MM-dd HH:mm:ss"), replyCommentId, UserImage);
            Clients.Caller.Result(id, sendId, "ok", comment.Created.ToString("yyyy-MM-dd HH:mm:ss"));

        }

        public void Connect(int articleId = 0)
        {
            var identity = Context.User.Identity;
            var id = Context.ConnectionId;
            if (articleId > 0) Groups.Add(id, "article-" + articleId);
        }

        public override Task OnConnected()
        {
            var identity = Context.User.Identity;
            if (identity.IsAuthenticated) Groups.Add(Context.ConnectionId, "user-" + identity.GetUserId<int>().ToString());
            return base.OnConnected();
        }
    }


}