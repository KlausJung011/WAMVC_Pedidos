using Microsoft.EntityFrameworkCore;
using WAMVCPedidos.Data;
using WAMVCPedidos.Models;

namespace WAMVCPedidos.Services
{
    public class OrderService
    {
        private readonly AppDbContext _db;

        public OrderService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(bool ok, string? error, OrderModel? order)> CreateOrderAsync(int idUser, DateTime fecha)
        {
            var order = new OrderModel
            {
                IdUser = idUser,
                Fecha = fecha.Date,
                Estado = "Pendiente",
                Total = 0m
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return (true, null, order);
        }

        public async Task<(bool ok, string? error)> AddItemAsync(int orderId, int productId, int cantidad)
        {
            if (cantidad <= 0) return (false, "La cantidad debe ser mayor a 0");

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order is null) return (false, "Pedido no encontrado");

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null) return (false, "Producto no encontrado");

            if (product.Stock < cantidad) return (false, $"Stock insuficiente para {product.Nombre}. Disponible: {product.Stock}");

            var subtotal = product.Precio * cantidad;

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var item = new OrderItemModel
                {
                    IdOrder = orderId,
                    IdProduct = productId,
                    Cantidad = cantidad,
                    Subtotal = subtotal
                };
                _db.OrderItems.Add(item);

                product.Stock -= cantidad;
                await _db.SaveChangesAsync();

                await RecalculateOrderTotalAsync(orderId);

                await tx.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Error al agregar item: {ex.Message}");
            }
        }

        public async Task<(bool ok, string? error)> UpdateItemQtyAsync(int orderItemId, int newCantidad)
        {
            if (newCantidad <= 0) return (false, "La cantidad debe ser mayor a 0");

            var item = await _db.OrderItems.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == orderItemId);
            if (item is null) return (false, "Detalle no encontrado");

            var diff = newCantidad - item.Cantidad; // si >0 necesitamos más stock
            if (diff > 0 && item.Product!.Stock < diff)
                return (false, $"Stock insuficiente. Disponible: {item.Product.Stock}");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Ajuste de stock
                if (diff > 0) item.Product!.Stock -= diff;
                if (diff < 0) item.Product!.Stock += (-diff);

                item.Cantidad = newCantidad;
                item.Subtotal = item.Product!.Precio * newCantidad;

                await _db.SaveChangesAsync();
                await RecalculateOrderTotalAsync(item.IdOrder);

                await tx.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Error al actualizar cantidad: {ex.Message}");
            }
        }

        public async Task<(bool ok, string? error)> RemoveItemAsync(int orderItemId)
        {
            var item = await _db.OrderItems.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == orderItemId);
            if (item is null) return (false, "Detalle no encontrado");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Devolver stock
                item.Product!.Stock += item.Cantidad;

                _db.OrderItems.Remove(item);
                await _db.SaveChangesAsync();

                await RecalculateOrderTotalAsync(item.IdOrder);

                await tx.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Error al eliminar item: {ex.Message}");
            }
        }

        public async Task<(bool ok, string? error)> DeleteOrderAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null) return (false, "Pedido no encontrado");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Restaurar stock de todos los items
                foreach (var it in order.OrderItems!)
                {
                    if (it.Product is not null)
                        it.Product.Stock += it.Cantidad;
                }

                _db.OrderItems.RemoveRange(order.OrderItems!);
                _db.Orders.Remove(order);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Error al eliminar el pedido: {ex.Message}");
            }
        }

        public async Task<(bool ok, string? error)> SetEstadoAsync(int orderId, string nuevoEstado)
        {
            var validos = new[] { "Pendiente", "Procesado", "Enviado", "Entregado" };
            if (!validos.Contains(nuevoEstado)) return (false, "Estado inválido");

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order is null) return (false, "Pedido no encontrado");

            order.Estado = nuevoEstado;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task RecalculateOrderTotalAsync(int orderId)
        {
            var total = await _db.OrderItems
                .Where(i => i.IdOrder == orderId)
                .SumAsync(i => (decimal?)i.Subtotal) ?? 0m;

            var order = await _db.Orders.FirstAsync(o => o.Id == orderId);
            order.Total = total;
            await _db.SaveChangesAsync();
        }
    }
}