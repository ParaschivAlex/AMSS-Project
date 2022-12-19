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
        GenericRepository<Review> ReviewRepository { get; }
        GenericRepository<Food> FoodRepository { get; }
        void Save();
        void Dispose();
    }
}
