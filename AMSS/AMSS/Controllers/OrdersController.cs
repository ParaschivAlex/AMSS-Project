using AMSS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Admin, Delivery User")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var orders = from order in db.Orders select order;

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
            return View();
        }

        [Authorize(Roles = "User")]
        public ActionResult New()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            Order order = new Order();
            var currentUserId = User.Identity.GetUserId();
            var currentOrderList = db.OrderLists.Where(user => user.UserId == currentUserId).First();
            order.OrderDetails = db.OrderDetails.Where(orderDetail => orderDetail.OrderListId == currentOrderList.OrderListId).ToList();

            float payment = 0;

            foreach (var food in order.OrderDetails)
            {
                payment+= food.Price * food.Quantity;
            }
            order.Payment = payment;

            ViewBag.OrderDetails = order.OrderDetails;
            ViewBag.Payment = order.Payment;

            return View(order);
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
                        TempData["message"] = "Comanda a fost plasata!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["error"] = "Selecati produse inainte de a plasa comanda.";
                        return RedirectToAction("Index");
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

        [HttpPut]
        [Authorize(Roles = "Delivery User")] 
        public ActionResult UpdateStatus(int id)
        {
            var currentUserId = User.Identity.GetUserId();
            Order order = db.Orders.Find(id);

            if (order.Status == "In progress")
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

            TempData["error"] = "Nu aveti acces la aceasta comanda";
            return RedirectToAction("Index");
        }
    }
}