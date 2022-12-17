using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AMSS.Repository
{
    internal interface IGenericRepository<EntityType> where EntityType : class
    {
        // Get a set of entities of the given type based on a couple of conditions
        // Call without any parameters to get all entries for a certain entity type from database
        IEnumerable<EntityType> Get(Expression<Func<EntityType, bool>> filter = null,
            Func<IQueryable<EntityType>, IOrderedQueryable<EntityType>> orderBy = null,
            string includeProperties = "");

        // Extracts using the object id
        EntityType GetByID(object id);

        // Inserts a new entity of any type into the database
        void Insert(EntityType entity);

        // Deletes an object using its id from the database
        void Delete(object id);

        // Deletes an object from the database using its instance
        void Delete(EntityType entityToDelete);

        // Updates an object in the database using its instance
        void Update(EntityType entityToUpdate);

        // Save changes to the database
        void Save();
    }
}
