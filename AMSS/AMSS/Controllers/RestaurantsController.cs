﻿using AMSS.Models;
using AMSS.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Controllers
{
    public class RestaurantsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IUnitOfWork unitOfWork;

        public RestaurantsController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        // GET: Categories
        [Authorize(Roles = "Admin, User")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            IEnumerable<Restaurant> restaurants = unitOfWork.RestaurantRepository.Get(
                orderBy: q => q.OrderBy(restaurant => restaurant.RestaurantName)
            );

            foreach (Restaurant restaurant in restaurants)
            {
                RatingChecker(restaurant.RestaurantId);
            }

            ViewBag.Restaurants = restaurants;
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View();
        }

        public ActionResult Show(int id)
        {
            try
            {
                Restaurant restaurant = unitOfWork.RestaurantRepository.GetByID(id);
                IEnumerable<Food> foods = unitOfWork.FoodRepository.Get(food=>food.RestaurantId==restaurant.RestaurantId);
                ViewBag.Reviews = restaurant.Reviews;
                if (foods != null)
                {
                    ViewBag.Restaurants = restaurant;
                    ViewBag.Foods = foods;
                    return View(restaurant);
                }
                else
                {
                    throw new NullReferenceException("You can't check a restaurant that has no foods!");
                }
            }
            catch (Exception e)
            {
                TempData["message"] = e;
                return Redirect("/Restaurants/Index");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult New(Restaurant res)
        {
            try
            {
                unitOfWork.RestaurantRepository.Insert(res);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = unitOfWork.RestaurantRepository.GetByID(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            return View(restaurant);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Restaurant restaurant)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RestaurantCreationError = "Model state is not valid...";
                return View(restaurant);
            }

            restaurant.Foods = unitOfWork.FoodRepository.Get(food => food.RestaurantId == restaurant.RestaurantId).ToArray();
            restaurant.Reviews = unitOfWork.ReviewRepository.Get(review => review.RestaurantId == restaurant.RestaurantId).ToArray();

            unitOfWork.RestaurantRepository.Update(restaurant);
            unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            Restaurant restaurant = unitOfWork.RestaurantRepository.GetByID(id);
            IEnumerable<Review> reviews = unitOfWork.ReviewRepository.Get(review => review.RestaurantId == restaurant.RestaurantId);

            foreach (Review review in reviews)
            {
                unitOfWork.ReviewRepository.Delete(review.ReviewId);
            }

            /*List<int> ordersCompleted = db.OrderCompletes.Where(ordCom => ordCom.FreshenerId == freshener.FreshenerId).Select(ordC => ordC.OrderId).ToList();
			foreach (var orderCompleted in ordersCompleted)
			{
				Order orde = db.Orders.Find(orderCompleted);
					db.Orders.Remove(orde);
				}
			}*/

            unitOfWork.RestaurantRepository.Delete(restaurant.RestaurantId);
            unitOfWork.Save();
            TempData["message"] = "The restaurant has been deleted!";
            return RedirectToAction("Index");
        }

        [HttpPut]
        public ActionResult RatingChecker(int id)
        {
            int rating = 0;
            int numberOfReviews = 0;
            Restaurant restaurant = unitOfWork.RestaurantRepository.GetByID(id);
            IEnumerable<Review> reviews = unitOfWork.ReviewRepository.Get(review => review.RestaurantId == restaurant.RestaurantId);

            if (reviews != null)
            {
                if (TryUpdateModel(restaurant))
                {
                    foreach (var rev in reviews)
                    {
                        rating += rev.ReviewGrade;
                        numberOfReviews++;
                    }
                    if (numberOfReviews != 0)
                    {
                        rating /= numberOfReviews;
                        //rating /= reviews.Count();
                        restaurant.RestaurantRating = rating;
                        //Debug.WriteLine(rating);
                        unitOfWork.Save();
                    }
                }
            }
            return View();
        }
    }
}