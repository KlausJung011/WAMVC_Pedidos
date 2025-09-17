using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WAMVCPedidos.Models
{
    public class ProductModel
    {
        public int Id { get; set; }

        [Display(Name = "Nombre del producto")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        //[Remote(action: "CheckNombreUnico", controller: "Productos", AdditionalFields = nameof(Id), ErrorMessage = "Ya existe un producto con ese nombre.")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500)]
        public string Descripcion { get; set; }

        [Display(Name = "Precio")]
        [Required(ErrorMessage = "El precio es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 99,999,999.99")]
        [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "Máximo 8 dígitos antes y 2 después del punto decimal")]
        public decimal Precio { get; set; }

        [Display(Name = "Cantidad en stock")]
        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, 9999999, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Display(Name = "Categroía del producto")]
        [Required(ErrorMessage = "La categoría es obligatoria")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "La categoría debe tener entre 3 y 100 caracteres")]
        //[Remote(action: "CheckNombreUnico", controller: "Productos", AdditionalFields = nameof(Id), ErrorMessage = "Ya existe un producto con ese nombre.")]
        public string Categoria { get; set; }

        //Un producto puede estar en muchos detalles de pedido 1-N
        public ICollection<OrderItemModel>? OrderItems { get; set; }
    }
}
