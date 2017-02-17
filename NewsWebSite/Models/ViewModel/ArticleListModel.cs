using NewsWebSite.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsWebSite.Models.ViewModel
{
    public class ArticleListModel
    {
        public PagedList<Article> ArticleList { get; set; }
        //public int UsierId { get; set; }
        public string Type { get; set; } = "default";
        public int FirstPageLastId { get; set; }
    }
}