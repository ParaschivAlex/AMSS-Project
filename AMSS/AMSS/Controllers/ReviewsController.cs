using AMSS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Controllers
{
    public class ReviewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var reviews = from review in db.Reviews.Include("Restaurant")
                          orderby review.ReviewId
                          select review;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            ViewBag.Reviews = reviews;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Show(int id)
        {
            Review review = db.Reviews.Find(id);
            ViewBag.Review = review;
            return View();
        }

        [Authorize(Roles = "Admin, User, Restaurant Manager, Delivery User")]
        public ActionResult Edit(int id)
        {
            Review rev = db.Reviews.Find(id);
            if (rev.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                ViewBag.CurrentUser = User.Identity.GetUserId();
                ViewBag.Review = rev;
                return View(rev);
            }
            else
            {
                TempData["message"] = "You do not have the rights to modify this review!";
                return RedirectToAction("Index", "Restaurants");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin, User, Restaurant Manager, Delivery User")]
        public ActionResult Edit(int id, Review requestReview)
        {
            try
            {
                Review rev = db.Reviews.Find(id);
                //System.Diagnostics.Debug.WriteLine("found review");

                if (rev.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                {
                    //System.Diagnostics.Debug.WriteLine("checked user");

                    //if (ModelState.IsValid)
                    if (TryUpdateModel(rev))
                    {
                        //System.Diagnostics.Debug.WriteLine("entered update");

                        rev.ReviewComment = requestReview.ReviewComment;
                        rev.ReviewGrade = requestReview.ReviewGrade;
                        rev.ReviewModifyDate = DateTime.Now;
                        db.SaveChanges();

                        //System.Diagnostics.Debug.WriteLine("pass update");
                    }

                    //System.Diagnostics.Debug.WriteLine("final update");
                    return Redirect("/Restaurants/Show/" + rev.RestaurantId);
                }
                else
                {
                    TempData["message"] = "You do not have the rights to modify this review!";
                    return RedirectToAction("Index", "Restaurants");
                }

            }
            catch (Exception)
            {
                return View(requestReview);
            }

        }

        [Authorize(Roles = "Admin")]
        public ActionResult New()
        {
            return View();
        }

        [Authorize(Roles = "Admin, User, Restaurant Manager, Delivery User")]
        [HttpPost]
        public ActionResult New(Review rev)
        {
            rev.ReviewModifyDate = DateTime.Now;
            rev.UserId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                db.Reviews.Add(rev);
                db.SaveChanges();
                TempData["message"] = "The review has been added successfully!";
            }
            return Redirect("/Restaurants/Show/" + rev.RestaurantId);
        }

        [Authorize(Roles = "Admin, User, Restaurant Manager, Delivery User")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {

            Review rev = db.Reviews.Find(id);
            if (rev.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(rev);
                TempData["message"] = "The review has been deleted!";
                db.SaveChanges();
                return Redirect("/Restaurants/Show/" + rev.RestaurantId);
            }
            else
            {
                TempData["message"] = "You do not have the rights to delete this review!";
                return Redirect("/Restaurants/Show/" + rev.RestaurantId);
            }
        }
    }
}