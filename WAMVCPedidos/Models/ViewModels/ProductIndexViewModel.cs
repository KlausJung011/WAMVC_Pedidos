using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WAMVCPedidos.Models;

namespace WAMVCPedidos.Models.ViewModels
{
    public class ProductFilter
    {
        public string? Search { get; set; }
        public string? Category { get; set; }

        [Range(0, 99999999.99)]
        public decimal? MinPrice { get; set; }

        [Range(0, 99999999.99)]
        public decimal? MaxPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 10;
    }

    public class ProductIndexViewModel
    {
        public IEnumerable<ProductModel> Items { get; set; } = Enumerable.Empty<ProductModel>();
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
        public ProductFilter Filter { get; set; } = new();

        public int TotalCount { get; set; }
        public int TotalPages => Filter.PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / Filter.PageSize) : 1;
    }
}
