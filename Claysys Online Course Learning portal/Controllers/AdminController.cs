    using System.Web.Mvc;
    using Claysys_Online_Course_Learning_portal.DataAccess;
    using Claysys_Online_Course_Learning_portal.Models;
    using BCrypt.Net;
    using System.Web.Security;
    using System.IO;
    using System;
    using System.Web;


    namespace Claysys_Online_Course_Learning_portal.Controllers
    {
        public class AdminController : Controller
        {
            private readonly AdminDataAccess _adminDataAccess = new AdminDataAccess();
            private readonly UserDataAccess _userDataAccess = new UserDataAccess();
            private readonly CourseDataAccess _courseDataAccess = new CourseDataAccess();

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
                    // Set the authentication cookie
                    FormsAuthentication.SetAuthCookie(existingAdmin.Email, false);

                    // Create session
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



            [HttpGet]
            public ActionResult CourseManagement()
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
            [HttpPost]
            public ActionResult EditCourse(Course course)
            {
                if (ModelState.IsValid)
                {
                    _courseDataAccess.UpdateCourse(course);
                    return RedirectToAction("CourseManagement"); // Redirect to course management page or another appropriate action
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

        }
    }
