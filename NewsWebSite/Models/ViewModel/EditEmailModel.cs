using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NewsWebSite.Models.ViewModel
{
    public class EditEmailModel
    {
        [Required(ErrorMessage = "Поле должно быть заполнено!")]
        [EmailAddress(ErrorMessage = "Введенное значение не является Email адрессом")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}