using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WAMVCPedidos.Models;

namespace WAMVCPedidos.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = services.GetRequiredService<UserManager<UserModel>>();

            string[] roles = { "admin", "cliente", "empleado" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            // Usuario admin inicial (opcional)
            var adminEmail = "nico@gmail.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new UserModel
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Nombre = "Nicolás Vargas"
                };
                var result = await userManager.CreateAsync(admin, "123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}