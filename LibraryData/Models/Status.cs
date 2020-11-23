using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryData.Models
{
    public class Status
    {
        public int Id { get; set; }
        [Required]
        public String Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
