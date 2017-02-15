using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NewsWebSite.Models.ViewModel
{

    public class ArticleForView
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string Image { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Editable { get; set; }
        public int CurUserId { get; set; }
        public string CurUserName { get; set; }
        public string CurUserImage { get; set; }
        public int CommentId { get; set; }


        [UIHint("TagList")]
        public ISet<Tag> ArticleTags { get; set; }

        public ArticleForView(Article a)
        {
            Id = a.Id;
            Title = a.Title;
            FullDescription = a.FullDescription;
            Image = a.Image;
            ArticleTags = a.Tags;
            ShortDescription = a.ShortDescription;
            CreateDate = a.CreateDate;
            UpdateDate = (a.CreateDate != a.LastUpdateDate ? a.LastUpdateDate : a.CreateDate);
            Editable = false;
        }
        public ArticleForView() { }
    }

}