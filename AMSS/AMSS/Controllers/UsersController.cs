using AMSS.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AMSS.Repository;

namespace AMSS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private IUnitOfWork unitOfWork;

        public UsersController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        // GET: Users
        public ActionResult Index()
        {
            List<ApplicationUser> users = unitOfWork.UserRepository.Get(
                orderBy: q => q.OrderBy(user => user.UserName)
            ).ToList();
            IEnumerable<IdentityRole> roles = unitOfWork.RolesRepository.Get().ToList();
            ViewBag.UsersList = users;
            ViewBag.Roles = roles;
            return View();
        }

        public ActionResult Show(string id)
        {

            ApplicationUser user = unitOfWork.UserRepository.GetByID(id);
            ViewBag.utilizatorCurent = User.Identity.GetUserId();


            string currentRole = user.Roles.FirstOrDefault().RoleId;

            var userRoleName = unitOfWork.RolesRepository.Get(role => role.Id == currentRole).First().Name;

            ViewBag.roleName = userRoleName;

            return View(user);
        }

        public ActionResult Edit(string id)
        {
            ApplicationUser user = unitOfWork.UserRepository.GetByID(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            return View(user);
        }

        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = unitOfWork.UserRepository.GetByID(id);
            user.AllRoles = GetAllRoles();
            IdentityUserRole userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;

            try
            {
                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;
                    newData.AllRoles = user.AllRoles;

                    IdentityUserRole userToRolesToDelete = unitOfWork.UserToRolesRepository.Get(
                        userToRole => userToRole.UserId == user.Id
                    ).First();
                    unitOfWork.UserToRolesRepository.Delete(userToRolesToDelete);

                    IdentityUserRole newUserToRole = new IdentityUserRole();
                    newUserToRole.RoleId = HttpContext.Request.Params.Get("newRole");
                    newUserToRole.UserId = user.Id;
                    unitOfWork.UserToRolesRepository.Insert(newUserToRole);
                    unitOfWork.Save();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                newData.Id = id;
                return View(newData);
            }
        }

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            ApplicationUser userToDelete = unitOfWork.UserRepository.GetByID(id);
            unitOfWork.UserRepository.Delete(userToDelete);
            IEnumerable<IdentityUserRole> userRolesEntryToClean = unitOfWork.UserToRolesRepository.Get(
                userToRole => userToRole.UserId == id
            );
            foreach (IdentityUserRole entryToDelete in userRolesEntryToClean)
            {
                unitOfWork.UserToRolesRepository.Delete(entryToDelete);
            }
            unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = unitOfWork.RolesRepository.Get();
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}