using System.Collections.Generic;
using System.Web.Mvc;
using Claysys_Online_Course_Learning_portal.DataAccess;
using Claysys_Online_Course_Learning_portal.Models;
using System.Diagnostics;
using BCrypt.Net;
using System.Web;
using System;
using System.Web.Security;

namespace Claysys_Online_Course_Learning_portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDataAccess _userDataAccess = new UserDataAccess();

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

            if (user != null && VerifyPassword(password, user.Password))
            {
                // Set the authentication cookie
                FormsAuthentication.SetAuthCookie(user.Username, false);

                // Create session
                Session["UserID"] = user.UserID;
                Session["Username"] = user.Username;

                // Create a persistent login cookie if "Remember Me" is checked
                if (Request.Form["rememberMe"] != null && Request.Form["rememberMe"].Equals("on"))
                {
                    var authTicket = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddMonths(1), true, "");
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    authCookie.Expires = authTicket.Expiration;
                    Response.Cookies.Add(authCookie);
                }

                return RedirectToAction("Index", "Account");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
        }


        [HttpGet]
        public ActionResult Index()
        {
            if (Session["Username"] != null)
            {
                ViewBag.Username = Session["Username"].ToString();
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }






        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Logout()
        {
            // Clear session
            Session.Abandon();

            // Clear authentication cookie
            FormsAuthentication.SignOut();

            // Redirect to login page
            return RedirectToAction("Login", "Account");
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
    }
}
