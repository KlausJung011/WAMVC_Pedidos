using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WAMVCPedidos.Models
{
    public class OrderItemModel
    {
        public int Id { get; set; }

        [Display(Name = "Orden")]
        [Required(ErrorMessage = "El ID de la orden es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de pedido debe ser mayor a 0")]
        public int IdOrder { get; set; }

        [Display(Name = "Nombre del Producto")]
        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de producto debe ser mayor a 0")]
        public int IdProduct { get; set; }

        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "La cantidad es obligatorio")]
        [Range(1, 9999999, ErrorMessage = "La cantidad tiene que ser mayor a 0")]
        public int Cantidad { get; set; }

        [Display(Name = "Subtotal del producto")]
        [Required(ErrorMessage = "El subtotal es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "El subtotal debe estar entre 0.01 y 99,999,999.99")]
        [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Máximo 8 dígitos antes y 2 después del punto decimal")]
        public decimal Subtotal { get; set; }

        public OrderModel? Order { get; set; }
        public ProductModel? Product { get; set; }
    }
}
