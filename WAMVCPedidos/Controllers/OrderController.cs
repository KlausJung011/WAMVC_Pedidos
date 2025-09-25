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
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly OrderService _svc;

        public OrderController(AppDbContext context, OrderService svc)
        {
            _context = context;
            _svc = svc;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Orders.Include(o => o.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderModel = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderModel == null)
            {
                return NotFound();
            }

            // Para el formulario de agregar item
            ViewBag.Products = new SelectList(await _context.Products.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(), "Id", "Nombre");
            return View(orderModel);
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Nombre");
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUser,Fecha")] OrderModel orderModel)
        {
            if (!ModelState.IsValid)
            {
                ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Nombre", orderModel.IdUser);
                return View(orderModel);
            }

            var (ok, error, order) = await _svc.CreateOrderAsync(orderModel.IdUser, orderModel.Fecha);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error!);
                ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Nombre", orderModel.IdUser);
                return View(orderModel);
            }

            return RedirectToAction(nameof(Details), new { id = order!.Id });
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderModel = await _context.Orders.FindAsync(id);
            if (orderModel == null)
            {
                return NotFound();
            }
            ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Nombre", orderModel.IdUser);
            return View(orderModel);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdUser,Fecha")] OrderModel orderModel)
        {
            if (id != orderModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Nombre", orderModel.IdUser);
                return View(orderModel);
            }

            try
            {
                var existing = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
                if (existing == null) return NotFound();

                existing.IdUser = orderModel.IdUser;
                existing.Fecha = orderModel.Fecha;
                // Estado y Total se gestionan por lógica, no desde el form
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == orderModel.Id))
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

        // GET: Order/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderModel = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderModel == null)
            {
                return NotFound();
            }

            return View(orderModel);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (ok, error) = await _svc.DeleteOrderAsync(id);
            if (!ok) TempData["Error"] = error;
            return RedirectToAction(nameof(Index));
        }

        // POST: Order/SetEstado
        [HttpPost]
        [Authorize(Roles = "admin,empleado")]
        public async Task<IActionResult> SetEstado(int id, string estado)
        {
            var (ok, error) = await _svc.SetEstadoAsync(id, estado);
            if (!ok) TempData["Error"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}