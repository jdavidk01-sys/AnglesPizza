using AngelesPizza.Models;
using Microsoft.AspNetCore.Identity;

namespace AngelesPizza.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                "Administrador",
                "Operador"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            const string username = "admin";
            const string password = "Admin123";
            const string fullName = "Administrador";

            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = username,
                    FullName = fullName,
                    Email = "admin@angelespizza.com",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrador");
                }
            }
        }
    }
}