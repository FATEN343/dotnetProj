using Microsoft.EntityFrameworkCore;
using ProjetDotNet;

namespace ProjetDotNet.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get stock by product id
        public async Task<Stock?> GetStockByProductId(int productId)
            => await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId);

        // Add or update stock
        public async Task ManageStock(StockDTO stockToManage)
        {
            var existingStock = await GetStockByProductId(stockToManage.ProductId);
            if (existingStock is null)
            {
                var stock = new Stock
                {
                    ProductId = stockToManage.ProductId,
                    Quantity = stockToManage.Quantity
                };
                _context.Stocks.Add(stock);
            }
            else
            {
                existingStock.Quantity = stockToManage.Quantity;
            }
            await _context.SaveChangesAsync();
        }

        // Get all stocks (with optional search by product name)
        public async Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "")
        {
            var stocks = await (from product in _context.Products
                                join stock in _context.Stocks
                                on product.Id equals stock.ProductId
                                into product_stock
                                from productStock in product_stock.DefaultIfEmpty()
                                where string.IsNullOrWhiteSpace(sTerm)
                                      || product.ProductName.ToLower().Contains(sTerm.ToLower())
                                select new StockDisplayModel
                                {
                                    ProductId = product.Id,
                                    ProductName = product.ProductName,
                                    Quantity = productStock == null ? 0 : productStock.Quantity
                                }).ToListAsync();

            return stocks;
        }
    }

    public interface IStockRepository
    {
        Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "");
        Task<Stock?> GetStockByProductId(int productId);
        Task ManageStock(StockDTO stockToManage);
    }
}
