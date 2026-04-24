using gasosa_backend.Models;
using gasosa_backend.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;

namespace gasosa_backend.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            UserManager<Usuario> userManager,
            ITokenService tokenService,
            SignInManager<Usuario> signInManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _environment = environment;
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
                Email = user.Email ?? string.Empty,
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

            var cpfNumeros = new string(registerDto.CPF?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());

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
                    Email = usuario.Email ?? string.Empty,
                    Token = _tokenService.CreateToken(usuario)
                });
            }
            else
            {
                return StatusCode(500, createdUser.Errors);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (user == null)
            {
                return Ok(new
                {
                    message = "Se o email estiver cadastrado, você receberá instruções para redefinir a senha."
                });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return Ok(new
            {
                message = "Token gerado com sucesso",
                token = token,
                email = user.Email
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return BadRequest("Token inválido ou usuário não encontrado.");
            }

            var resultado = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);


            if (!resultado.Succeeded)
            {
                return BadRequest(resultado.Errors.Select(error => error.Description));
            }

            return Ok(new { message = "Senha redefinida com sucesso." });
        }

    }
}
