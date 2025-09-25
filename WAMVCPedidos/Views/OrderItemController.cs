using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WAMVCPedidos.Data;
using WAMVCPedidos.Models;

namespace WAMVCPedidos.Views
{
    public class OrderItemController : Controller
    {
        private readonly AppDbContext _context;

        public OrderItemController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrderItem
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OrderItems.Include(o => o.Order).Include(o => o.Product);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OrderItem/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItemModel = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItemModel == null)
            {
                return NotFound();
            }

            return View(orderItemModel);
        }

        // GET: OrderItem/Create
        public IActionResult Create()
        {
            ViewData["IdOrder"] = new SelectList(_context.Orders, "Id", "Estado");
            ViewData["IdProduct"] = new SelectList(_context.Products, "Id", "Categoria");
            return View();
        }

        // POST: OrderItem/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdOrder,IdProduct,Cantidad,Subtotal")] OrderItemModel orderItemModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderItemModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdOrder"] = new SelectList(_context.Orders, "Id", "Estado", orderItemModel.IdOrder);
            ViewData["IdProduct"] = new SelectList(_context.Products, "Id", "Categoria", orderItemModel.IdProduct);
            return View(orderItemModel);
        }

        // GET: OrderItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItemModel = await _context.OrderItems.FindAsync(id);
            if (orderItemModel == null)
            {
                return NotFound();
            }
            ViewData["IdOrder"] = new SelectList(_context.Orders, "Id", "Estado", orderItemModel.IdOrder);
            ViewData["IdProduct"] = new SelectList(_context.Products, "Id", "Categoria", orderItemModel.IdProduct);
            return View(orderItemModel);
        }

        // POST: OrderItem/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdOrder,IdProduct,Cantidad,Subtotal")] OrderItemModel orderItemModel)
        {
            if (id != orderItemModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderItemModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemModelExists(orderItemModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdOrder"] = new SelectList(_context.Orders, "Id", "Estado", orderItemModel.IdOrder);
            ViewData["IdProduct"] = new SelectList(_context.Products, "Id", "Categoria", orderItemModel.IdProduct);
            return View(orderItemModel);
        }

        // GET: OrderItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItemModel = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItemModel == null)
            {
                return NotFound();
            }

            return View(orderItemModel);
        }

        // POST: OrderItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItemModel = await _context.OrderItems.FindAsync(id);
            if (orderItemModel != null)
            {
                _context.OrderItems.Remove(orderItemModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemModelExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }
}
