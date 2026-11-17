
using BEE.Models;
using BEE.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
// Sử dụng Model từ BEE.Models
using Category = BEE.Models.Category;
using Product = BEE.Models.Product;

namespace BEE.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Thêm dữ liệu mẫu
        private void SeedData()
        {
            try
            {
                if (!db.Categories.Any())
                {
                    db.Categories.Add(new Category
                    {
                        CategoryName = "Hành động (Action)"
                    });

                    db.Categories.Add(new Category
                    {
                        CategoryName = "Nhập vai (RPG)"
                    });

                    db.Categories.Add(new Category
                    {
                        CategoryName = "Thế giới mở (Open World)"
                    });

                    db.Categories.Add(new Category
                    {
                        CategoryName = "Thể thao (Sports)"
                    });

                    db.SaveChanges();
                }

                if (!db.Products.Any())
                {
                    var actionCat = db.Categories.FirstOrDefault(
                        c => c.CategoryName.Contains("Hành động"));

                    var rpgCat = db.Categories.FirstOrDefault(
                        c => c.CategoryName.Contains("Nhập vai"));

                    var openWorldCat = db.Categories.FirstOrDefault(
                        c => c.CategoryName.Contains("Thế giới mở"));

                    var sportsCat = db.Categories.FirstOrDefault(
                        c => c.CategoryName.Contains("Thể thao"));

                    int defaultId = db.Categories
                        .Select(c => c.CategoryID)
                        .FirstOrDefault();

                    db.Products.Add(new Product
                    {
                        ProductName = "Grand Theft Auto V",
                        CategoryID = openWorldCat != null ? openWorldCat.CategoryID : defaultId,
                        ProductPrice = 495000,
                        ProductImage = "https://cdn.cloudflare.steamstatic.com/steam/apps/271590/header.jpg"
                    });

                    db.Products.Add(new Product
                    {
                        ProductName = "Cyberpunk 2077",
                        CategoryID = rpgCat != null ? rpgCat.CategoryID : defaultId,
                        ProductPrice = 990000,
                        ProductImage = "https://cdn.cloudflare.steamstatic.com/steam/apps/1091500/header.jpg"
                    });

                    db.Products.Add(new Product
                    {
                        ProductName = "Elden Ring",
                        CategoryID = rpgCat != null ? rpgCat.CategoryID : defaultId,
                        ProductPrice = 1150000,
                        ProductImage = "https://cdn.cloudflare.steamstatic.com/steam/apps/1245620/header.jpg"
                    });

                    db.Products.Add(new Product
                    {
                        ProductName = "Counter-Strike 2",
                        CategoryID = actionCat != null ? actionCat.CategoryID : defaultId,
                        ProductPrice = 0,
                        ProductImage = "https://cdn.cloudflare.steamstatic.com/steam/apps/730/header.jpg"
                    });

                    db.Products.Add(new Product
                    {
                        ProductName = "EA SPORTS FC 24",
                        CategoryID = sportsCat != null ? sportsCat.CategoryID : defaultId,
                        ProductPrice = 1050000,
                        ProductImage = "https://cdn.cloudflare.steamstatic.com/steam/apps/2195250/header.jpg"
                    });

                    db.Products.Add(new Product
                    {
                        ProductName = "Red Dead Redemption 2",
                        CategoryID = openWorldCat != null ? openWorldCat.CategoryID : defaultId,
                        ProductPrice = 1000000,
                        ProductImage = "https://cdn.cloudflare.steamstatic.com/steam/apps/1174180/header.jpg"
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Seed Error: " + ex.Message);
            }
        }

        // Trang chủ
        public ActionResult Index(int? categoryId, string searchString)
        {
            SeedData();

            ViewBag.Categories = db.Categories.AsNoTracking().ToList();

            ViewBag.SelectedCategory = categoryId;

            ViewBag.SearchString = searchString;

            var products = db.Products.AsQueryable();

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                products = products.Where(
                    p => p.CategoryID == categoryId.Value);
            }

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(
                    p => p.ProductName.Contains(searchString));
            }

            return View(products.ToList());
        }

        // Chi tiết sản phẩm
        public ActionResult Details(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // Giải phóng kết nối database
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
