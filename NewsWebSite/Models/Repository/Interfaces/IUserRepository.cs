using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsWebSite.Models.Repository
{
    public interface IUserRepository
    {
        int Save(AppUser u);
        AppUser GetById(int id);
        AppUser FindByName(string name);
        string GetUserImage(int id);

    }
}