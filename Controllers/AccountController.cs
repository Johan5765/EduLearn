using Aula.Data;
using Aula.Models;
using Aula.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Aula.Controllers;

public class AccountController(EduLearnDbContext _context,IPasswordHasher<Usuario> passwordHasher) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u =>
                u.Correo == model.Correo && u.Activo);

        if (usuario == null)
        {
            ModelState.AddModelError(
                "", "Correo o contraseña incorrectos.");

            return View(model);
        }

        var resultado = passwordHasher.VerifyHashedPassword(usuario,usuario.PasswordHash,model.Password);

        if (resultado == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");

            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,usuario.UsuarioId.ToString()),

            new Claim(ClaimTypes.Name,usuario.Nombre),

            new Claim(ClaimTypes.Email,usuario.Correo),

            new Claim(ClaimTypes.Role,usuario.Rol.Nombre)
        };

        var identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }
}
