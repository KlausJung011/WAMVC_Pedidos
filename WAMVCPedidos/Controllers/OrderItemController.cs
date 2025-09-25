using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WAMVCPedidos.Data;
using WAMVCPedidos.Models;
using WAMVCPedidos.Services;

namespace WAMVCPedidos.Controllers
{
    [Authorize(Roles = "admin,empleado")]
    public class OrderItemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly OrderService _svc;

        public OrderItemController(AppDbContext context, OrderService svc)
        {
            _context = context;
            _svc = svc;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOrder,IdProduct,Cantidad")] OrderItemModel orderItemModel)
        {
            // Validar con el servicio (calcula Subtotal y ajusta stock)
            var (ok, error) = await _svc.AddItemAsync(orderItemModel.IdOrder, orderItemModel.IdProduct, orderItemModel.Cantidad);
            if (!ok)
            {
                TempData["Error"] = error;
                ViewData["IdOrder"] = new SelectList(_context.Orders, "Id", "Estado", orderItemModel.IdOrder);
                ViewData["IdProduct"] = new SelectList(_context.Products, "Id", "Categoria", orderItemModel.IdProduct);
                return View(orderItemModel);
            }

            // Redirige al detalle del pedido para continuar trabajando
            return RedirectToAction("Details", "Order", new { id = orderItemModel.IdOrder });
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdOrder,IdProduct,Cantidad")] OrderItemModel orderItemModel)
        {
            if (id != orderItemModel.Id)
            {
                return NotFound();
            }

            // Solo permitimos cambiar cantidad. El servicio ajusta stock y subtotal.
            var (ok, error) = await _svc.UpdateItemQtyAsync(orderItemModel.Id, orderItemModel.Cantidad);
            if (!ok)
            {
                TempData["Error"] = error;
                ViewData["IdOrder"] = new SelectList(_context.Orders, "Id", "Estado", orderItemModel.IdOrder);
                ViewData["IdProduct"] = new SelectList(_context.Products, "Id", "Categoria", orderItemModel.IdProduct);
                return View(orderItemModel);
            }

            return RedirectToAction("Details", "Order", new { id = orderItemModel.IdOrder });
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
            // Obtener el pedido para redirigir
            var item = await _context.OrderItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            int? orderId = item?.IdOrder;

            var (ok, error) = await _svc.RemoveItemAsync(id);
            if (!ok) TempData["Error"] = error;

            if (orderId.HasValue)
                return RedirectToAction("Details", "Order", new { id = orderId.Value });

            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemModelExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }
}