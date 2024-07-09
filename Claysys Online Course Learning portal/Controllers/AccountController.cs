using Claysys_Online_Course_Learning_portal.DataAccess;
using Claysys_Online_Course_Learning_portal.Models;
using System.Web.Mvc;

namespace Claysys_Online_Course_Learning_portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDataAccess _userDataAccess = new UserDataAccess();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(User user)
        {
            if (ModelState.IsValid)
            {
                // Hash the password
                user.PasswordHash = HashPassword(user.PasswordHash);
                user.ConfirmPasswordHash = user.PasswordHash;

                // Save user to database
                _userDataAccess.InsertUser(user);

                return RedirectToAction("Login");
            }
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
            var hashedPassword = HashPassword(password);
            var user = _userDataAccess.ValidateUser(username, hashedPassword);

            if (user != null)
            {
                // Successful login, redirect to some secure page
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
        }

        private string HashPassword(string password)
        {
            // Implement a secure password hashing mechanism here
            return password; // For demonstration, use plain password (insecure, do not use in production)
        }
    }
}
