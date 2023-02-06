using AMSS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Controllers
{
    public class OrderListsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "User")]
        public OrderList GetOrderList()
        {
            var currentUserId = User.Identity.GetUserId();

            OrderList orderList = db.OrderLists.Where(user => user.UserId == currentUserId).FirstOrDefault();

            if (orderList == null)
            {
                orderList = new OrderList
                {
                    OrderDetails = new List<OrderDetail>(),
                    UserId = currentUserId
                };

                db.OrderLists.Add(orderList);
                db.SaveChanges();
            }

            return orderList;
        }

        [Authorize(Roles = "User")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var currentUserId = User.Identity.GetUserId();

            OrderList orderList = GetOrderList();

            var orderDetails = db.OrderDetails.Where(a => a.OrderListId == orderList.OrderListId).Include(orderDetail => orderDetail.Food);

            var total = 0f;
            foreach (var orderDetail in orderDetails)
            {
                total += orderDetail.Quantity * orderDetail.Price;
            }

            ViewBag.NoFood = !orderDetails.ToList().Any();
            ViewBag.OrderListFoods = orderDetails;
            ViewBag.TotalPrice = total;

            return View();
        }
    }
}