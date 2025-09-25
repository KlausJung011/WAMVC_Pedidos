using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WAMVCPedidos.Data;
using WAMVCPedidos.Models;
using WAMVCPedidos.Models.ViewModels;

namespace WAMVCPedidos.Controllers
{
    [Authorize(Roles = "admin,empleado")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index([FromQuery] ProductFilter filter)
        {
            // Normaliza PageNumber/PageSize
            if (filter.PageSize <= 0) filter.PageSize = 10;
            if (filter.PageSize > 200) filter.PageSize = 200;
            if (filter.PageNumber <= 0) filter.PageNumber = 1;

            var query = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                query = query.Where(p => p.Nombre.Contains(term) || p.Descripcion.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                var cat = filter.Category.Trim();
                query = query.Where(p => p.Categoria == cat);
            }

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Precio >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Precio <= filter.MaxPrice.Value);

            var total = await query.CountAsync();

            // Ajusta PageNumber si se va de rango
            var totalPages = (int)Math.Ceiling((double)Math.Max(total, 1) / filter.PageSize);
            if (filter.PageNumber > totalPages) filter.PageNumber = totalPages == 0 ? 1 : totalPages;

            var items = await query
                .OrderBy(p => p.Nombre)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var categories = await _context.Products
                .AsNoTracking()
                .Select(p => p.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var vm = new ProductIndexViewModel
            {
                Items = items,
                TotalCount = total,
                Filter = filter,
                Categories = categories.Select(c => new SelectListItem { Value = c, Text = c })
            };

            return View(vm);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var productModel = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (productModel == null) return NotFound();

            return View(productModel);
        }

        // GET: Product/Create
        public IActionResult Create() => View();

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Precio,Stock,Categoria")] ProductModel productModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productModel);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var productModel = await _context.Products.FindAsync(id);
            if (productModel == null) return NotFound();
            return View(productModel);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio,Stock,Categoria")] ProductModel productModel)
        {
            if (id != productModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == productModel.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productModel);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var productModel = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (productModel == null) return NotFound();

            return View(productModel);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productModel = await _context.Products.FindAsync(id);
            if (productModel != null)
                _context.Products.Remove(productModel);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}