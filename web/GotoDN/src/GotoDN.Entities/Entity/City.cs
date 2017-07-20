﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GotoDN.Entities
{
    public class City
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public List<District> Districts { get; set; }
    }
}
