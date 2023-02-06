using AMSS.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AMSS.Repository
{
    internal interface IUnitOfWork
    {
        /*
        In order to add a new type to the database and repository you need to:
        - Add a getter interface here
            ex(Restaurant Type): GenericRepository<Restaurant> RestaurantRepository { get; }
        - Follow the next steps in UnitOfWork.cs
         */
        GenericRepository<Restaurant> RestaurantRepository { get; }
        GenericRepository<Order> OrdersRepository { get; }
        GenericRepository<OrderList> OrdersListsRepository { get; }
        GenericRepository<OrderDetail> OrdersDetailsRepository { get; }
        GenericRepository<Review> ReviewRepository { get; }
        GenericRepository<Food> FoodRepository { get; }
        GenericRepository<ApplicationUser> UserRepository { get; }
        GenericRepository<IdentityRole> RolesRepository { get; }
        GenericRepository<IdentityUserRole> UserToRolesRepository { get; }

        void Save();
        void Dispose();
    }
}
