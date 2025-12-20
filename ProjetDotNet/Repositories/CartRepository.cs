using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetDotNet.Models;

namespace ProjetDotNet.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddItem(int productId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in");

                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();

                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.ProductId == productId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var product = _db.Products.Find(productId);
                    cartItem = new CartDetail
                    {
                        ProductId = productId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = product.Price
                    };
                    _db.CartDetails.Add(cartItem);
                }

                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<int> RemoveItem(int productId)
        {
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in");

                var cart = await GetCart(userId);
                if (cart is null)
                    throw new InvalidOperationException("Invalid cart");

                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.ProductId == productId);

                if (cartItem is null)
                    throw new InvalidOperationException("No items in cart");
                else if (cartItem.Quantity == 1)
                    _db.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity -= 1;

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new InvalidOperationException("Invalid user ID");

            var shoppingCart = await _db.ShoppingCarts
                .Include(a => a.CartDetails)
                .ThenInclude(a => a.Product)
                //.ThenInclude(a => a.Stock)  // commented temporarily
                .Include(a => a.CartDetails)
                .ThenInclude(a => a.Product)
                //.ThenInclude(a => a.Category) // keep if Category exists
                .Where(a => a.UserId == userId)
                .FirstOrDefaultAsync();

            return shoppingCart;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            return await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }

            var data = await (from cart in _db.ShoppingCarts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId == userId
                              select new { cartDetail.Id })
                        .ToListAsync();

            return data.Count;
        }

        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in");

                var cart = await GetCart(userId);
                if (cart is null)
                    throw new InvalidOperationException("Invalid cart");

                var cartDetails = _db.CartDetails
                                     .Where(a => a.ShoppingCartId == cart.Id)
                                     .ToList();

                if (cartDetails.Count == 0)
                    throw new InvalidOperationException("Cart is empty");

                var pendingRecord = _db.OrderStatuses.FirstOrDefault(s => s.StatusName == "Pending");
                if (pendingRecord is null)
                    throw new InvalidOperationException("Order status does not have Pending status");

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid = false,
                    OrderStatusId = pendingRecord.Id
                };
                _db.Orders.Add(order);
                _db.SaveChanges();

                foreach (var item in cartDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _db.OrderDetails.Add(orderDetail);

                    // update stock here (commented temporarily)
                    /*
                    var stock = await _db.Stocks.FirstOrDefaultAsync(a => a.ProductId == item.ProductId);
                    if (stock == null)
                        throw new InvalidOperationException("Stock is null");

                    if (item.Quantity > stock.Quantity)
                        throw new InvalidOperationException($"Only {stock.Quantity} item(s) are available in stock");

                    stock.Quantity -= item.Quantity;
                    */
                }

                _db.CartDetails.RemoveRange(cartDetails);
                _db.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            return _userManager.GetUserId(principal);
        }
    }
}
