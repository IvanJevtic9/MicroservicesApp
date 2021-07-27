using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;
using Play.Invertory.Service.Clients;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> itemsRepository;
        private readonly IRepository<CatalogItem> catalogRepository;
        //private readonly CatalogClient client;

        public ItemsController(IRepository<InventoryItem> itemsRepository, IRepository<CatalogItem> catalogRepository/*, CatalogClient client*/)
        {
            this.itemsRepository = itemsRepository;
            this.catalogRepository = catalogRepository;
            //this.client = client;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            //var catalogItems = await client.GetCatalogItemsAsync(); - old fashion way
            var invItems = await itemsRepository.GetAllAsync(item => item.UserId == userId);
            var itemIds = invItems.Select(ii => ii.CatalogItemId);
            var catalogItems = await catalogRepository.GetAllAsync(x => itemIds.Contains(x.Id));

            var inventoryItemDtos = invItems.Select(item =>
            {
                var catalogitem = catalogItems.Single(catalogitem => catalogitem.Id == item.CatalogItemId);

                return item.AsDto(catalogitem.Name, catalogitem.Description);
            });

            return Ok(inventoryItemDtos);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await itemsRepository.GetAsync(
                item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await itemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await itemsRepository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
    }
}