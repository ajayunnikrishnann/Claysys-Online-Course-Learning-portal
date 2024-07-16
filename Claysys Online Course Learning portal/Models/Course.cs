using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Claysys_Online_Course_Learning_portal.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; } // Added Description

        public string SmallVideoPath { get; set; } // Path to small video file

        public string ImageBase64 { get; set; } // Base64 encoded image for thumbnail

        public string ReferenceLinks { get; set; } // Links to reference other websites

        public int UserPurchasedCount { get; set; } // Number of users who purchased the course

        public virtual ICollection<Review> Reviews { get; set; } // Course reviews

        // Constructor to initialize Reviews collection
        public Course()
        {
            Reviews = new HashSet<Review>();
        }

        [NotMapped]
        public double AverageReviewScore
        {
            get
            {
                if (Reviews != null && Reviews.Count > 0)
                {
                    return Reviews.Average(r => r.ReviewScore);
                }
                return 0;
            }
        }

        // NotMapped properties for file uploads
        [NotMapped]
        public HttpPostedFileBase SmallVideoFile { get; set; } // Uploaded video file

        [NotMapped]
        public HttpPostedFileBase ImageFile { get; set; } // Uploaded image file
    }
}
