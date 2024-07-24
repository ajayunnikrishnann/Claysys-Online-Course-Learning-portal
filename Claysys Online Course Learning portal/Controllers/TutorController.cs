using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BCrypt.Net;
using System.Diagnostics;
using Claysys_Online_Course_Learning_portal.DataAccess;
using System.Web.Mvc;
using Claysys_Online_Course_Learning_portal.Models;
using System.IO;
using System.Web.Security;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Reflection;

namespace Claysys_Online_Course_Learning_portal.Controllers
{
    public class TutorController : Controller
    {

        private readonly CourseDataAccess _courseDataAccess;
        private readonly UserDataAccess _userDataAccess = new UserDataAccess();
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString;
        public TutorController()
        {
            _courseDataAccess = new CourseDataAccess();
        }

        public ActionResult TutorIndex()
        {
            // Check if user is logged in
            if (Session["Username"] != null)
            {
                ViewBag.Username = Session["Username"].ToString();
                ViewBag.IsLoggedIn = true;

                var userId = (int)Session["UserID"];

            }
            else
            {
                ViewBag.IsLoggedIn = false;

            }

            var courses = _courseDataAccess.GetAllCourses();

            foreach (var course in courses)
            {
                course.Reviews = _courseDataAccess.GetReviewsByCourseId(course.CourseId);
                Debug.WriteLine($"Course: {course.Title}, AverageReviewScore: {course.AverageReviewScore}, ReviewCount: {course.Reviews.Count}");
            }

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

        [HttpGet]
        public ActionResult SignupTutor()
        {
            PopulateStateAndCityLists(); // Sync method for initial load
            return View();
        }

        [HttpPost]
        public ActionResult SignupTutor(Tutor tutor)
        {
            if (ModelState.IsValid)
            {
                if (tutor.Password != tutor.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View(tutor);
                }

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_TutorSignup", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FirstName", tutor.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", tutor.LastName);
                        cmd.Parameters.AddWithValue("@DateOfBirth", tutor.DateOfBirth);
                        cmd.Parameters.AddWithValue("@Gender", tutor.Gender);
                        cmd.Parameters.AddWithValue("@Phone", tutor.Phone);
                        cmd.Parameters.AddWithValue("@Email", tutor.Email);
                        cmd.Parameters.AddWithValue("@Address", tutor.Address);
                        cmd.Parameters.AddWithValue("@State", tutor.State);
                        cmd.Parameters.AddWithValue("@City", tutor.City);
                        cmd.Parameters.AddWithValue("@Username", tutor.Username);
                        cmd.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(tutor.Password));
                        cmd.Parameters.AddWithValue("@Role", "Tutor"); // Set role to 'Tutor'

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                return RedirectToAction("LoginTutor");
            }

            PopulateStateAndCityLists(); // Sync method for redisplaying form
            return View(tutor);
        }


        [HttpGet]
        public ActionResult LoginTutor()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginTutor(string username, string password)
        {
            var tutor = _userDataAccess.GetTutorByUsername(username);
           

            if (tutor != null && VerifyPassword(password, tutor.Password))
            {
                if (tutor.Role != "Tutor")
                {
                    ModelState.AddModelError("", "You do not have permission to access this page.");
                    return View();
                }

                FormsAuthentication.SetAuthCookie(tutor.Username, false);

                var ticket = new FormsAuthenticationTicket(1, tutor.Username, DateTime.Now, DateTime.Now.AddMinutes(30), false, "Tutor");
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var authCookie = new HttpCookie(".AspNetTutorAuth", encryptedTicket);
                Response.Cookies.Add(authCookie);

                Session["UserID"] = tutor.TutorID;
                Session["Username"] = tutor.Username;

                return RedirectToAction("TutorIndex", "Tutor");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
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

    }
}