using Microsoft.Security.Application;
using NewsWebSite.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NewsWebSite.Models.ViewModel
{
    public class CreateArticleModel
    {

        //public int Id { get; set; }
        //string _Title;
        [Required]
        [Display(Name = "Заголовок")]
        [StringLength(150, ErrorMessage = "Description Max Length is 150")]
        public string Title { get; set; }
        // public string Title { get { return _Title; } set { _Title = Sanitizer.GetSafeHtmlFragment(value).Replace("&#1084;", "м"); } }

        //string _ShortDescription;
        [Required]
        [Display(Name = "Краткое описание статьи")]
        [StringLength(250, ErrorMessage = "Максимальная длина описания статьи 250 символов")]
        public string ShortDescription { get; set; }
        //public string ShortDescription { get { return _ShortDescription; } set { _ShortDescription = Sanitizer.GetSafeHtmlFragment(value).Replace("&#1084;", "м"); } }

        string _FullDescription;
        [Required]
        [Display(Name = "Текст статьи")]
        [DataType(DataType.MultilineText)]
        [StringLength(20000, ErrorMessage = "Description Max Length is 20000")]
        public string FullDescription { get { return _FullDescription; } set { _FullDescription = Sanitizer.GetSafeHtmlFragment(value); } }

        public string Tags { get; set; }

        [Display(Name = "Изображение")]
        [ValidImage]
        [AllowedExtensions(new string[] { ".jpg", ".png" })]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase Image { get; set; }

        [Display(Name = "Теги статьи")]
        [UIHint("TagList")]
        public IEnumerable<Tag> AllTags { get; set; }
    }
}
