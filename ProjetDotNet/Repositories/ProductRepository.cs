using Microsoft.EntityFrameworkCore;

namespace ProjetDotNet.Repositories
{
    public interface IProductRepository
    {
        Task AddProduct(Product product);
        Task DeleteProduct(Product product);
        Task<Product?> GetProductById(int id);
        Task<IEnumerable<Product>> GetProducts();
        Task UpdateProduct(Product product);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<Product?> GetProductById(int id) => await _context.Products
                                                                            .Include(a => a.Category)
                                                                            .Include(a => a.Stock)
                                                                            .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _context.Products
                .Include(a => a.Category)
                .Include(a => a.Stock)
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}
