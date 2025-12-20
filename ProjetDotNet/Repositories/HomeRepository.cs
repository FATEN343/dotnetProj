using Microsoft.EntityFrameworkCore;
using ProjetDotNet.Models;

namespace ProjetDotNet.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _db;

        public HomeRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Category>> Categories()
        {
            return await _db.Categories.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProducts(string sTerm = "", int categoryId = 0)
        {
            var productQuery = _db.Products
               .AsNoTracking()
               .Include(x => x.Category)
               // .Include(x => x.Stock) // Commented out: Stock table doesn't exist yet
               .AsQueryable();

            if (!string.IsNullOrWhiteSpace(sTerm))
            {
                productQuery = productQuery.Where(p => p.ProductName.StartsWith(sTerm.ToLower()));
            }

            if (categoryId > 0)
            {
                productQuery = productQuery.Where(p => p.CategoryId == categoryId);
            }

            var products = await productQuery
                .AsNoTracking()
                .Select(product => new Product
                {
                    Id = product.Id,
                    Image = product.Image,
                    ProductName = product.ProductName,
                    CategoryId = product.CategoryId,
                    Price = product.Price,
                    CategoryName = product.Category.CategoryName,
                    // Quantity = product.Stock == null ? 0 : product.Stock.Quantity // Commented out
                }).ToListAsync();

            return products;
        }
    }
}
