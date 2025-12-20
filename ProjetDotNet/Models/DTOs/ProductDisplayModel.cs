using ProjetDotNet.Models;
using System.Collections.Generic;

namespace ProjetDotNet.Models.DTOs
{
    public class ProductDisplayModel
    {
        public IEnumerable<Product> Products { get; set; }  // MUST be IEnumerable<Product>
        public IEnumerable<Category> Categories { get; set; }
        public string STerm { get; set; }
        public int CategoryId { get; set; }
    }
}
