using gasosa_backend.Dtos.Avaliacao;
using gasosa_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gasosa_backend.Controllers
{
    [Route("api/avaliacoes")]
    [ApiController]
    public class AvaliacaoController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<Usuario> _userManager;

        public AvaliacaoController(DataContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CriarAvaliacao([FromBody] CreateAvaliacaoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var postoExiste = await _context.Postos.AnyAsync(p => p.Id == dto.PostoId);
            if (!postoExiste) return NotFound("O posto informado não existe.");

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized("Usuário não autenticado.");

            var novaAvaliacao = new Avaliacao
            {
                PostoId = dto.PostoId,
                UsuarioId = usuario.Id,
                NotaGeral = dto.NotaGeral,
                NotaPrecos = dto.NotaPrecos,
                NotaAtendimento = dto.NotaAtendimento,
                TemLojaConveniencia = dto.TemLojaConveniencia,
                TemCalibrador = dto.TemCalibrador,
                TemLavaRapido = dto.TemLavaRapido,
                TemTrocaOleo = dto.TemTrocaOleo,
                TemAreaDescanso = dto.TemAreaDescanso,
                TemCarregadorEletrico = dto.TemCarregadorEletrico,
                Comentario = dto.Comentario,
                DataAvaliacao = DateTime.UtcNow
            };

            _context.Avaliacoes.Add(novaAvaliacao);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Avaliação enviada com sucesso!", avaliacaoId = novaAvaliacao.Id });
        }

        [HttpGet("posto/{postoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvaliacoesDoPosto(int postoId)
        {
            var postoExiste = await _context.Postos.AnyAsync(p => p.Id == postoId);
            if (!postoExiste) return NotFound("Posto não encontrado.");

            var avaliacoes = await _context.Avaliacoes
                .Where(a => a.PostoId == postoId)
                .Select(a => new
                {
                    id = a.Id,
                    // Para o frontend, usamos a nota geral como "média" de estrelas
                    mediaEstrelas = (double)a.NotaGeral,
                    comentario = a.Comentario,
                    temLojaConveniencia = a.TemLojaConveniencia,
                    temCalibrador = a.TemCalibrador,
                    temLavaRapido = a.TemLavaRapido,
                    temTrocaOleo = a.TemTrocaOleo,
                    temAreaDescanso = a.TemAreaDescanso,
                    temCarregadorEletrico = a.TemCarregadorEletrico,
                    diasAtras = (DateTime.UtcNow - a.DataAvaliacao).Days
                })
                .OrderByDescending(a => a.id)
                .ToListAsync();

            return Ok(avaliacoes);
        }
    }
}
