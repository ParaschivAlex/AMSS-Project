using AMSS.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AMSS.Repository
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        /*
        In order to add a new type to the database and repository you need to:
        - create a private GenericRepository type here;
            ex (Restaurants type): private GenericRepository<Restaurant> restaurantRepository;
        - create a method that creates or retrieves (if it already exists) the repository for your type;
            ex: look for "RestaurantRepository" method below;
        After you implement this, you should be able to call all repository methods for your entity type
            without any extra hustle;
         */
        private bool disposed = false;

        private readonly ApplicationDbContext dbContext;
        private GenericRepository<Restaurant> restaurantRepository;
        private GenericRepository<Review> reviewRepository;
        private GenericRepository<Food> foodRepository;

        public UnitOfWork()
        {
            this.dbContext = new ApplicationDbContext();
        }

        public GenericRepository<Restaurant> RestaurantRepository
        {
            get
            {

                if (this.restaurantRepository == null)
                {
                    this.restaurantRepository = new GenericRepository<Restaurant>(dbContext);
                }
                return this.restaurantRepository;
            }
        }
        public GenericRepository<Review> ReviewRepository
        {
            get
            {

                if (this.reviewRepository == null)
                {
                    this.reviewRepository = new GenericRepository<Review>(dbContext);
                }
                return this.reviewRepository;
            }
        }
        public GenericRepository<Food> FoodRepository
        {
            get
            {

                if (this.foodRepository == null)
                {
                    this.foodRepository = new GenericRepository<Food>(dbContext);
                }
                return this.foodRepository;
            }
        }

        public void Save() => dbContext.SaveChanges();

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    dbContext.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}