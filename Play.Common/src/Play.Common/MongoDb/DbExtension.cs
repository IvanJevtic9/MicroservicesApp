using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Common.MongoDb
{
    public static class DbExtension
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();

                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

                return new MongoClient(mongoDbSettings.ConnectionString).GetDatabase(serviceSettings.Name);
            });

            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            return services;
        }

        public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services, string collectionName) where TEntity : IEntity, new()
        {
            services.AddSingleton<IRepository<TEntity>>(serviceProvider =>
            {
                var db = serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<TEntity>(db, collectionName);
            });

            return services;
        }
    }
}
