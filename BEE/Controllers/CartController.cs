
using BEE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BEE.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // 1. LẤY GIỎ HÀNG
        public List<CartItem> GetCart()
        {
            List<CartItem> cart = Session["Cart"] as List<CartItem>;

            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }

            return cart;
        }

        // 2. THÊM SẢN PHẨM
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            if (quantity < 1)
                quantity = 1;

            if (quantity > 99)
                quantity = 99;

            var product = db.Products.Find(id);

            if (product == null)
            {
                TempData["CartMessage"] = "Không tìm thấy sản phẩm!";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = product.ProductImage,
                    UnitPrice = product.ProductPrice,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity = Math.Min(item.Quantity + quantity, 99);
            }

            TempData["CartMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";

            return RedirectToAction("Index");
        }

        // 3. HIỂN THỊ GIỎ HÀNG
        public ActionResult Index()
        {
            var cart = GetCart();

            ViewBag.Total = cart.Sum(item => item.TotalPrice);
            ViewBag.CartCount = cart.Sum(item => item.Quantity);

            return View(cart);
        }

        // 4. XÓA SẢN PHẨM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                cart.Remove(item);
                TempData["CartMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction("Index");
        }

        // 5. CẬP NHẬT SỐ LƯỢNG
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = Math.Min(quantity, 99);
                }
            }

            return RedirectToAction("Index");
        }

        // 6. TRANG THANH TOÁN
        [HttpGet]
        public ActionResult Checkout()
        {
            var cart = GetCart();

            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Total = cart.Sum(item => item.TotalPrice);

            return View();
        }

        // 7. XỬ LÝ ĐẶT HÀNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string shippingAddress)
        {
            var cart = GetCart();

            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra địa chỉ
            if (string.IsNullOrWhiteSpace(shippingAddress)
                || shippingAddress.Length > 500)
            {
                ViewBag.Error = "Vui lòng nhập địa chỉ hợp lệ (tối đa 500 ký tự).";
                ViewBag.Total = cart.Sum(item => item.TotalPrice);
                return View();
            }

            // Kiểm tra khách hàng
            string username = Session["Username"].ToString();

            var customer = db.Customers.FirstOrDefault(
                c => c.Username == username
            );

            if (customer == null)
            {
                ViewBag.Error = "Không tìm thấy thông tin khách hàng.";
                ViewBag.Total = cart.Sum(item => item.TotalPrice);
                return View();
            }

            // Lấy lại sản phẩm và giá từ database
            var productIds = cart.Select(x => x.ProductID).Distinct().ToList();

            var products = db.Products
                .Where(p => productIds.Contains(p.ProductID))
                .ToList();

            if (products.Count != productIds.Count)
            {
                ViewBag.Error = "Một số sản phẩm không còn tồn tại.";
                ViewBag.Total = cart.Sum(item => item.TotalPrice);
                return View();
            }

            // Đồng bộ giá mới nhất từ database
            foreach (var item in cart)
            {
                var product = products.First(
                    p => p.ProductID == item.ProductID
                );

                item.UnitPrice = product.ProductPrice;
                item.ProductName = product.ProductName;
                item.ProductImage = product.ProductImage;

                if (item.Quantity < 1 || item.Quantity > 99)
                {
                    ViewBag.Error = "Số lượng sản phẩm không hợp lệ.";
                    ViewBag.Total = cart.Sum(x => x.TotalPrice);
                    return View();
                }
            }

            decimal total = cart.Sum(item => item.TotalPrice);

            // Lưu đơn hàng bằng Transaction
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Order order = new Order
                    {
                        CustomerID = customer.CustomerID,
                        OrderDate = DateTime.Now,
                        TotalAmount = total,
                        PaymentStatus = "Chưa thanh toán",
                        AddressDelivery = shippingAddress.Trim()
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    // Lưu chi tiết đơn hàng
                    foreach (var item in cart)
                    {
                        OrderDetail detail = new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        };

                        db.OrderDetails.Add(detail);
                    }

                    db.SaveChanges();

                    transaction.Commit();

                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    Session["Cart"] = null;

                    TempData["OrderMessage"] = "Đặt hàng thành công!";

                    return RedirectToAction("OrderSuccess");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(
                        "Checkout Error: " + ex.ToString()
                    );

                    ViewBag.Error = "Đặt hàng thất bại. Vui lòng thử lại.";
                    ViewBag.Total = total;

                    return View();
                }
            }
        }

        // 8. ĐẶT HÀNG THÀNH CÔNG
        public ActionResult OrderSuccess()
        {
            return View();
        }

        // 9. LỊCH SỬ ĐƠN HÀNG
        public ActionResult OrderHistory()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();

            var customer = db.Customers.FirstOrDefault(
                c => c.Username == username
            );

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = db.Orders
                .Where(o => o.CustomerID == customer.CustomerID)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // 10. CHI TIẾT ĐƠN HÀNG
        public ActionResult OrderDetail(int id)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();

            var customer = db.Customers.FirstOrDefault(
                c => c.Username == username
            );

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = db.Orders.FirstOrDefault(
                o => o.OrderID == id &&
                     o.CustomerID == customer.CustomerID
            );

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // 11. GIẢI PHÓNG DATABASE
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
