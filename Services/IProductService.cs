using InMemoryCaching.Models;

namespace InMemoryCaching.Services
{
	public interface IProductService
	{
		Task<Product>  GetById(Guid ID);
		Task<IEnumerable<Product>> GetAll();	
		Task Add(ProductDTO product);

	}
}
