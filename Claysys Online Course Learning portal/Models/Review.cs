using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Claysys_Online_Course_Learning_portal.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public int CourseId { get; set; } // Foreign key

        public virtual Course Course { get; set; } // Navigation property
    }
}