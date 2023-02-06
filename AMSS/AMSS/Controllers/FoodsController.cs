using AMSS.Models;
using AMSS.Repository;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Controllers
{
    public class FoodsController : Controller
    {
        private IUnitOfWork unitOfWork;

        public FoodsController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        public ActionResult Index(string sortOrder, string search, string currentFilter)
        {
            ViewBag.CurrentSort = sortOrder;

            if (search == null)
            {
                search = currentFilter;
            }

            ViewBag.CurrentFilter = search;

            var foods = unitOfWork.FoodRepository.Get();
           
            if (!String.IsNullOrEmpty(search))
            {
                foods = foods.Where(fr => fr.FoodName.Contains(search) || fr.FoodIngredients.Contains(search));
            }
           
            switch (sortOrder)
            {
                case "1": //increasing by price
                    foods = foods.OrderBy(f => f.FoodPrice);
                    //System.Diagnostics.Debug.WriteLine("increasing price");
                    break;
                case "2": //decreasing by price
                    foods = foods.OrderByDescending(f => f.FoodPrice);
                    //System.Diagnostics.Debug.WriteLine("decreasing price");
                    break;
                case "5": //newest
                    foods = foods.OrderBy(f => f.FoodModifyDate);
                    //System.Diagnostics.Debug.WriteLine("newest");
                    break;
                case "6": //oldest
                    foods = foods.OrderByDescending(f => f.FoodModifyDate);
                    //System.Diagnostics.Debug.WriteLine("oldest");
                    break;
                default:
                    foods = foods.OrderBy(f => f.FoodModifyDate);
                    break;
            }

            var numberOfFoods = foods.Count();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.total = numberOfFoods;
            ViewBag.Foods = foods;
            //ViewBag.SearchString = search;
            //db.SaveChanges();
            return View();
        }

        public ActionResult Show(int id)
        {
            Food food = unitOfWork.FoodRepository.GetByID(id);
            ViewBag.Food = food;
            ViewBag.Restaurant = food.Restaurant;            
            ViewBag.currentUser = User.Identity.GetUserId();
            return View(food);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult New()
        {
            Food food = new Food();
            food.RestaurantList = GetRestaurants();

            return View(food);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult New(Food food)
        {
            food.FoodModifyDate = DateTime.Now;
            try
            {
                if (ModelState.IsValid)
                {
                    unitOfWork.FoodRepository.Insert(food);
                    unitOfWork.Save();
                    //Console.WriteLine("DB.SAVEDCHANGES");
                    TempData["message"] = "The food has been added! Add another food?";
                    return Redirect("/Home/Index");
                }
                else
                {
                    Console.WriteLine("Error on modelstate.isvalid for adding a new food.");
                    food.RestaurantList = GetRestaurants();
                    return View(food);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error on try catch for adding a new food.");
                food.RestaurantList = GetRestaurants();
                return View(food);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            Food food = unitOfWork.FoodRepository.GetByID(id);
            food.RestaurantList = GetRestaurants();
            ViewBag.Freshener = food;
            ViewBag.Restaurant = food.Restaurant;
            ViewBag.currentUser = User.Identity.GetUserId();
            return View(food);

        }
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public ActionResult Edit(int id, Food requestFood)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    Food food = unitOfWork.FoodRepository.GetByID(id);

                    if (TryUpdateModel(food))
                    {
                        food = requestFood;
                        food.FoodModifyDate = DateTime.Now;
                        unitOfWork.Save();
                        TempData["message"] = "The food has been modified!";
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return View(requestFood);
            }

        }    

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            Food food = unitOfWork.FoodRepository.GetByID(id);
            unitOfWork.FoodRepository.Delete(food);
            unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetRestaurants()
        {
            var selectList = new List<SelectListItem>();
            var restaurants = unitOfWork.RestaurantRepository.Get();

            foreach (var restaurant in restaurants)
            {
                selectList.Add(new SelectListItem
                {
                    Value = restaurant.RestaurantId.ToString(),
                    Text = restaurant.RestaurantName.ToString()
                });
            }
            return selectList;
        }        
    }
}