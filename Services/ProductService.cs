using InMemoryCaching.Data;
using InMemoryCaching.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;

namespace InMemoryCaching.Services
{
	public class ProductService : IProductService
	{
		private ApplicationDbContext context;
		private IMemoryCache cache;
		private ILogger<ProductService> logger;

		public ProductService(ApplicationDbContext Context , IMemoryCache Cache , ILogger<ProductService> Logger)
		{
			context = Context;
			cache = Cache;
			logger = Logger;
		}

		public async Task Add(ProductDTO request)
		{
			var product = new Product(request.Name, request.Description, request.Price);
			await context.Products.AddAsync(product);
			await context.SaveChangesAsync();

			// invalidate cache for products, as new product is added
			var cacheKey = "products";

			logger.LogInformation("invalidating cache for key: {CacheKey} from cache.", cacheKey);
			cache.Remove(cacheKey);
		}

		public async Task<IEnumerable<Product>> GetAll()
		{
			var cacheKey = "products";

			logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
			if (!cache.TryGetValue(cacheKey, out IEnumerable<Product> products)) 
			{
				logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
				products = await context.Products.ToListAsync();

				var cacheOptions = new MemoryCacheEntryOptions()
					.SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
					.SetSlidingExpiration(TimeSpan.FromMinutes(2))
					.SetPriority(CacheItemPriority.NeverRemove)
					.SetSize(2048);

				logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
				cache.Set(cacheKey, products , cacheOptions);
			}
			else
				logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
				return products;
			
			

		}

		public async Task<Product> GetById(Guid ID)
		{
			var cacheKey = $"product{ID}";

			logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
			if (!cache.TryGetValue(cacheKey, out Product product))
			{
				logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
				product = await context.Products.FindAsync(ID);

				var cacheOptions = new MemoryCacheEntryOptions()
					.SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
					.SetSlidingExpiration(TimeSpan.FromMinutes(2))
					.SetPriority(CacheItemPriority.NeverRemove);

				logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
				cache.Set(cacheKey, product, cacheOptions);
			}
			else
				logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
				return product;
		}
	}
}
