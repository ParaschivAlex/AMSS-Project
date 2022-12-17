using AMSS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
// Used https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application 
// as a reference

namespace AMSS.Repository
{
    public class GenericRepository<EntityType> : IGenericRepository<EntityType> where EntityType : class 
    {
        private readonly ApplicationDbContext dbContext;
        private readonly DbSet<EntityType> dbSet;

        public GenericRepository(ApplicationDbContext dbContext = null)
        {
            if (dbContext == null)
            {
                this.dbContext = new ApplicationDbContext();
            } else
            {
                this.dbContext = dbContext;
            }
            dbSet = dbContext.Set<EntityType>();
        }

        // Get a set of entities of the given type based on a couple of conditions
        public virtual IEnumerable<EntityType> Get(
            Expression<Func<EntityType, bool>> filter = null,
            Func<IQueryable<EntityType>, IOrderedQueryable<EntityType>> orderBy = null,
            string includeProperties = "")
        // You can add a filter option by simply adding a boolean example when calling Get
        //  ex: x => x.LastName == "Smith"
        // Similarly for the "orderBy" parameter, you may add it in order to specify a certain sorting lambda expression
        //  ex: q => q.OrderBy(s => s.LastName)
        // Lastly, if you want to include in the results anything else that is outside of the given results, but related to them
        //  ex: Extract all users and their orders from the DB => GenericRepository<Users>.Get(includeProperties="Orders");
        {
            IQueryable<EntityType> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        // Extracts using the object id
        public virtual EntityType GetByID(object id) => dbSet.Find(id);

        // Inserts a new entity of any type into the database
        public virtual void Insert(EntityType entity) => dbSet.Add(entity);

        // Deletes an object using its id from the database
        public virtual void Delete(object id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            EntityType entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        // Deletes an object from the database using its instance
        public virtual void Delete(EntityType entityToDelete)
        {
            if (dbContext.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        // Updates an object in the database using its instance
        public virtual void Update(EntityType entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            dbContext.Entry(entityToUpdate).State = EntityState.Modified;
        }

        // Save changes to the database
        public virtual void Save() => dbContext.SaveChanges();
    }
}