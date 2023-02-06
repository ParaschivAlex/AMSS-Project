using AMSS.Models;
using AMSS.Repository;
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
        private IUnitOfWork unitOfWork;

        public OrderDetailsController()
        {
            unitOfWork = new UnitOfWork();
        }

        [Authorize(Roles = "User")]
        public OrderList GetOrderList()
        {
            var currentUserId = User.Identity.GetUserId();

            OrderList orderList = unitOfWork.OrdersListsRepository.Get(user => user.UserId == currentUserId).FirstOrDefault();

            //SINGLETON

            if (orderList == null)
            {
                orderList = new OrderList
                {
                    OrderDetails = new List<OrderDetail>(),
                    UserId = currentUserId
                };

                unitOfWork.OrdersListsRepository.Insert(orderList);
                unitOfWork.Save();
            }

            return orderList;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult New(int foodId)
        {
            var currentUserId = User.Identity.GetUserId();

            Food food = unitOfWork.FoodRepository.Get(a => a.FoodId == foodId).FirstOrDefault();

            OrderList orderList = GetOrderList();

            if (orderList.OrderDetails.Where(a => a.FoodId == food.FoodId).Count() != 0)
            {
                OrderDetail selectedOrder = orderList.OrderDetails.Where(a => a.FoodId == food.FoodId).First();
                selectedOrder.Quantity++;
                unitOfWork.Save();
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

                unitOfWork.OrdersDetailsRepository.Insert(newOrderDetail);
                unitOfWork.Save();
            }

            TempData["message"] = "Product was added to your cart!";
            return Redirect("/Restaurants/Show/" + food.RestaurantId);
        }

        [Authorize(Roles = "User")]
        public ActionResult Delete(int id)
        {
            var currentUserId = User.Identity.GetUserId();

            OrderDetail orderDetail = unitOfWork.OrdersDetailsRepository.GetByID(id);
            OrderList orderList = unitOfWork.OrdersListsRepository.GetByID(orderDetail.OrderListId);

            if (orderList.UserId == currentUserId)
            {
                if (orderDetail.Quantity > 1)
                {
                    orderDetail.Quantity--;
                }
                else
                {
                    unitOfWork.OrdersDetailsRepository.Delete(orderDetail);
                }
                unitOfWork.Save();
            }

            return Redirect("/OrderLists/Index");
        }
    }
}