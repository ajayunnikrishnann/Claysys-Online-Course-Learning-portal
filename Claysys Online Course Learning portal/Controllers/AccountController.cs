using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Claysys_Online_Course_Learning_portal.DataAccess;
using Claysys_Online_Course_Learning_portal.Models;
using System.Diagnostics;
using BCrypt.Net;
using System.Web;
using System;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Net;
using System.Web.Configuration;
using System.Configuration;


namespace Claysys_Online_Course_Learning_portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDataAccess _userDataAccess = new UserDataAccess();
        private CourseDataAccess courseDataAccess = new CourseDataAccess();
        private readonly EnrollmentRequestDataAccess _enrollmentRequestDataAccess = new EnrollmentRequestDataAccess(ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString);


        // Action to render the About page
        public ActionResult About()
        {
            return View();
        }

        // Action to render the Contact page
        public ActionResult Contact()
        {
            return View();
        }

        // GET: Action to render the Signup page
        [HttpGet]
        public ActionResult Signup()
        {
            try
            {
                PopulateStateAndCityLists(); // Load states and cities for dropdowns
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }

        // POST: Action to handle the Signup form submission
        [HttpPost]
        public ActionResult Signup(User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    user.Password = HashPassword(user.Password); // Hash the password
                    user.ConfirmPassword = user.Password;

                    _userDataAccess.InsertUser(user); // Insert user into the database
                    return RedirectToAction("Login");
                }

                PopulateStateAndCityLists(); // Reload states and cities if model state is invalid
                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }

        // GET: Action to render the Login page
        [HttpGet]
        public ActionResult Login()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }

        // POST: Action to handle the Login form submission
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            try
            {
                var user = _userDataAccess.GetUserByUsername(username);


                if (user == null)
                {
                    ModelState.AddModelError("UsernameNotFound", "Username not found.");
                    return View();
                }

                if (!VerifyPassword(password, user.Password))
                {
                    ModelState.AddModelError("PasswordIncorrect", "Incorrect password.");
                    return View();
                }

                FormsAuthentication.SetAuthCookie(user.Username, false); // Set auth cookie

                var ticket = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddMinutes(30), false, "User");
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var authCookie = new HttpCookie(".AspNetUserAuth", encryptedTicket);
                Response.Cookies.Add(authCookie);

                Session["UserID"] = user.UserID;
                Session["Username"] = user.Username; // Store username in session
                Session["Email"] = user.Email;  // Store email in session
                Session["PhoneNumber"] = user.Phone;  // Store phone number in session

                return RedirectToAction("Index", "Account");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }


        // GET: Action to render the Index page (user dashboard)
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                var courses = courseDataAccess.GetAllCourses();



                // Check if user is logged in
                if (Session["Username"] != null)
                {
                    ViewBag.Username = Session["Username"].ToString();
                    ViewBag.IsLoggedIn = true;

                    var userId = (int)Session["UserID"];
                    ViewBag.CurrentUserId = userId;
                    ViewBag.TutorID = Session["TutorID"];

                    ViewBag.EnrolledCourses = _enrollmentRequestDataAccess.GetEnrollmentRequestsByUserId(Convert.ToInt32(Session["UserID"])).Select(r => r.CourseId).ToList();
                    ViewBag.ApprovedCourses = _enrollmentRequestDataAccess.GetApprovedEnrollmentRequestsByUserId(Convert.ToInt32(Session["UserID"])).Select(r => r.CourseId).ToList();

                }
                else
                {
                    ViewBag.IsLoggedIn = false;
                    ViewBag.EnrolledCourses = new List<int>(); // Empty list when the user is not logged in
                    ViewBag.ApprovedCourses = new List<int>(); // Empty list when the user is not logged in
                }

                // Get reviews for each course
                foreach (var course in courses)
                {
                    course.Reviews = courseDataAccess.GetReviewsByCourseId(course.CourseId);
                }

                return View(courses); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error");
            }

        }



        // Action to handle user logout
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Logout()
        {
            try
            {
                Session.Abandon();

                FormsAuthentication.SignOut();
                var authCookie = Request.Cookies[".AspNetUserAuth"];
                if (authCookie != null)
                {
                    authCookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(authCookie);
                }

                return RedirectToAction("Index", "Account");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Error");
            }

        }

        // Method to check if a username is taken
        private bool IsUsernameTaken(string username)
        {
            // Implement your logic to check if the username exists
            return false;
        }

        // Method to validate password format

        private bool IsPasswordValid(string password)
        {
            
            var regex = new Regex("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$");
            return regex.IsMatch(password);
        }

        // POST: Action to validate password via AJAX
        [HttpPost]
        public JsonResult ValidatePassword(string password)
        {
            try
            {
                bool isValid = IsPasswordValid(password);
                return Json(new { valid = isValid });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { valid = false, error = ex.Message });
            }
        }

        // POST: Action to check if an email is available via AJAX
        [HttpPost]
        public JsonResult CheckEmail(string value)
        {
            
                var dataAccess = new UserDataAccess();
                bool isAvailable = dataAccess.IsEmailAvailable(value);
                return Json(new { available = isAvailable });

            }


        // POST: Action to check if a username is available via AJAX
        [HttpPost]
        public JsonResult CheckUsername(string value)
        {
          
                var dataAccess = new UserDataAccess();
                bool isAvailable = dataAccess.IsUsernameAvailable(value);
                return Json(new { available = isAvailable });
            }



        // POST: Action to get cities based on selected state via AJAX
        [HttpPost]
        public JsonResult GetCities(string state)
        {
            try
            {
                var cities = GetCitiesByState(state); // Load cities for selected state
                return Json(cities);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { error = ex.Message });
            }
        }

        // Method to populate state and city dropdown lists
        private void PopulateStateAndCityLists()
        {
            try
            {
                ViewBag.StateList = GetStates(); // Load states
                ViewBag.CityList = new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Method to get a list of states (static data)
        private List<SelectListItem> GetStates()
        {
            // Static list of states in India
            var states = new List<SelectListItem>
            {
                new SelectListItem { Value = "Andhra Pradesh", Text = "Andhra Pradesh" },
                new SelectListItem { Value = "Arunachal Pradesh", Text = "Arunachal Pradesh" },
                new SelectListItem { Value = "Assam", Text = "Assam" },
                new SelectListItem { Value = "Bihar", Text = "Bihar" },
                new SelectListItem { Value = "Chhattisgarh", Text = "Chhattisgarh" },
                new SelectListItem { Value = "Goa", Text = "Goa" },
                new SelectListItem { Value = "Gujarat", Text = "Gujarat" },
                new SelectListItem { Value = "Haryana", Text = "Haryana" },
                new SelectListItem { Value = "Himachal Pradesh", Text = "Himachal Pradesh" },
                new SelectListItem { Value = "Jharkhand", Text = "Jharkhand" },
                new SelectListItem { Value = "Karnataka", Text = "Karnataka" },
                new SelectListItem { Value = "Kerala", Text = "Kerala" },
                new SelectListItem { Value = "Madhya Pradesh", Text = "Madhya Pradesh" },
                new SelectListItem { Value = "Maharashtra", Text = "Maharashtra" },
                new SelectListItem { Value = "Manipur", Text = "Manipur" },
                new SelectListItem { Value = "Meghalaya", Text = "Meghalaya" },
                new SelectListItem { Value = "Mizoram", Text = "Mizoram" },
                new SelectListItem { Value = "Nagaland", Text = "Nagaland" },
                new SelectListItem { Value = "Odisha", Text = "Odisha" },
                new SelectListItem { Value = "Punjab", Text = "Punjab" },
                new SelectListItem { Value = "Rajasthan", Text = "Rajasthan" },
                new SelectListItem { Value = "Sikkim", Text = "Sikkim" },
                new SelectListItem { Value = "Tamil Nadu", Text = "Tamil Nadu" },
                new SelectListItem { Value = "Telangana", Text = "Telangana" },
                new SelectListItem { Value = "Tripura", Text = "Tripura" },
                new SelectListItem { Value = "Uttar Pradesh", Text = "Uttar Pradesh" },
                new SelectListItem { Value = "Uttarakhand", Text = "Uttarakhand" },
                new SelectListItem { Value = "West Bengal", Text = "West Bengal" }
            };
            return states;
        }

        // Method to get a list of cities based on selected state (static data)
        private List<SelectListItem> GetCitiesByState(string state)
        {
            // Static list of cities for selected state
            var cities = new List<SelectListItem>();

            switch (state)
            {
                case "Andhra Pradesh":
                    cities.Add(new SelectListItem { Value = "Visakhapatnam", Text = "Visakhapatnam" });
                    cities.Add(new SelectListItem { Value = "Vijayawada", Text = "Vijayawada" });
                    cities.Add(new SelectListItem { Value = "Guntur", Text = "Guntur" });
                    cities.Add(new SelectListItem { Value = "Nellore", Text = "Nellore" });
                    cities.Add(new SelectListItem { Value = "Kurnool", Text = "Kurnool" });
                    cities.Add(new SelectListItem { Value = "Rajahmundry", Text = "Rajahmundry" });
                    cities.Add(new SelectListItem { Value = "Tirupati", Text = "Tirupati" });
                    cities.Add(new SelectListItem { Value = "Kadapa", Text = "Kadapa" });
                    cities.Add(new SelectListItem { Value = "Anantapur", Text = "Anantapur" });
                    cities.Add(new SelectListItem { Value = "Eluru", Text = "Eluru" });
                    break;
                default:
                    break;
            }

            return cities;
        }

        // Method to hash passwords using BCrypt
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Method to verify password against the hash
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }



        // POST: Action to handle review deletion
        [HttpPost]
        public ActionResult DeleteReview(int reviewId, int courseId)
        {
            try
            {
                // Get the currently logged-in user's ID
                var userId = User.Identity.Name; // Assuming User.Identity.Name uniquely identifies the user

                // Check if the current user is the owner of the review
                if (courseDataAccess.IsReviewOwner(reviewId, userId))
                {
                    // Delete the review from the database
                    courseDataAccess.DeleteReview(reviewId, userId);
                }
                // Redirect to the course detail view after deletion
                return RedirectToAction("ViewDetail", new { id = courseId });
            }
            catch (Exception ex)
            {
                // Log the error message
                Console.WriteLine(ex.Message);
                // Redirect to the course detail view with an error message
                return RedirectToAction("ViewDetail", new { id = courseId, error = "An error occurred while deleting the review." });
            }
        }

        // POST: Action to handle review editing
        [HttpPost]
        public ActionResult EditReview(AddReviewViewModel model)
        {
            try
            {
                // Check if the model state is valid
                if (ModelState.IsValid)
                {
                    var review = new Review
                    {
                        ReviewId = model.ReviewId,
                        CourseId = model.CourseId,
                        ReviewScore = model.ReviewScore,
                        Comment = model.Comment,
                        UserId = User.Identity.Name // Assuming User.Identity.Name uniquely identifies the user
                    };

                    // Check if the current user is the owner of the review
                    if (courseDataAccess.IsReviewOwner(model.ReviewId, User.Identity.Name))
                    {
                        // Update the review in the database
                        courseDataAccess.UpdateReview(review);
                    }
                    // Redirect to the course detail view after editing
                    return RedirectToAction("ViewDetail", new { id = model.CourseId });
                }
                // Return the view with the model if the model state is not valid
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("ViewDetail", new { id = model.CourseId, error = "An error occurred while editing the review." });
            }
        }

        // GET: Action to view course details
        public ActionResult ViewDetail(int id)
        {
            try
            {
                // Get the course details by ID
                var course = courseDataAccess.GetCourseById(id);
                if (course != null)
                {
                    // Get the reviews for the course
                    course.Reviews = courseDataAccess.GetReviewsByCourseId(id);
                }
                return View(course);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index", "Home", new { error = "An error occurred while retrieving course details." });
            }
        }


        // POST: Action to handle review addition
        [HttpPost]
        public ActionResult AddReview(AddReviewViewModel model)
        {
            try
            {
                // Check if the model state is valid
                if (ModelState.IsValid)
                {
                    // Get the course details by ID
                    var course = courseDataAccess.GetCourseById(model.CourseId);
                    if (course == null)
                    {
                        return HttpNotFound(); // Return 404 if course not found
                    }

                    var review = new Review
                    {
                        CourseId = model.CourseId,
                        ReviewScore = model.ReviewScore,
                        Comment = model.Comment,
                        UserId = User.Identity.Name // Assuming User.Identity.Name uniquely identifies the user
                    };

                    // Add the review to the database
                    courseDataAccess.AddReview(review);

                    return RedirectToAction("ViewDetail", new { id = model.CourseId });
                }

                // Handle the case where the model state is invalid
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("ViewDetail", new { id = model.CourseId, error = "An error occurred while adding the review." });
            }
        }

        // POST: Action to handle enrollment request
        [HttpPost]
        public JsonResult RequestEnrollment(int courseId)
        {
            try
            {
                // Check if the user is logged in
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "User is not logged in." });
                }

                int userId = (int)Session["UserID"];
                var username = Session["Username"].ToString();
                var email = Session["Email"].ToString();
                var phoneNumber = Session["PhoneNumber"].ToString();

                // Get the course title using the courseId
                var course = courseDataAccess.GetCourseById(courseId);
                if (course == null)
                {
                    return Json(new { success = false, message = "Course not found." });
                }

                var enrollmentRequest = new EnrollmentRequest
                {
                    UserId = userId,
                    CourseId = courseId,
                    CourseTitle = course.Title,
                    Username = username,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    RequestDate = DateTime.Now,
                    IsApproved = false,
                    IsRejected = false
                };

                // Insert the enrollment request into the database
                _enrollmentRequestDataAccess.InsertEnrollmentRequest(enrollmentRequest);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "An error occurred while requesting enrollment." });
            }
        }

        // POST: Action to get the enrollment status
        [HttpPost]
        public JsonResult GetEnrollmentStatus(int courseId)
        {
            try
            {
                // Check if the user is logged in
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "User is not logged in." });
                }

                int userId = (int)Session["UserID"];
                // Get the enrollment requests for the user
                var enrollmentRequests = _enrollmentRequestDataAccess.GetEnrollmentRequestsByUserId(userId);
                var request = enrollmentRequests.FirstOrDefault(r => r.CourseId == courseId);

                if (request == null)
                {
                    return Json(new { success = true, status = "Not Enrolled" });
                }

                if (request.IsApproved)
                {
                    return Json(new { success = true, status = "Approved" });
                }

                if (request.IsRejected)
                {
                    return Json(new { success = true, status = "Rejected" });
                }

                return Json(new { success = true, status = "Enrollment Requested" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "An error occurred while getting enrollment status." });
            }
        }

        // GET: Action to check if the user is logged in
        public ActionResult CheckSession()
        {
            if (Session["Username"] != null)
            {
                return Json(new { isLoggedIn = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isLoggedIn = false }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}



