using BEE.Models;
using BEE.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace BEE.Controllers
{
    public class AdminController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private bool CheckAdmin()
        {
            if (Session["Username"] == null) return false;
            return true;
        }


        public ActionResult Index()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            var products = db.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        public ActionResult Create()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm tựa game mới thành công!";
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public ActionResult Edit(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin game thành công!";
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public ActionResult Delete(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            var product = db.Products.Find(id);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa game khỏi hệ thống thành công!";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Orders()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            var orders = db.Orders.Include(o => o.Customer)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToList();
            return View(orders);
        }

        public ActionResult OrderDetails(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account");
            var order = db.Orders.Include(o => o.Customer)
                                 .Include(o => o.OrderDetails.Select(d => d.Product))
                                 .FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }
    }
}