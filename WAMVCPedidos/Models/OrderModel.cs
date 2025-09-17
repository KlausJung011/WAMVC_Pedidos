using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WAMVCPedidos.Models
{
    public class OrderModel
    {
        public int Id { get; set; }

        [Display(Name = "Nombre del Cliente")]
        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int IdCliente { get; set; }

        [Display(Name = "Fecha de Pedido")]
        [Required(ErrorMessage = "La fecha de entrega es obligatoria")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(OrderModel), nameof(ValidateFechaPedido))]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Display(Name = "Estado")]
        [StringLength(11, ErrorMessage = "El estado no puede exceder los 11 caracteres")]
        [RegularExpression(@"^(Pendiente|Procesado|Enviado|Entregado)$", ErrorMessage = "El estado debe ser Pendiente, Enviado o Entregado")]
        public string Estado { get; set; } = "Pendiente";

        [Display(Name = "Precio total")]
        [Required(ErrorMessage = "El total es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "El precio total debe estar entre 0.01 y 99,999,999.99")]
        [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Máximo 8 dígitos antes y 2 después del punto decimal")]
        public decimal Total { get; set; }

        //Una orden pertenece a un solo cliente
        public UserModel? Cliente { get; set; }
        //Una orden puede tener muchos items 1-N
        public ICollection<OrderItemModel>? OrderItems{ get; set; }

        public static ValidationResult ValidateFechaPedido(DateTime fechaPedido, ValidationContext context)
        {
            if (fechaPedido.Date < DateTime.Today)
            {
                return new ValidationResult("La fecha del pedido no puede ser anterior a hoy");
            }
            return ValidationResult.Success;
        }
    }
}
