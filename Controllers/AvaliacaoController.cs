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

            // Tenta identificar o usuário se ele estiver autenticado (endpoint é público)
            var usuario = await _userManager.GetUserAsync(User);
            var meuId = usuario?.Id;

            var avaliacoes = await _context.Avaliacoes
                .Where(a => a.PostoId == postoId)
                .OrderByDescending(a => a.Id)
                .Select(a => new
                {
                    id = a.Id,
                    usuarioId = a.UsuarioId,
                    mediaEstrelas = (double)a.NotaGeral,
                    comentario = a.Comentario,
                    temLojaConveniencia = a.TemLojaConveniencia,
                    temCalibrador = a.TemCalibrador,
                    temLavaRapido = a.TemLavaRapido,
                    temTrocaOleo = a.TemTrocaOleo,
                    temAreaDescanso = a.TemAreaDescanso,
                    temCarregadorEletrico = a.TemCarregadorEletrico,
                    diasAtras = (DateTime.UtcNow - a.DataAvaliacao).Days,
                    totalLikes = _context.AvaliacaoVotos.Count(v => v.AvaliacaoId == a.Id && v.IsLike),
                    totalDislikes = _context.AvaliacaoVotos.Count(v => v.AvaliacaoId == a.Id && !v.IsLike),
                    meuVoto = meuId == null
                        ? null
                        : _context.AvaliacaoVotos
                            .Where(v => v.AvaliacaoId == a.Id && v.UsuarioId == meuId)
                            .Select(v => v.IsLike ? "like" : "dislike")
                            .FirstOrDefault(),
                    eMinha = meuId != null && a.UsuarioId == meuId
                })
                .ToListAsync();

            return Ok(avaliacoes);
        }

        [HttpPost("{id}/voto")]
        [Authorize]
        public async Task<IActionResult> Votar(int id, [FromBody] VotoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var avaliacao = await _context.Avaliacoes.FirstOrDefaultAsync(a => a.Id == id);
            if (avaliacao == null) return NotFound(new { message = "Avaliação não encontrada." });

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized(new { message = "Usuário não autenticado." });

            // Não permite votar na própria avaliação
            if (avaliacao.UsuarioId == usuario.Id)
                return BadRequest(new { message = "Você não pode votar na sua própria avaliação." });

            var votoExistente = await _context.AvaliacaoVotos
                .FirstOrDefaultAsync(v => v.AvaliacaoId == id && v.UsuarioId == usuario.Id);

            string acao;

            if (votoExistente == null)
            {
                // Sem voto → cria
                _context.AvaliacaoVotos.Add(new AvaliacaoVoto
                {
                    AvaliacaoId = id,
                    UsuarioId = usuario.Id,
                    IsLike = dto.IsLike,
                    DataVoto = DateTime.UtcNow
                });
                acao = "criado";
            }
            else if (votoExistente.IsLike == dto.IsLike)
            {
                // Mesmo tipo → remove (toggle off)
                _context.AvaliacaoVotos.Remove(votoExistente);
                acao = "removido";
            }
            else
            {
                // Tipo diferente → troca
                votoExistente.IsLike = dto.IsLike;
                votoExistente.DataVoto = DateTime.UtcNow;
                acao = "atualizado";
            }

            await _context.SaveChangesAsync();

            var totalLikes = await _context.AvaliacaoVotos.CountAsync(v => v.AvaliacaoId == id && v.IsLike);
            var totalDislikes = await _context.AvaliacaoVotos.CountAsync(v => v.AvaliacaoId == id && !v.IsLike);

            string? meuVoto = acao == "removido"
                ? null
                : (dto.IsLike ? "like" : "dislike");

            return Ok(new
            {
                acao,
                totalLikes,
                totalDislikes,
                meuVoto
            });
        }
    }
}
