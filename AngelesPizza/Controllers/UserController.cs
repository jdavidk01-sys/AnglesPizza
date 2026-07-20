using AngelesPizza.Models;
using AngelesPizza.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AngelesPizza.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();

            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName!,
                    Role = roles.FirstOrDefault() ?? "",
                    IsActive = user.IsActive
                });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name
                })
                .ToList();

            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name!,
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            var exists = await _userManager.FindByNameAsync(model.UserName);

            if (exists != null)
            {
                ModelState.AddModelError("UserName", "El usuario ya existe.");

                ViewBag.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name!,
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = $"{model.UserName}@angelespizza.local",
                EmailConfirmed = true,
                IsActive = model.IsActive
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ViewBag.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name!,
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName!,
                Role = role ?? "",
                IsActive = user.IsActive
            };

            ViewBag.Roles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name!,
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            // Validar que el nuevo nombre de usuario no pertenezca a otro usuario
            var existingUser = await _userManager.FindByNameAsync(model.UserName);

            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("UserName", "El nombre de usuario ya está en uso.");

                ViewBag.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name!,
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.Email = $"{model.UserName}@angelespizza.local";
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ViewBag.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name!,
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            // Actualizar rol
            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var model = new ChangePasswordViewModel
            {
                Id = user.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            // Elimina la contraseña actual
            var removeResult = await _userManager.RemovePasswordAsync(user);

            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            // Asigna la nueva contraseña
            var addResult = await _userManager.AddPasswordAsync(user, model.Password);

            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return Json(new
                {
                    ok = false
                });
            }

            if (user.UserName == User.Identity!.Name)
            {
                return Json(new
                {
                    ok = false,
                    message = "No puede desactivar su propio usuario."
                });
            }

            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);

            return Json(new
            {
                ok = result.Succeeded
            });
        }
    }
}