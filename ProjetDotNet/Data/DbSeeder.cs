using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetDotNet.Constants;
using ProjetDotNet.Data;
using ProjetDotNet.Models;

namespace ProjetDotNet.Data
{
    public class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider service)
        {
            try
            {
                var context = service.GetRequiredService<ApplicationDbContext>();

                // Apply migrations
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }

                var userMgr = service.GetRequiredService<UserManager<IdentityUser>>();
                var roleMgr = service.GetRequiredService<RoleManager<IdentityRole>>();

                // -------- Roles --------
                if (!await roleMgr.RoleExistsAsync(Roles.Admin.ToString()))
                    await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));

                if (!await roleMgr.RoleExistsAsync(Roles.User.ToString()))
                    await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));

                // -------- Admin User --------
                var adminEmail = "admin@gmail.com";
                var admin = await userMgr.FindByEmailAsync(adminEmail);

                if (admin == null)
                {
                    admin = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    await userMgr.CreateAsync(admin, "Admin@123");
                    await userMgr.AddToRoleAsync(admin, Roles.Admin.ToString());
                }

                // -------- Categories --------
                if (!context.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { CategoryName = "Cleanser" },
                        new Category { CategoryName = "Moisturizer" },
                        new Category { CategoryName = "Serum" },
                        new Category { CategoryName = "Sunscreen" },
                        new Category { CategoryName = "Mask" }
                    };

                    await context.Categories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                }

                // -------- Products --------
                if (!context.Products.Any())
                {
                    var products = new List<Product>
                    {
                        new Product { ProductName = "Gentle Cleanser", Price = 120, CategoryId = 1 },
                        new Product { ProductName = "Hydrating Moisturizer", Price = 180, CategoryId = 2 },
                        new Product {ProductName = "Vitamin C Serum", Price = 250, CategoryId = 3 },
                        new Product { ProductName = "SPF 50 Sunscreen", Price = 200, CategoryId = 4 },
                        new Product { ProductName = "Clay Face Mask", Price = 150, CategoryId = 5 }
                    };

                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                }

                // -------- Order Status --------
                if (!context.orderStatuses.Any())
                {
                    var statuses = new List<OrderStatus>
                    {
                        new OrderStatus { StatusId = 1, StatusName = "Pending" },
                        new OrderStatus { StatusId = 2, StatusName = "Shipped" },
                        new OrderStatus { StatusId = 3, StatusName = "Delivered" },
                        new OrderStatus { StatusId = 4, StatusName = "Cancelled" }
                    };

                    await context.orderStatuses.AddRangeAsync(statuses);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
