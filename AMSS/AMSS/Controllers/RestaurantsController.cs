using AMSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Controllers
{
    public class RestaurantsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Categories
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var restaurants = from rest in db.Restaurants
                             orderby rest.RestaurantName
                             select rest;

            var restaurantsToUpdateRating = db.Restaurants.ToList();

            foreach (var restaurant in restaurantsToUpdateRating)
            {
                RatingChecker(restaurant.RestaurantId);
            }

            ViewBag.Restaurants = restaurants;
            return View();
        }

        public ActionResult Show(int id)
        {
            try
            {
                Restaurant restaurant = db.Restaurants.Find(id);
                var foods = from food in db.Foods
                                 where food.RestaurantId == restaurant.RestaurantId
                                 select food;
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
                db.Restaurants.Add(res);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            Restaurant restaurant = db.Restaurants.Find(id);
            return View(restaurant);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, Restaurant requestRestaurant)
        {
            try
            {
                Restaurant restaurant = db.Restaurants.Find(id);
                if (TryUpdateModel(restaurant))
                {
                    restaurant = requestRestaurant;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(requestRestaurant);
            }
            catch (Exception e)
            {
                return View(requestRestaurant);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Restaurant restaurant = db.Restaurants.Find(id);

            var reviews = db.Reviews.Where(fr => fr.RestaurantId == id);
            foreach (var review in reviews)
            {
                db.Reviews.Remove(review);
            }

            /*List<int> ordersCompleted = db.OrderCompletes.Where(ordCom => ordCom.FreshenerId == freshener.FreshenerId).Select(ordC => ordC.OrderId).ToList();
			foreach (var orderCompleted in ordersCompleted)
			{
				Order orde = db.Orders.Find(orderCompleted);
					db.Orders.Remove(orde);
				}
			}*/

            db.Restaurants.Remove(restaurant);
            db.SaveChanges();
            TempData["message"] = "The restaurant has been deleted!";
            return RedirectToAction("Index");
        }

        [HttpPut]
        public ActionResult RatingChecker(int id)
        {
            int rating = 0;
            int numberOfReviews = 0;
            Restaurant restaurant = db.Restaurants.Find(id);

            var reviews = db.Reviews.Where(rv => rv.RestaurantId == restaurant.RestaurantId);
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
                        db.SaveChanges();
                        return View();
                    }
                    return View();
                }
            }
            else
            {
                return View();
            }
            return View();
        }
    }
}