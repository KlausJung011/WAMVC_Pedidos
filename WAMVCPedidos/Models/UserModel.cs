using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WAMVCPedidos.Models
{
    public class UserModel: IdentityUser<int>
    {
        [Display(Name = "Nombre del usuario")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        //[Remote(action: "CheckNombreUnico", controller: "Clientes", AdditionalFields = nameof(Id), ErrorMessage = "Ya existe un cliente con ese nombre.")]
        public string Nombre { get; set; }

        // IdentityUser ya maneja el id, email y password

        /*public int Id { get; set; }
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo es obligatorio")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "El email debe tener entre 7 y 100 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
        [RegularExpression(@"^[\w\.-]+@[\w\.-]+\.\w{2,4}$", ErrorMessage = "El correo no cumple el formato requerido.")]
        public string Email { get; set; }

        [Display(Name = "Contraseña del usuario")]
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 20 caracteres")]
        public string Password { get; set; }*/

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Display(Name = "Rol")]
        [StringLength(9, ErrorMessage = "El rol no puede exceder los 9 caracteres")]
        [RegularExpression(@"^(admin|cliente|empleado)$", ErrorMessage = "El rol debe ser admin, cliente o empleado")]
        public string Rol { get; set; }

        //Un cliente puede tener varias órdenes
        public ICollection<OrderModel>? Orders { get; set; }
    }
}
