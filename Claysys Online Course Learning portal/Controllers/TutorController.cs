using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Claysys_Online_Course_Learning_portal.DataAccess;
using System.Web.Mvc;
using Claysys_Online_Course_Learning_portal.Models;
using System.IO;

namespace Claysys_Online_Course_Learning_portal.Controllers
{
    public class TutorController : Controller
    {

        private readonly CourseDataAccess _courseDataAccess;

        public TutorController()
        {
            _courseDataAccess = new CourseDataAccess();
        }

        public ActionResult TutorIndex()
        {
            var courses = _courseDataAccess.GetAllCourses();
            return View(courses);
        }

        [HttpGet]
        public ActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                if (course.SmallVideoFile != null && course.SmallVideoFile.ContentLength > 0)
                {
                    string videoDirectory = Server.MapPath("~/Content/Videos");
                    if (!Directory.Exists(videoDirectory))
                    {
                        Directory.CreateDirectory(videoDirectory);
                    }

                    string videoPath = Path.Combine(videoDirectory, Path.GetFileName(course.SmallVideoFile.FileName));
                    course.SmallVideoFile.SaveAs(videoPath);
                    course.SmallVideoPath = "/Content/Videos/" + Path.GetFileName(course.SmallVideoFile.FileName);
                }

                if (course.ImageFile != null && course.ImageFile.ContentLength > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        course.ImageFile.InputStream.CopyTo(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        course.ImageBase64 = Convert.ToBase64String(imageBytes);
                    }
                }

                _courseDataAccess.InsertCourse(course);
                return RedirectToAction("TutorIndex");
            }

            return View(course);
        }

    }
}