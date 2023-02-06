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
        public ActionResult New(int foodId)
        {
            var currentUserId = User.Identity.GetUserId();

            Food food = db.Foods.Where(a => a.FoodId == foodId).FirstOrDefault();

            OrderList orderList = db.OrderLists.Where(user => user.UserId == currentUserId).FirstOrDefault();

            if (orderList == null)
            {
                orderList = new OrderList
                {
                    UserId = currentUserId
                };
                db.SaveChanges();
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
                    OrderListId = orderList.OrderListId,
                    Quantity = 1
                };

                db.OrderDetails.Add(newOrderDetail);
                db.SaveChanges();
            }

            TempData["message"] = "Product was added to your cart!";
            return Redirect("/Restaurants/Show/" + food.RestaurantId);
        }

        [Authorize(Roles = "User")]
        public ActionResult Delete(int id)
        {
            var currentUserId = User.Identity.GetUserId();

            OrderDetail orderDetail = db.OrderDetails.Find(id);
            OrderList orderList = db.OrderLists.Find(orderDetail.OrderListId);

            if (orderList.UserId == currentUserId)
            {
                if (orderDetail.Quantity > 1)
                {
                    orderDetail.Quantity--;
                }
                else
                {
                    db.OrderDetails.Remove(orderDetail);
                }
                db.SaveChanges();
            }

            return Redirect("/OrderLists/Index");
        }
    }
}