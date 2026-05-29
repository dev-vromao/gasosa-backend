using gasosa_backend.Dtos.Avaliacao;
using gasosa_backend.Filters;
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
        private readonly IConfiguration _configuration;

        public AvaliacaoController(
            DataContext context,
            UserManager<Usuario> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // Banimento por porcentagem de avaliações ruins (dislikes > likes).
        // Configurável em appsettings.json → "Banimento".
        private double PercentualMinimoAvaliacoesRuins =>
            _configuration.GetValue<double>("Banimento:PercentualMinimoAvaliacoesRuins", 50);

        private int MinimoAvaliacoesParaConsiderar =>
            _configuration.GetValue<int>("Banimento:MinimoAvaliacoesParaConsiderar", 2);

        /// <summary>
        /// Recalcula o status de banimento de um usuário com base na proporção
        /// de avaliações dele em que os dislikes superam os likes.
        /// Só pune se o usuário tiver pelo menos <see cref="MinimoAvaliacoesParaConsiderar"/> avaliações.
        /// </summary>
        private async Task AtualizarStatusBanimentoAsync(string usuarioId)
        {
            var resumo = await _context.Avaliacoes
                .Where(a => a.UsuarioId == usuarioId)
                .Select(a => new
                {
                    Likes = _context.AvaliacaoVotos.Count(v => v.AvaliacaoId == a.Id && v.IsLike),
                    Dislikes = _context.AvaliacaoVotos.Count(v => v.AvaliacaoId == a.Id && !v.IsLike)
                })
                .ToListAsync();

            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null) return;

            var totalAvaliacoes = resumo.Count;
            bool deveBanir;

            if (totalAvaliacoes < MinimoAvaliacoesParaConsiderar)
            {
                // Amostra insuficiente: nunca está banido por este critério
                deveBanir = false;
            }
            else
            {
                var avaliacoesRuins = resumo.Count(r => r.Dislikes > r.Likes);
                var percentual = (double)avaliacoesRuins / totalAvaliacoes * 100;
                deveBanir = percentual >= PercentualMinimoAvaliacoesRuins;
            }

            if (usuario.Banido != deveBanir)
            {
                usuario.Banido = deveBanir;
                await _userManager.UpdateAsync(usuario);
            }
        }

        [HttpPost]
        [Authorize]
        [BloqueiaBanido]
        public async Task<IActionResult> CriarAvaliacao([FromBody] CreateAvaliacaoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var postoExiste = await _context.Postos.AnyAsync(p => p.Id == dto.PostoId);
            if (!postoExiste) return NotFound("O posto informado não existe.");

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized("Usuário não autenticado.");

            // Apenas uma avaliação por usuário por posto
            var jaAvaliou = await _context.Avaliacoes
                .AnyAsync(a => a.PostoId == dto.PostoId && a.UsuarioId == usuario.Id);
            if (jaAvaliou)
            {
                return Conflict(new { message = "Você já avaliou este posto." });
            }

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
        [BloqueiaBanido]
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

            // Recalcula o banimento do dono da avaliação com a nova distribuição de votos
            await AtualizarStatusBanimentoAsync(avaliacao.UsuarioId);

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

        [HttpGet("minhas")]
        [Authorize]
        public async Task<IActionResult> GetMinhasAvaliacoes()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized("Usuário não autenticado.");

            var minhasAvaliacoes = await _context.Avaliacoes
                .Include(a => a.Posto)
                .Where(a => a.UsuarioId == usuario.Id)
                .OrderByDescending(a => a.DataAvaliacao)
                .Select(a => new
                {
                    id = a.Id,
                    postoId = a.PostoId,
                    postoNome = a.Posto.Nome,
                    mediaEstrelas = (double)a.NotaGeral,
                    comentario = a.Comentario,
                    temLojaConveniencia = a.TemLojaConveniencia,
                    temCalibrador = a.TemCalibrador,
                    temLavaRapido = a.TemLavaRapido,
                    temTrocaOleo = a.TemTrocaOleo,
                    temAreaDescanso = a.TemAreaDescanso,
                    temCarregadorEletrico = a.TemCarregadorEletrico,
                    dataAvaliacao = a.DataAvaliacao
                })
                .ToListAsync();

            return Ok(minhasAvaliacoes);
        }
    }
}
