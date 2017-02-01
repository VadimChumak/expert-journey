﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsWebSite.Models.Repository
{
    public interface ITagRepository
    {
        int Save(Tag tag);
        Tag GetTagById(int id);
        Tag GetTagByName(string name);
        IEnumerable<Tag> GetAllTags();
        IEnumerable<Tag> SaveTagsGroup(string[] tags);
        IEnumerable<Tag> GatTagsGroupByNames(string[] tags);
    }
}
