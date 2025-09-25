using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WAMVCPedidos.Models;

namespace WAMVCPedidos.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminUsersController(UserManager<UserModel> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var list = new List<UserListVm>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserListVm { Id = u.Id, UserName = u.UserName!, Nombre = u.Nombre, Email = u.Email!, Roles = roles.ToList() });
            }
            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = new[] { "admin", "empleado", "cliente" };
            return View(new CreateUserVm());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserVm vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new[] { "admin", "empleado", "cliente" };
                return View(vm);
            }

            var user = new UserModel
            {
                // Regla: UserName = Email
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,
                Nombre = vm.Nombre
            };

            var create = await _userManager.CreateAsync(user, vm.Password);
            if (!create.Succeeded)
            {
                foreach (var e in create.Errors) ModelState.AddModelError(string.Empty, e.Description);
                ViewBag.Roles = new[] { "admin", "empleado", "cliente" };
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.Rol))
            {
                if (!await _roleManager.RoleExistsAsync(vm.Rol))
                    await _roleManager.CreateAsync(new IdentityRole<int>(vm.Rol));
                await _userManager.AddToRoleAsync(user, vm.Rol);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var u = await _userManager.FindByIdAsync(id.ToString());
            if (u == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(u);
            var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

            var vm = new EditUserVm
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email!,
                Roles = roles.ToList(),
                AllRoles = allRoles
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserVm vm)
        {
            var u = await _userManager.FindByIdAsync(vm.Id.ToString());
            if (u == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
                return View(vm);
            }

            // Mantener la regla: UserName = Email
            u.Email = vm.Email;
            u.UserName = vm.Email;
            u.Nombre = vm.Nombre;

            var update = await _userManager.UpdateAsync(u);
            if (!update.Succeeded)
            {
                foreach (var e in update.Errors) ModelState.AddModelError(string.Empty, e.Description);
                vm.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
                return View(vm);
            }

            var currentRoles = await _userManager.GetRolesAsync(u);
            var toRemove = currentRoles.Except(vm.Roles).ToArray();
            var toAdd = vm.Roles.Except(currentRoles).ToArray();

            if (toRemove.Length > 0) await _userManager.RemoveFromRolesAsync(u, toRemove);
            if (toAdd.Length > 0)
            {
                foreach (var r in toAdd)
                    if (!await _roleManager.RoleExistsAsync(r))
                        await _roleManager.CreateAsync(new IdentityRole<int>(r));
                await _userManager.AddToRolesAsync(u, toAdd);
            }

            return RedirectToAction(nameof(Index));
        }

        public class UserListVm
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Nombre { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; } = new();
        }

        public class CreateUserVm
        {
            [Required, StringLength(100, MinimumLength = 3)]
            public string Nombre { get; set; }
            [Required, EmailAddress]
            public string Email { get; set; }
            [Required, StringLength(100, MinimumLength = 3)]
            public string Password { get; set; }
            public string? Rol { get; set; }
        }

        public class EditUserVm
        {
            public int Id { get; set; }
            [Required, StringLength(100, MinimumLength = 3)]
            public string Nombre { get; set; }
            [Required, EmailAddress]
            public string Email { get; set; }
            public List<string> Roles { get; set; } = new();
            public List<string> AllRoles { get; set; } = new();
        }
    }
}