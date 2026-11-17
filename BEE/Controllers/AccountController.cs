using BEE.Models;
using BEE.Models.ViewModel;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace BEE.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                model.Username = (model.Username ?? "").Trim();

                var existingUser = db.Users.SingleOrDefault(
                    u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }

                User user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "Customer"
                };
                db.Users.Add(user);
                Customer customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.Email,
                    CustomerPhone = model.Phone,
                    CustomerAddress = model.Address,
                    Username = model.Username
                };
                db.Customers.Add(customer);

                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var username = (model.Username ?? "").Trim();

                var user = db.Users.SingleOrDefault(u =>
                    u.Username == username &&
                    u.Password == model.Password &&
                    (u.UserRole == "Customer" || u.UserRole == "Admin"));
                if (user != null)
                {
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}