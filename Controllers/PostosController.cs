using gasosa_backend.Dtos.Postos;
using gasosa_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace gasosa_backend.Controllers
{
    [Route("api/postos")]
    [ApiController]
    [Authorize]
    public class PostosController : ControllerBase
    {
        private readonly DataContext _context;

        public PostosController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMeusPostos()
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized("Token inválido.");

            var postos = await _context.Postos
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new PostoDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Bandeira = p.Bandeira,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude
                })
                .ToListAsync();

            return Ok(postos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPostoById([FromRoute] int id)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized("Token inválido.");

            var posto = await _context.Postos
                .Where(p => p.Id == id && p.UsuarioId == usuarioId)
                .Select(p => new PostoDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Bandeira = p.Bandeira,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude
                })
                .FirstOrDefaultAsync();

            if (posto == null) return NotFound("Posto não encontrado.");

            return Ok(posto);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePosto([FromBody] CreatePostoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!CoordenadasValidas(dto.Latitude, dto.Longitude))
                return BadRequest("Latitude/Longitude inválidas.");

            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized("Token inválido.");

            var posto = new Posto
            {
                Nome = dto.Nome,
                Bandeira = dto.Bandeira,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                UsuarioId = usuarioId
            };

            _context.Postos.Add(posto);
            await _context.SaveChangesAsync();

            var response = new PostoDto
            {
                Id = posto.Id,
                Nome = posto.Nome,
                Bandeira = posto.Bandeira,
                Latitude = posto.Latitude,
                Longitude = posto.Longitude
            };

            return CreatedAtAction(nameof(GetPostoById), new { id = posto.Id }, response);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePosto([FromRoute] int id)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized("Token inválido.");

            var posto = await _context.Postos.FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);
            if (posto == null) return NotFound("Posto não encontrado.");

            _context.Postos.Remove(posto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? GetUsuarioId()
        {
            return User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static bool CoordenadasValidas(decimal latitude, decimal longitude)
        {
            return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }
    }
}