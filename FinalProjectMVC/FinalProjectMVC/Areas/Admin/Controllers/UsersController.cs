using FinalProjectMVC.Helpers;
using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string? search, string? role, int page = 1)
        {
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Roles = await _userService.GetRolesAsync();

            var users = await _userService.GetAllUsersAsync(search, role);
            return View(users.Paginate(page, ViewData));
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: Admin/Users/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _userService.GetRolesAsync();
            // Customer is the usual choice, so start with it ticked
            return View(new CreateUserVM { Roles = new List<string> { "Customer" } });
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM model)
        {
            RequireRole(model.Roles);

            if (ModelState.IsValid)
            {
                var result = await _userService.CreateUserAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"User '{result.Data?.UserName ?? model.UserName}' created.";
                    return RedirectToAction(nameof(Index));
                }

                // e.g. "Email already exists" or the password rules
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create user.");
            }

            ViewBag.Roles = await _userService.GetRolesAsync();
            return View(model);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new UpdateUserVM
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsActive = user.IsActive,
                Roles = user.Roles
            };

            await LoadEditViewBagAsync(user);
            return View(model);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateUserVM model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            RequireRole(model.Roles);

            if (ModelState.IsValid)
            {
                var result = await _userService.UpdateUserAsync(id, model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "User updated.";
                    return RedirectToAction(nameof(Index));
                }

                // e.g. "You cannot remove your own admin role."
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update user.");
            }

            // The page needs the username/email banner again, not just the roles
            await LoadEditViewBagAsync(await _userService.GetUserByIdAsync(id));
            return View(model);
        }

        // GET: Admin/Users/ChangePassword/5
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UserName = user.UserName;
            return View(new ChangePasswordVM { UserId = id });
        }

        // POST: Admin/Users/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string id, ChangePasswordVM model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _userService.ChangePasswordAsync(id, model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Password changed.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to change password.");
            }

            var user = await _userService.GetUserByIdAsync(id);
            ViewBag.UserName = user?.UserName;
            return View(model);
        }

        // POST: Admin/Users/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var result = await _userService.ToggleUserStatusAsync(id);

            if (result.Success)
                TempData["SuccessMessage"] = "User status updated.";
            else
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update user status.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var result = await _userService.DeleteUserAsync(id);

            if (result.Success)
                TempData["SuccessMessage"] = "User deleted.";
            else
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete user.";

            return RedirectToAction(nameof(Index));
        }

        // [Required] on a List passes for an empty list, so check it here
        private void RequireRole(List<string>? roles)
        {
            if (roles == null || roles.Count == 0)
            {
                ModelState.AddModelError("Roles", "Please choose at least one role.");
            }
        }

        private async Task LoadEditViewBagAsync(UserVM? user)
        {
            ViewBag.UserName = user?.UserName;
            ViewBag.Email = user?.Email;
            ViewBag.Roles = await _userService.GetRolesAsync();
        }
    }
}
