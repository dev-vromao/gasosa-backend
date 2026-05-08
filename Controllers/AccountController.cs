using gasosa_backend.Models;
using gasosa_backend.Interfaces;
using gasosa_backend.Dtos.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gasosa_backend.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IEmailService _emailService;
        private readonly DataContext _context;

        public AccountController(
            UserManager<Usuario> userManager,
            ITokenService tokenService,
            SignInManager<Usuario> signInManager,
            IEmailService emailService,
            DataContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
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

            // Sempre retorna a mesma mensagem para não revelar se o email existe
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                return Ok(new { message = "Se o email estiver cadastrado, você receberá um código de verificação." });
            }

            // Invalida códigos anteriores do mesmo email
            var codigosAntigos = await _context.PasswordResetCodes
                .Where(c => c.Email == forgotPasswordDto.Email && !c.Used)
                .ToListAsync();

            foreach (var codigo in codigosAntigos)
                codigo.Used = true;

            // Gera código de 6 dígitos
            var code = new Random().Next(100000, 999999).ToString();

            _context.PasswordResetCodes.Add(new PasswordResetCode
            {
                Email = forgotPasswordDto.Email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Used = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetCodeAsync(forgotPasswordDto.Email, code);

            return Ok(new { message = "Se o email estiver cadastrado, você receberá um código de verificação." });
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resetCode = await _context.PasswordResetCodes
                .Where(c =>
                    c.Email == dto.Email &&
                    c.Code == dto.Code &&
                    !c.Used &&
                    c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetCode == null)
                return BadRequest(new { message = "Código inválido ou expirado." });

            // Marca como usado
            resetCode.Used = true;
            await _context.SaveChangesAsync();

            // Gera o token do Identity para reset de senha
            var user = await _userManager.FindByEmailAsync(dto.Email);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user!);

            return Ok(new { token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Usuário não encontrado." });
            }

            var resultado = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (!resultado.Succeeded)
            {
                return BadRequest(new { message = resultado.Errors.Select(e => e.Description) });
            }

            return Ok(new { message = "Senha redefinida com sucesso." });
        }
    }
}
