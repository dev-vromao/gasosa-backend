using gasosa_backend.Models;
using gasosa_backend.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace gasosa_backend.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<Usuario> _signInManager;

        public AccountController(UserManager<Usuario> userManager, ITokenService tokenService, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);

            if (user == null) return Unauthorized("Email inválido!");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Email ou senha incorretos");

            return Ok(new NewUserDto
            {
                Nome = user.Nome,
                Email = user.Email,
                Token = _tokenService.CreateToken(user)
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Normaliza CPF (apenas dígitos)
            var cpfNumeros = new string(registerDto.CPF?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());

            // Garante apenas que o CPF tenha 11 dígitos numéricos
            if (cpfNumeros.Length != 11)
            {
                return BadRequest("CPF deve conter exatamente 11 dígitos");
            }

            var usuario = new Usuario
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Nome = registerDto.Nome,
                CPF = cpfNumeros,
                DataNascimento = registerDto.DataNascimento
            };

            var createdUser = await _userManager.CreateAsync(usuario, registerDto.Password);

            if (createdUser.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, "User");
                return Ok(new NewUserDto
                {
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Token = _tokenService.CreateToken(usuario)
                });
            }
            else
            {
                return StatusCode(500, createdUser.Errors);
            }
        }

    }
}
