using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Claysys_Online_Course_Learning_portal.Models
{
    public class AddReviewViewModel
    {
        public int ReviewId { get; set; }
        public int CourseId { get; set; }
        public int ReviewScore { get; set; }
        public string Comment { get; set; }
    }

}