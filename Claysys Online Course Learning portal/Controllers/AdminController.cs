    using System.Web.Mvc;
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
        // Data access objects for various data operations
        private readonly AdminDataAccess _adminDataAccess = new AdminDataAccess();
        private readonly UserDataAccess _userDataAccess = new UserDataAccess();
        private readonly CourseDataAccess _courseDataAccess = new CourseDataAccess();
        private readonly EnrollmentRequestDataAccess _enrollmentRequestDataAccess = new EnrollmentRequestDataAccess(ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString);

        // GET: Admin/Signup
        [HttpGet]
        public ActionResult Signup()
        {
            return View();
        }

        // POST: Admin/Signup
        [HttpPost]
        public ActionResult Signup(Admin admin)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Hash the password before saving
                    admin.Password = HashPassword(admin.Password);
                    admin.ConfirmPassword = admin.Password;

                    _adminDataAccess.InsertAdmin(admin); // Insert admin data
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
                ModelState.AddModelError("", "An error occurred while signing up. Please try again later.");
            }

            return View(admin);
                  
        }

        // GET: Admin/Login
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

                    // Create a forms authentication ticket
                    var ticket = new FormsAuthenticationTicket(1, existingAdmin.Email, DateTime.Now, DateTime.Now.AddMinutes(30), false, "Admin");
                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var authCookie = new HttpCookie(".AspNetAdminAuth", encryptedTicket);
                    Response.Cookies.Add(authCookie);

                    // Set session variables
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

        // Admin logout
        public ActionResult Logout()
        {
            try
            {
                Session.Abandon(); // Clear the session

                FormsAuthentication.SignOut();
                var authCookie = Request.Cookies[".AspNetAdminAuth"];
                if (authCookie != null)
                {
                    authCookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(authCookie); // Remove authentication cookie
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
               
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

        // GET: Admin/UserManagement
        [AdminAuthorize]
        [HttpGet]
        public ActionResult UserManagement()
        {
            try
            {
                var users = _userDataAccess.GetAllUsers();
                return View(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
                return View("Error"); 
            }
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        public ActionResult DeleteUser(int userId)
        {
            try
            {
                _userDataAccess.DeleteUser(userId);
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
                return RedirectToAction("UserManagement");
            }
        }

        // Hash the password using BCrypt
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verify the password using BCrypt
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        // GET: Admin/CourseManagement
        [AdminAuthorize]
        [HttpGet]
        public ActionResult CourseManagement()
        {
            
                var courseDataAccess = new CourseDataAccess();
            var courses = courseDataAccess.GetAllCourses(); // Get all courses

            return View(courses);
        }

        // GET: Admin/CreateCourse
        [HttpGet]
        public ActionResult CreateCourse()
        {

            return View();
        }

        // POST: Admin/CreateCourse
        [HttpPost]
        public ActionResult CreateCourse(Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Save the small video file
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

                    // Save the image file as a base64 string
                    if (course.ImageFile != null && course.ImageFile.ContentLength > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            course.ImageFile.InputStream.CopyTo(memoryStream);
                            byte[] imageBytes = memoryStream.ToArray();
                            course.ImageBase64 = Convert.ToBase64String(imageBytes);
                        }
                    }
                    // Insert course data
                    _courseDataAccess.InsertCourse(course);
                    return RedirectToAction("CourseManagement");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ModelState.AddModelError("", "An error occurred while creating the course. Please try again later.");
            }

            return View(course);
        }



        // GET: Admin/EditCourse/{courseId}
        public ActionResult EditCourse(int courseId)
        {
            try
            {
                Course course = _courseDataAccess.GetCourseById(courseId);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error"); // or redirect to an error page
            }
        }

        
        // POST: Admin/EditCourse
        [HttpPost]
        public ActionResult EditCourse(Course course)
        {
            try
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

                    // Handle image file upload
                    if (course.ImageFile != null && course.ImageFile.ContentLength > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        course.ImageFile.InputStream.CopyTo(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        course.ImageBase64 = Convert.ToBase64String(imageBytes);
                    }
                }

                    _courseDataAccess.UpdateCourse(course); // Update course data
                    return RedirectToAction("CourseManagement");
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ModelState.AddModelError("", "An error occurred while updating the course. Please try again later.");
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
            try
            {
                Course course = _courseDataAccess.GetCourseById(courseId);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error"); // or redirect to an error page
            }
        }

        // POST: Admin/DeleteCourse/{courseId}
        [HttpPost, ActionName("DeleteCourse")]
        public ActionResult DeleteCourseConfirmed(int courseId)
        {
            try
            {
                _courseDataAccess.DeleteCourse(courseId);
            return RedirectToAction("CourseManagement"); // Redirect to course management page or another appropriate action
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("CourseManagement"); // or redirect to an error page
            }
        }

        // GET: Admin/ViewCourseReviews/{courseId}
        [HttpGet]
        public ActionResult ViewCourseReviews(int courseId)
        {
            try
            {
                // Fetch the course data along with its reviews
                var course = _courseDataAccess.GetCourseWithReviewsById(courseId);  
            if (course == null)
            {
                return HttpNotFound(); // Return 404 if course is not found
                }

            return View(course); // Return the course data to the view
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error"); // or redirect to an error page
            }
        }


        // POST: Admin/SetPurchaseLimit
        [HttpPost]
        [ValidateAntiForgeryToken]  // Protect against CSRF attacks
        public ActionResult SetPurchaseLimit(int CourseId, int PurchaseLimit)
        {
            try
            {
                // Fetch the course by its ID
                var course = _courseDataAccess.GetCourseById(CourseId);

                if (course != null)
                {
                    // Update the purchase limit
                    course.PurchaseLimit = PurchaseLimit;
                    _courseDataAccess.UpdateCourse(course); // Save the changes
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

        // GET: Admin/ManageEnrollmentRequests
        [AdminAuthorize] // Custom attribute for admin authorization
        [HttpGet]
        public ActionResult ManageEnrollmentRequests()
        {
            try
            {
                // Fetch all enrollment requests
                var requests = _enrollmentRequestDataAccess.GetAllEnrollmentRequests();
                return View(requests); // Return the requests to the view
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
                return View("Error"); // or redirect to an error page
            }
        }

        // POST: Admin/ApproveRequest
        [HttpPost]
        public JsonResult ApproveRequest(int id)
        {
            try
            {
                // Approve the enrollment request
                _enrollmentRequestDataAccess.UpdateEnrollmentRequestStatus(id, true, false);
                return Json(new { success = true }); // Return success response
            }
            catch (Exception ex)
            {
                // Return error response
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/RejectRequest
        [HttpPost]
        public JsonResult RejectRequest(int id)
        {
            try
            {
                // Reject the enrollment request
                _enrollmentRequestDataAccess.UpdateEnrollmentRequestStatus(id, false, true);
                return Json(new { success = true }); // Return success response
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }

}
