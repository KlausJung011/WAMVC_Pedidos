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
            var adminPassword = "Admin123!";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin is null)
            {
                admin = new UserModel
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Nombre = "Nico Vargas"
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
            else
            {
                // Ajustar si se creó con UserName distinto
                if (admin.UserName != adminEmail)
                {
                    admin.UserName = adminEmail;
                    await userManager.UpdateAsync(admin);
                }
                if (!await userManager.IsInRoleAsync(admin, "admin"))
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}