using System.Collections.Generic;
using System.Web.Mvc;
using Claysys_Online_Course_Learning_portal.DataAccess;
using Claysys_Online_Course_Learning_portal.Models;
using System.Diagnostics;
using BCrypt.Net;
using System.Web;
using System;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Net;


namespace Claysys_Online_Course_Learning_portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDataAccess _userDataAccess = new UserDataAccess();
        private CourseDataAccess courseDataAccess = new CourseDataAccess();

       


        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Signup()
        {
            PopulateStateAndCityLists(); // Sync method for initial load
            return View();
        }

        [HttpPost]
        public ActionResult Signup(User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = HashPassword(user.Password);
                user.ConfirmPassword = user.Password;

                _userDataAccess.InsertUser(user);
                return RedirectToAction("Login");
            }

            PopulateStateAndCityLists(); // Sync method for redisplaying form
            return View(user);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
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

            FormsAuthentication.SetAuthCookie(user.Username, false);

            var ticket = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddMinutes(30), false, "User");
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var authCookie = new HttpCookie(".AspNetUserAuth", encryptedTicket);
            Response.Cookies.Add(authCookie);

            Session["UserID"] = user.UserID;
            Session["Username"] = user.Username;

            return RedirectToAction("Index", "Account");
        }



        [HttpGet]
        public ActionResult Index()
        {

            // Check if user is logged in
            if (Session["Username"] != null)
            {
                ViewBag.Username = Session["Username"].ToString();
                ViewBag.IsLoggedIn = true;

                var userId = (int)Session["UserID"];
                ViewBag.CurrentUserId = userId;
                ViewBag.TutorID = Session["TutorID"];


            }
            else
            {
                ViewBag.IsLoggedIn = false;
                
            }

            var courses = courseDataAccess.GetAllCourses();

            foreach (var course in courses)
            {
                course.Reviews = courseDataAccess.GetReviewsByCourseId(course.CourseId);
                Debug.WriteLine($"Course: {course.Title}, AverageReviewScore: {course.AverageReviewScore}, ReviewCount: {course.Reviews.Count}");
            }

            return View(courses); // Ensure there is a corresponding Index.cshtml view in Views/Account
        }








        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Logout()
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


        private bool IsUsernameTaken(string username)
        {
            // Implement your logic to check if the username exists
            return false;
        }

        private bool IsPasswordValid(string password)
    {
        // Implement your logic to check the password format
        var regex = new Regex("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$");
        return regex.IsMatch(password);
    }

    [HttpPost]
    public JsonResult ValidatePassword(string password)
    {
        bool isValid = IsPasswordValid(password);
        return Json(new { valid = isValid });
    }


        [HttpPost]
        public JsonResult CheckEmail(string value)
        {
            var dataAccess = new UserDataAccess();
            bool isAvailable = dataAccess.IsEmailAvailable(value);
            return Json(new { available = isAvailable });

        }

        [HttpPost]
        public JsonResult CheckUsername(string value)
        {
            var dataAccess = new UserDataAccess();
            bool isAvailable = dataAccess.IsUsernameAvailable(value);
            return Json(new { available = isAvailable });
        }



        [HttpPost]
        public JsonResult GetCities(string state)
        {
            var cities = GetCitiesByState(state); // Sync method
            return Json(cities);
        }

        private void PopulateStateAndCityLists()
        {
            ViewBag.StateList = GetStates(); // Sync method
            ViewBag.CityList = new List<SelectListItem>();
        }

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

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }




        [HttpPost]
        public ActionResult DeleteReview(int reviewId, int courseId)
        {
            var userId = User.Identity.Name; // Assuming User.Identity.Name uniquely identifies the user
            if (courseDataAccess.IsReviewOwner(reviewId, userId))
            {
                courseDataAccess.DeleteReview(reviewId, userId);
            }
            return RedirectToAction("ViewDetail", new { id = courseId });
        }

        [HttpPost]
        public ActionResult EditReview(AddReviewViewModel model)
        {
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

                if (courseDataAccess.IsReviewOwner(model.ReviewId, User.Identity.Name))
                {
                    courseDataAccess.UpdateReview(review);
                }
                return RedirectToAction("ViewDetail", new { id = model.CourseId });
            }
            return View(model);
        }

        public ActionResult ViewDetail(int id)
        {
            var course = courseDataAccess.GetCourseById(id);
            if (course != null)
            {
                course.Reviews = courseDataAccess.GetReviewsByCourseId(id);
            }
            return View(course);
        }


        [HttpPost]
        public ActionResult AddReview(AddReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = courseDataAccess.GetCourseById(model.CourseId);
                if (course == null)
                {
                    return HttpNotFound();
                }

                var review = new Review
                {
                    CourseId = model.CourseId,
                    ReviewScore = model.ReviewScore,
                    Comment = model.Comment,
                    UserId = User.Identity.Name // Assuming User.Identity.Name uniquely identifies the user
                };

                courseDataAccess.AddReview(review);

                return RedirectToAction("ViewDetail", new { id = model.CourseId });
            }

            // Handle the case where the model state is invalid
            return View(model);
        }


        [HttpPost]
        public JsonResult Enroll(int courseId)
        {

            if (Session["UserID"] == null)
            {
                return Json(new { success = false, redirectUrl = Url.Action("Login", "Account"), message = "You need to log in first." });
            }

            try
            {
                var userId = (int)Session["UserID"];
                if (!courseDataAccess.IsUserEnrolledInCourse(userId, courseId))
                {
                    courseDataAccess.AddEnrollment(userId, courseId);

                    var course = courseDataAccess.GetCourseById(courseId);
                    if (course != null)
                    {
                        // Increment the UserPurchasedCount
                        course.UserPurchasedCount += 1;
                        courseDataAccess.UpdateCourse(course);

                        // Return success response
                        return Json(new { success = true });
                    }
                }
                else
                {
                    // Return error response if user is already enrolled
                    return Json(new { success = false, message = "You are already enrolled in this course." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception and return error response
                Debug.WriteLine($"An error occurred: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                return Json(new { success = false, message = "An error occurred during enrollment." });
            }

            // Return error response if course is not found or other conditions
            return Json(new { success = false, message = "An error occurred during enrollment." });
        }



       




    }
}


