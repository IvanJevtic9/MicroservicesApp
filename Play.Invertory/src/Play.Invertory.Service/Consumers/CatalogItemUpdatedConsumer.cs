using MassTransit;
using Play.Catalog.Contract;
using Play.Common;
using Play.Inventory.Service.Entities;
using System.Threading.Tasks;

namespace Play.Invertory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> repository;
        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }
        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(message.ItemId);
            if(item == null)
            {
                /*Item doesn't exist in database, in that case we are adding it.*/
                var catalogItem = new CatalogItem()
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };

                await repository.CreateAsync(catalogItem);
                return;
            }

            item.Id = message.ItemId;
            item.Name = message.Name;
            item.Description = message.Description;

            await repository.UpdateAsync(item);
        }
    }
}
