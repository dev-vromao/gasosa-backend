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
        private readonly IWebHostEnvironment _env;

        public PostosController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetPostos()
        {
            var postos = await _context.Postos
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
            var posto = await _context.Postos
                .Where(p => p.Id == id)
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

        [HttpGet("{id:int}/fotos")]
        public async Task<IActionResult> GetFotosDoPosto([FromRoute] int id)
        {
            var fotos = await _context.PostoFotos
                .Where(f => f.PostoId == id)
                .Select(f => f.UrlImagem)
                .ToListAsync();

            return Ok(fotos);
        }

        [HttpPost("{id:int}/fotos")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFotoPosto([FromRoute] int id, [FromForm] UploadPostoFotoDto dto)
        {
            try
            {
                var foto = dto.Foto;
                if (foto == null || foto.Length == 0)
                    return BadRequest("Nenhuma foto foi enviada.");

                var postoExiste = await _context.Postos.AnyAsync(p => p.Id == id);
                if (!postoExiste) return NotFound("Posto não encontrado.");

                var webRoot = _env.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRoot))
                {
                    webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
                }

                var uploadsFolder = Path.Combine(webRoot, "uploads", "postos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var nomeOriginal = Path.GetFileName(foto.FileName);
                var nomeUnico = Guid.NewGuid().ToString() + "_" + nomeOriginal;
                var caminhoFisicoArquivo = Path.Combine(uploadsFolder, nomeUnico);

                using (var stream = new FileStream(caminhoFisicoArquivo, FileMode.Create))
                {
                    await foto.CopyToAsync(stream);
                }

                var urlImagem = $"/uploads/postos/{nomeUnico}";

                var novaFoto = new PostoFoto
                {
                    PostoId = id,
                    UrlImagem = urlImagem
                };

                _context.PostoFotos.Add(novaFoto);
                await _context.SaveChangesAsync();

                return Ok(new { mensagem = "Foto enviada com sucesso!", url = urlImagem });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO NO UPLOAD: {ex.Message}");
                return StatusCode(500, "Erro interno ao salvar a imagem.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePosto([FromBody] CreatePostoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!CoordenadasValidas(dto.Latitude, dto.Longitude))
                return BadRequest("Latitude/Longitude inválidas.");

            var postosMesmoNome = await _context.Postos
                .Where(p => p.Nome == dto.Nome)
                .ToListAsync();

            foreach (var existente in postosMesmoNome)
            {
                var distancia = CalcularDistanciaEmMetros(
                    existente.Latitude,
                    existente.Longitude,
                    dto.Latitude,
                    dto.Longitude);

                if (distancia < 20)
                    return BadRequest("Já existe um posto com esse nome muito próximo desta localização.");
            }

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

        private static double CalcularDistanciaEmMetros(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
        {
            const double raioTerraMetros = 6371000d;

            var lat1Rad = GrausParaRadianos((double)latitude1);
            var lat2Rad = GrausParaRadianos((double)latitude2);
            var deltaLat = GrausParaRadianos((double)(latitude2 - latitude1));
            var deltaLon = GrausParaRadianos((double)(longitude2 - longitude1));

            var a = System.Math.Sin(deltaLat / 2) * System.Math.Sin(deltaLat / 2) +
                    System.Math.Cos(lat1Rad) * System.Math.Cos(lat2Rad) *
                    System.Math.Sin(deltaLon / 2) * System.Math.Sin(deltaLon / 2);

            var c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));
            return raioTerraMetros * c;
        }

        private static double GrausParaRadianos(double graus)
        {
            return graus * (System.Math.PI / 180d);
        }
    }
}