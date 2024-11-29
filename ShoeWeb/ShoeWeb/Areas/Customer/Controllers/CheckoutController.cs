﻿using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using ShoeWeb.App_Start;
using ShoeWeb.Data;
using ShoeWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ShoeWeb.Areas.Customer.CustomerVM;
using ShoeWeb.Utility;
using System.Diagnostics;

namespace ShoeWeb.Areas.Customer.Controllers
{
    [Authorize(Roles = SD.CustomerRole)]

    public class CheckoutController : Controller
    {

        private readonly ApplicationDbContext _db;
        private ApplicationUserManager _userManager;

        public CheckoutController(ApplicationDbContext db)
        {
            _db = db;
        }
        public CheckoutController() : this(new ApplicationDbContext())
        {
        }
        public CheckoutController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public async Task<List<ShoppingCartItem>> GetCartItems()
        {
            if (User.Identity.IsAuthenticated)
            {

                var userId = User.Identity.GetUserId();
                var cart = await _db.shoppingCarts
                .Where(c => c.UserId == userId)
                .Include("ShoppingCartItems.Product")
                .FirstOrDefaultAsync();

                return cart?.ShoppingCartItems.ToList() ?? new List<ShoppingCartItem>();



            }
            else
            {
                var cart = Session["Cart"] as List<ShoppingCartItem>;
                return cart?.Select(ci => new ShoppingCartItem
                {
                    ProductId = ci.ProductId,
                    Product = ci.Product,
                    Price = ci.Price,
                    Quantity = ci.Quantity
                }).ToList() ?? new List<ShoppingCartItem>();
            }
        }

        // GET: Customer/Checkout
        [HttpGet]
        public async Task<ActionResult> Checkout()
        {
            OrderViewModel orderViewModel = new OrderViewModel();

            return View(orderViewModel);
        }
        public string GenerateOrderCode()
        {
            var lastOrder = _db.Orders.OrderByDescending(o => o.Id).FirstOrDefault();
            int newOrderId = (lastOrder == null) ? 1 : lastOrder.Id + 1;
            return "ORD" + newOrderId.ToString("D6"); // Sinh mã đơn hàng dạng ORD000001, ORD000002, ...
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    var cart = await _db.shoppingCarts.Where(c => c.UserId == userId).FirstOrDefaultAsync();
                    var cartItems = await _db.shoppingCartItems.Where(c => c.ShoppingCartId == cart.Id && c.status == false).ToListAsync();
                    Order order = new Order()
                    {
                        Code = GenerateOrderCode(), // Tạo mã đơn hàng
                        CustomerName = model.FirstName + " " + model.LastName,
                        Phone = model.Phone,
                        Address = model.Address,
                        TinhThanh = model.Province,
                        QuanHuyen = model.District,
                        PhuongXa = model.Ward,
                        TotalAmount = cart.TotalPrice, // Tính tổng tiền đơn hàng
                        Quantity = cartItems.Count, // Tổng số lượng sản phẩm trong giỏ
                        TypePayment = model.PaymentMethod == "VNPay" ? 1 : 0, // 1: VNPay, 0: COD
                        Status = 0, // Trạng thái đơn hàng (0: chưa xử lý)
                        CreatedDate = DateTime.Now,
                        isPayment = model.PaymentMethod == "VNPay" ? true : false, // Nếu là VNPay thì thanh toán, còn lại là COD -> chưa thanh toán
                        isAccept = false, // Chưa được xác nhận
                    };

                    _db.Orders.Add(order);
                    await _db.SaveChangesAsync();

                    if (await SaveOrderDetails(cartItems, order.Id))
                    {
                        return View("Success");
                    }
                    else
                    {
                        Debug.WriteLine("loi" );

                        return View("Error");
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ngoai le" + ex.Message);
                    return View("Error");
                }

            }

            else
            {
                return View(model);

            }
        }

        public async Task<bool> SaveOrderDetails(List<ShoppingCartItem> cartItems, int orderId)
        {
            var orderDetailItems = new List<OrderDetail>();

            // Duyệt qua từng item trong giỏ hàng để tạo OrderDetail và cập nhật trạng thái
            foreach (var item in cartItems)
            {
                var newOrderItem = new OrderDetail
                {
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Price = item.UnitPrice,
                    Quantity = item.Quantity,
                    numberSize = item.numberSize
                };

                // Cập nhật trạng thái của item thành true
                item.status = true;

                // Thêm OrderDetail vào danh sách
                orderDetailItems.Add(newOrderItem);

                // Cập nhật trạng thái của ShoppingCartItem trong cơ sở dữ liệu
                _db.Entry(item).State = EntityState.Modified; // Cập nhật trạng thái của từng item
            }

            try
            {
                // Thêm tất cả OrderDetail vào cơ sở dữ liệu
                _db.OrderDetails.AddRange(orderDetailItems);

                // Lưu tất cả các thay đổi vào cơ sở dữ liệu
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi để biết nguyên nhân nếu có
                Console.WriteLine(ex.Message);
                return false;
            }
        }


    }
}