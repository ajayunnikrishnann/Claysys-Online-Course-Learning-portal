﻿    using System.Web.Mvc;
using Claysys_Online_Course_Learning_portal.Attributes;
using Claysys_Online_Course_Learning_portal.DataAccess;
    using Claysys_Online_Course_Learning_portal.Models;
    using BCrypt.Net;
    using System.Web.Security;
    using System.IO;
    using System;
    using System.Web;
using System.Configuration;


namespace Claysys_Online_Course_Learning_portal.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminDataAccess _adminDataAccess = new AdminDataAccess();
        private readonly UserDataAccess _userDataAccess = new UserDataAccess();
        private readonly CourseDataAccess _courseDataAccess = new CourseDataAccess();
        private readonly EnrollmentRequestDataAccess _enrollmentRequestDataAccess = new EnrollmentRequestDataAccess(ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString);

        [HttpGet]
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(Admin admin)
        {
            if (ModelState.IsValid)
            {
                admin.Password = HashPassword(admin.Password);
                admin.ConfirmPassword = admin.Password;

                _adminDataAccess.InsertAdmin(admin);
                return RedirectToAction("Login");
            }

            return View(admin);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Admin admin)
        {
            var existingAdmin = _adminDataAccess.GetAdminByEmail(admin.Email);

            if (existingAdmin != null && VerifyPassword(admin.Password, existingAdmin.Password))
            {
                FormsAuthentication.SetAuthCookie(existingAdmin.Email, false);

                var ticket = new FormsAuthenticationTicket(1, existingAdmin.Email, DateTime.Now, DateTime.Now.AddMinutes(30), false, "Admin");
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var authCookie = new HttpCookie(".AspNetAdminAuth", encryptedTicket);
                Response.Cookies.Add(authCookie);

                Session["AdminID"] = existingAdmin.AdminID;
                Session["AdminName"] = existingAdmin.Name;

                return RedirectToAction("UserManagement");
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(admin);
            }
        }

        public ActionResult Logout()
        {
            Session.Abandon();

            FormsAuthentication.SignOut();
            var authCookie = Request.Cookies[".AspNetAdminAuth"];
            if (authCookie != null)
            {
                authCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(authCookie);
            }

            return RedirectToAction("Login", "Admin");
        }



        [HttpGet]
        public ActionResult Dashboard()
        {
            if (Session["AdminName"] != null)
            {
                ViewBag.AdminName = Session["AdminName"].ToString();
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [AdminAuthorize]
        [HttpGet]
        public ActionResult UserManagement()
        {
            var users = _userDataAccess.GetAllUsers();
            return View(users);
        }

        [HttpPost]
        public ActionResult DeleteUser(int userId)
        {
            _userDataAccess.DeleteUser(userId);
            return RedirectToAction("UserManagement");
        }


        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }


        [AdminAuthorize]
        [HttpGet]
        public ActionResult CourseManagement()
        {
            var courseDataAccess = new CourseDataAccess();
            var courses = courseDataAccess.GetAllCourses();

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
                return RedirectToAction("CourseManagement");
            }

            return View(course);
        }



        // GET: Admin/EditCourse/{courseId}
        public ActionResult EditCourse(int courseId)
        {
            Course course = _courseDataAccess.GetCourseById(courseId);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: Admin/EditCourse
        // POST: Admin/EditCourse
        [HttpPost]
        public ActionResult EditCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                // Handle file uploads (if any)
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

                _courseDataAccess.UpdateCourse(course);
                return RedirectToAction("CourseManagement");
            }

            // Re-fetch the existing course details if ModelState is invalid
            var existingCourse = _courseDataAccess.GetCourseById(course.CourseId);
            if (existingCourse != null)
            {
                course.ReferenceLinks = existingCourse.ReferenceLinks;
            }


            return View(course);
        }


        



        // GET: Admin/DeleteCourse/{courseId}
        public ActionResult DeleteCourse(int courseId)
        {
            Course course = _courseDataAccess.GetCourseById(courseId);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: Admin/DeleteCourse/{courseId}
        [HttpPost, ActionName("DeleteCourse")]
        public ActionResult DeleteCourseConfirmed(int courseId)
        {
            _courseDataAccess.DeleteCourse(courseId);
            return RedirectToAction("CourseManagement"); // Redirect to course management page or another appropriate action
        }

        [HttpGet]
        public ActionResult ViewCourseReviews(int courseId)
        {
            var course = _courseDataAccess.GetCourseWithReviewsById(courseId);  // Fetch the course data along with reviews
            if (course == null)
            {
                return HttpNotFound();
            }

            return View(course);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPurchaseLimit(int CourseId, int PurchaseLimit)
        {
            try
            {
                var course = _courseDataAccess.GetCourseById(CourseId);

                if (course != null)
                {
                    course.PurchaseLimit = PurchaseLimit;
                    _courseDataAccess.UpdateCourse(course);
                    return RedirectToAction("CourseManagement", "Admin");
                }
                else
                {
                    // If course is not found, redirect with an error message
                    TempData["ErrorMessage"] = "Course not found";
                    return RedirectToAction("CourseManagement", "Admin");
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("CourseManagement", "Admin");
            }
        }


        [HttpGet]
        public ActionResult ManageEnrollmentRequests()
        {
            var requests = _enrollmentRequestDataAccess.GetAllEnrollmentRequests();
            return View(requests);
        }

        [HttpPost]
        public JsonResult ApproveRequest(int id)
        {
            try
            {
                _enrollmentRequestDataAccess.UpdateEnrollmentRequestStatus(id, true, false);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RejectRequest(int id)
        {
            try
            {
                _enrollmentRequestDataAccess.UpdateEnrollmentRequestStatus(id, false, true);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }

}
