using Microsoft.EntityFrameworkCore;
using ProjetDotNet.Data;
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

        public async Task<IEnumerable<Product>> GetProducts(string sTerm = "", int CategoryId = 0)
        {
            // Build query
            var productQuery = _db.Products
               .AsNoTracking()
               .Include(x => x.Category)
               .Include(x => x.Stock)
               .AsQueryable();

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(sTerm))
            {
                var term = sTerm.ToLower();
                productQuery = productQuery.Where(p => p.ProductName.ToLower().StartsWith(term));
            }

            // Filter by category
            if (CategoryId > 0)
            {
                productQuery = productQuery.Where(p => p.CategoryId == CategoryId);
            }

            // Select Product with required fields
            var products = await productQuery
                .Select(p => new Product
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    BrandName = p.BrandName,
                    Price = p.Price,
                    Image = p.Image,
                    CategoryId = p.CategoryId,
                    Category = p.Category,
                    Quantity = p.Stock == null ? 0 : p.Stock.Quantity
                })
                .ToListAsync();

            return products;
        }
    }
}
