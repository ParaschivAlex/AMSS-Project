using AMSS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Delivery User")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var currentUserId = User.Identity.GetUserId();

            var userOrders = db.Orders.Where(a => a.DeliveryUserId == currentUserId && a.Status != "Delivered").OrderByDescending(a => a.OrderDate);

            var orders = db.Orders.Where(a => a.DeliveryUserId == null).OrderByDescending(a => a.OrderDate);

            ViewBag.NoOrders = !userOrders.ToList().Any();
            ViewBag.UserOrders = userOrders.ToList();
            ViewBag.Orders = orders.ToList();
            return View();
        }

        [Authorize(Roles = "User")]
        public ActionResult UserOrders()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var currentUserId = User.Identity.GetUserId();

            var orders = from order in db.Orders
                         where order.UserId == currentUserId
                         orderby order.OrderDate descending
                         select order;

            ViewBag.Orders = orders.ToList();
            ViewBag.NoOrders = !orders.ToList().Any();
            return View();
        }

        [Authorize(Roles = "User")]
        public ActionResult New(string address)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            Order order = new Order();
            var currentUserId = User.Identity.GetUserId();
            var currentOrderList = db.OrderLists.Where(user => user.UserId == currentUserId).First();
            order.OrderDetails = db.OrderDetails.Where(orderDetail => orderDetail.OrderListId == currentOrderList.OrderListId).ToList();
            order.UserId = currentUserId;
            order.Address = address;
            float payment = 0;

            foreach (var food in order.OrderDetails)
            {
                payment+= food.Price * food.Quantity;
            }
            order.Payment = payment;

            ViewBag.OrderDetails = order.OrderDetails;
            ViewBag.Payment = order.Payment;

            return New(order);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult New(Order order)
        {
            order.OrderDate = DateTime.Now;
            order.Status = "In progress";
            try
            {
                if (ModelState.IsValid)
                {
                    if (order.OrderDetails.Count > 0)
                    {
                        db.Orders.Add(order);
                        db.SaveChanges();
                        TempData["message"] = "The order was placed!";

                        var currentUserId = User.Identity.GetUserId();
                        var orderList = db.OrderLists.Where(a => a.UserId == currentUserId).FirstOrDefault();

                        foreach (var orderDetail in orderList.OrderDetails.ToList())
                        {
                            orderDetail.OrderListId = null;
                            orderDetail.OrderId = order.OrderId;
                            db.Entry(orderDetail).CurrentValues.SetValues(orderDetail);
                        }

                        db.SaveChanges();

                        return Redirect("/Orders/UserOrders");
                    }
                    else
                    {
                        TempData["error"] = "Add food to your cart to place an order.";
                        return Redirect("/Orders/UserOrders");
                    }
                }
                else
                {
                    return View(order);
                }
            }
            catch (Exception e)
            {
                return View(order);
            }
        }

        public ActionResult Show(int id)
        {
            try
            {
                var order = db.Orders.Find(id);
                var orderDetails = db.OrderDetails.Where(a => a.OrderId == id).Include(a => a.Food);
                var currentUserId = User.Identity.GetUserId();
                if (order != null || order.UserId != currentUserId)
                {
                    ViewBag.OrderDetails = orderDetails;
                    ViewBag.Order = order;
                    return View(order);
                }
                else
                {
                    throw new NullReferenceException("Invalid order.");
                }
            }
            catch (Exception e)
            {
                TempData["message"] = e;
                return Redirect("/Orders/UserOrders");
            }
        }

        [Authorize(Roles = "Delivery User")] 
        public ActionResult UpdateStatus(int id)
        {
            var currentUserId = User.Identity.GetUserId();
            Order order = db.Orders.Find(id);

            if (order.DeliveryUserId == null && order.Status == "In progress")
            {
                order.Status = "Picked up";
                order.DeliveryUserId = currentUserId;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            if (order.DeliveryUserId == currentUserId && order.Status == "Picked up")
            {
                order.Status = "Delivered";
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            TempData["error"] = "You don't have the necessary permission to update this order.";
            return RedirectToAction("Index");
        }
    }
}