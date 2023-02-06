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
    public class ReviewsController : Controller
    {
        private IUnitOfWork unitOfWork;

        public ReviewsController()
        {
            this.unitOfWork = new UnitOfWork();
        }
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
            Review review = unitOfWork.ReviewRepository.GetByID(id);
            ViewBag.Review = review;
            return View();
        }

        [Authorize(Roles = "Admin, User, Restaurant Manager, Delivery User")]
        public ActionResult Edit(int id)
        {
            Review rev = unitOfWork.ReviewRepository.GetByID(id);
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
                Review rev = unitOfWork.ReviewRepository.GetByID(id);
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
                        unitOfWork.Save();

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
                unitOfWork.ReviewRepository.Insert(rev);
                unitOfWork.Save();
                TempData["message"] = "The review has been added successfully!";
            }
            return Redirect("/Restaurants/Show/" + rev.RestaurantId);
        }

        [Authorize(Roles = "Admin, User, Restaurant Manager, Delivery User")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {

            Review rev = unitOfWork.ReviewRepository.GetByID(id);
            if (rev.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                unitOfWork.ReviewRepository.Delete(rev);
                TempData["message"] = "The review has been deleted!";
                unitOfWork.Save();
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