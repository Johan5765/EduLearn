using Aula.Data;
using Aula.Models;
using Aula.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Aula.Controllers
{
    [Authorize]
    public class UsuarioController(EduLearnDbContext _context,IPasswordHasher<Usuario> _passwordHasher) : Controller
    {
        public IActionResult Index()
        {
            var listUsuario = _context.Usuarios.Select(x => new UsuarioListVM()
            {
                UsuarioId = x.UsuarioId,
                Nombre = x.Nombre,
                Correo = x.Correo,
                Activo = x.Activo,
                NombreRol = x.Rol.Nombre,
            }).ToList();

            return View(listUsuario);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new UsuarioCreateVM();

            await CargarRolesAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(vm);
                return View(vm);
            }

            var usuario = new Usuario
            {
                Nombre = vm.Nombre,
                Correo = vm.Correo,
                RolId = vm.RolId,
                Activo = vm.Activo,
                PasswordHash = _passwordHasher.HashPassword(
                    new Usuario(), vm.Password)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            var vm = new UsuarioEditVM
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                RolId = usuario.RolId,
                Activo = usuario.Activo
            };

            await CargarRolesAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioEditVM vm)
        {
            if (id != vm.UsuarioId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(vm);
                return View(vm);
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            usuario.Nombre = vm.Nombre;
            usuario.Correo = vm.Correo;
            usuario.RolId = vm.RolId;
            usuario.Activo = vm.Activo;

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                usuario.PasswordHash = _passwordHasher.HashPassword(
                    usuario, vm.Password);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        [HttpPost]
        private async Task CargarRolesAsync(UsuarioCreateVM vm)
        {
            vm.Roles = await _context.Roles
                .Select(x => new SelectListItem
                {
                    Value = x.RolId.ToString(),
                    Text = x.Nombre
                })
                .ToListAsync();
        }
        private async Task CargarRolesAsync(UsuarioEditVM vm)
        {
            vm.Roles = await _context.Roles
                .Select(x => new SelectListItem
                {
                    Value = x.RolId.ToString(),
                    Text = x.Nombre
                })
                .ToListAsync();
        }
    }




}
