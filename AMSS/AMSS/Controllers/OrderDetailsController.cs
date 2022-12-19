using AMSS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Controllers
{
    public class OrderDetailsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult New(Food food)
        {
            var currentUserId = User.Identity.GetUserId();

            OrderList orderList = db.OrderLists.Where(user => user.UserId == currentUserId).FirstOrDefault();

            if (orderList == null)
            {
                // Current user is starting to order from a new restaurant;
                orderList = new OrderList
                {
                    RestaurantId = food.RestaurantId,
                    UserId = currentUserId
                };
                db.SaveChanges();
            }

            if (orderList.OrderDetails.Count > 0)
            {
                var foodInList = orderList.OrderDetails.First().Food;
                if (food.RestaurantId != foodInList.RestaurantId)
                {
                    // Current user is shopping at another restaurant
                    orderList = new OrderList
                    {
                        RestaurantId = food.RestaurantId,
                        UserId = currentUserId
                    };
                    db.SaveChanges();
                }
            }

            if (orderList.OrderDetails.Where(a => a.FoodId == food.FoodId).Count() != 0)
            {
                OrderDetail selectedOrder = orderList.OrderDetails.Where(a => a.FoodId == food.FoodId).First();
                selectedOrder.Quantity++;
                db.SaveChanges();
            }
            else
            {
                OrderDetail newOrderDetail = new OrderDetail
                {
                    FoodId = food.FoodId,
                    Price = food.FoodPrice,
                    OrderListId = orderList.OrderListId
                };

                db.OrderDetails.Add(newOrderDetail);
                db.SaveChanges();
            }

            return View();
        }

        [HttpDelete]
        [Authorize(Roles = "User")]
        public ActionResult Delete(int id)
        {
            OrderDetail orderDetail = db.OrderDetails.Find(id);
            db.OrderDetails.Remove(orderDetail);
            db.SaveChanges();
            return View();
        }
    }
}