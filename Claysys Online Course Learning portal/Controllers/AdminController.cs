using System.Web.Mvc;
using Claysys_Online_Course_Learning_portal.DataAccess;
using Claysys_Online_Course_Learning_portal.Models;
using BCrypt.Net;
using System.Web.Security;

namespace Claysys_Online_Course_Learning_portal.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminDataAccess _adminDataAccess = new AdminDataAccess();
        private readonly UserDataAccess _userDataAccess = new UserDataAccess();

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
    }
}
