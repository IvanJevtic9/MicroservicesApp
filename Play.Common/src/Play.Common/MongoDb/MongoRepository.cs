using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Play.Common.MongoDb
{
    public class MongoRepository<TEntity> : IRepository<TEntity> where TEntity : IEntity, new()
    {
        private readonly IMongoCollection<TEntity> dbCollection;
        private readonly FilterDefinitionBuilder<TEntity> filterBuilder = Builders<TEntity>.Filter;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            dbCollection = database.GetCollection<TEntity>(collectionName);
        }

        public async Task<IReadOnlyCollection<TEntity>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await dbCollection.Find(filter).ToListAsync();
        }

        public async Task<TEntity> GetAsync(Guid id)
        {
            var filter = filterBuilder.Eq(entity => entity.Id, id);

            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(TEntity entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await dbCollection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var filter = filterBuilder.Eq(entity => entity.Id, entity.Id);

            await dbCollection.ReplaceOneAsync(filter, entity);
        }

        public async Task RemoveAsync(Guid id)
        {
            var filter = filterBuilder.Eq(entity => entity.Id, id);

            await dbCollection.DeleteOneAsync(filter);
        }
    }
}
