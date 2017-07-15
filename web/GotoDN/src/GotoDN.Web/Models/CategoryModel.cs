﻿using GotoDN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GotoDN.Web.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? Priority { get; set; }

        public List<CategoryLanguageModel> CategoryLanguages { get; set; }
    }

    public class CategoryLanguageModel
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public int? ImageId { get; set; }

        public LanguageEnums? Language { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public CategoryModel Category { get; set; }
        public ImageModel Image { get; set; }
    }
}
