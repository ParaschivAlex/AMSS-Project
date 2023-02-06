using AMSS.Models;
using AMSS.Repository;
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
        private IUnitOfWork unitOfWork;

        public OrdersController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        [Authorize(Roles = "Delivery User")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var currentUserId = User.Identity.GetUserId();

            var userOrders = unitOfWork.OrdersRepository.Get(filter: a => a.DeliveryUserId == currentUserId && a.Status != "Delivered").OrderByDescending(a => a.OrderDate);

            var orders = unitOfWork.OrdersRepository.Get(filter: a => a.DeliveryUserId == null).OrderByDescending(a => a.OrderDate);

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

            var orders = unitOfWork.OrdersRepository.Get(filter: a => a.UserId == currentUserId).OrderByDescending(a => a.OrderDate);

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
            var currentOrderList = unitOfWork.OrdersListsRepository.Get(user => user.UserId == currentUserId).First();
            order.OrderDetails = unitOfWork.OrdersDetailsRepository.Get(orderDetail => orderDetail.OrderListId == currentOrderList.OrderListId).ToList();
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
                        unitOfWork.OrdersRepository.Insert(order);
                        unitOfWork.Save();
                        TempData["message"] = "The order was placed!";

                        var currentUserId = User.Identity.GetUserId();
                        var orderList = unitOfWork.OrdersListsRepository.Get(a => a.UserId == currentUserId).FirstOrDefault();

                        foreach (var orderDetail in orderList.OrderDetails.ToList())
                        {
                            orderDetail.OrderListId = null;
                            orderDetail.OrderId = order.OrderId;
                            unitOfWork.OrdersDetailsRepository.Update(orderDetail);
                            // db.Entry(orderDetail).CurrentValues.SetValues(orderDetail);
                        }

                        unitOfWork.Save();

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

        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Show(int id)
        {
            try
            {
                var order = unitOfWork.OrdersRepository.GetByID(id);
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
            Order order = unitOfWork.OrdersRepository.GetByID(id);

            if (order.DeliveryUserId == null && order.Status == "In progress")
            {
                order.Status = "Picked up";
                order.DeliveryUserId = currentUserId;
                unitOfWork.Save();
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            if (order.DeliveryUserId == currentUserId && order.Status == "Picked up")
            {
                order.Status = "Delivered";
                unitOfWork.Save();
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            TempData["error"] = "You don't have the necessary permission to update this order.";
            return RedirectToAction("Index");
        }
    }
}