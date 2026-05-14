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

        private int MinutosParaPermitirEdicao =>
            _configuration.GetValue<int>("Avaliacoes:MinutosParaPermitirEdicao", 2);

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
            var minutosCooldown = MinutosParaPermitirEdicao;

            var brutas = await _context.Avaliacoes
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
                    dataAvaliacao = a.DataAvaliacao,
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

            // Enriquece com podeEditar / editavelEm (só faz sentido nas próprias)
            var agora = DateTime.UtcNow;
            var avaliacoes = brutas.Select(a => new
            {
                a.id,
                a.usuarioId,
                a.mediaEstrelas,
                a.comentario,
                a.temLojaConveniencia,
                a.temCalibrador,
                a.temLavaRapido,
                a.temTrocaOleo,
                a.temAreaDescanso,
                a.temCarregadorEletrico,
                a.diasAtras,
                a.totalLikes,
                a.totalDislikes,
                a.meuVoto,
                a.eMinha,
                podeEditar = a.eMinha && (agora - a.dataAvaliacao).TotalMinutes >= minutosCooldown,
                editavelEm = a.eMinha
                    ? (DateTime?)a.dataAvaliacao.AddMinutes(minutosCooldown)
                    : null
            }).ToList();

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

            // Delta nos créditos do dono da avaliação:
            //   +1 quando perde um dislike ativo (toggle off ou troca para like)
            //   -1 quando ganha um dislike ativo (novo ou troca de like para dislike)
            int deltaCreditos = 0;
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
                if (!dto.IsLike) deltaCreditos = -1;
            }
            else if (votoExistente.IsLike == dto.IsLike)
            {
                // Mesmo tipo → remove (toggle off)
                _context.AvaliacaoVotos.Remove(votoExistente);
                acao = "removido";
                if (!votoExistente.IsLike) deltaCreditos = +1;
            }
            else
            {
                // Tipo diferente → troca
                votoExistente.IsLike = dto.IsLike;
                votoExistente.DataVoto = DateTime.UtcNow;
                acao = "atualizado";
                deltaCreditos = dto.IsLike ? +1 : -1;
            }

            // Atualiza créditos do dono da avaliação
            if (deltaCreditos != 0)
            {
                var dono = await _userManager.FindByIdAsync(avaliacao.UsuarioId);
                if (dono != null)
                {
                    dono.CreditosSociais += deltaCreditos;
                    // Banimento reversível: bane quando créditos <= 0, desbane quando > 0
                    dono.Banido = dono.CreditosSociais <= 0;
                    await _userManager.UpdateAsync(dono);
                }
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

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetAvaliacaoPorId(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized();

            var avaliacao = await _context.Avaliacoes.FirstOrDefaultAsync(a => a.Id == id);
            if (avaliacao == null) return NotFound(new { message = "Avaliação não encontrada." });

            if (avaliacao.UsuarioId != usuario.Id)
                return Forbid();

            var agora = DateTime.UtcNow;
            var podeEditar = (agora - avaliacao.DataAvaliacao).TotalMinutes >= MinutosParaPermitirEdicao;

            return Ok(new
            {
                id = avaliacao.Id,
                postoId = avaliacao.PostoId,
                notaGeral = avaliacao.NotaGeral,
                notaPrecos = avaliacao.NotaPrecos,
                notaAtendimento = avaliacao.NotaAtendimento,
                temLojaConveniencia = avaliacao.TemLojaConveniencia,
                temCalibrador = avaliacao.TemCalibrador,
                temLavaRapido = avaliacao.TemLavaRapido,
                temTrocaOleo = avaliacao.TemTrocaOleo,
                temAreaDescanso = avaliacao.TemAreaDescanso,
                temCarregadorEletrico = avaliacao.TemCarregadorEletrico,
                comentario = avaliacao.Comentario,
                dataAvaliacao = avaliacao.DataAvaliacao,
                podeEditar,
                editavelEm = avaliacao.DataAvaliacao.AddMinutes(MinutosParaPermitirEdicao)
            });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        [BloqueiaBanido]
        public async Task<IActionResult> AtualizarAvaliacao(int id, [FromBody] CreateAvaliacaoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized("Usuário não autenticado.");

            var avaliacao = await _context.Avaliacoes.FirstOrDefaultAsync(a => a.Id == id);
            if (avaliacao == null) return NotFound(new { message = "Avaliação não encontrada." });

            if (avaliacao.UsuarioId != usuario.Id)
                return Forbid();

            var agora = DateTime.UtcNow;
            var minutosDecorridos = (agora - avaliacao.DataAvaliacao).TotalMinutes;
            if (minutosDecorridos < MinutosParaPermitirEdicao)
            {
                var minutosRestantes = (int)Math.Ceiling(MinutosParaPermitirEdicao - minutosDecorridos);
                return BadRequest(new
                {
                    message = $"Esta avaliação ainda não pode ser editada. Aguarde {minutosRestantes} minuto(s).",
                    editavelEm = avaliacao.DataAvaliacao.AddMinutes(MinutosParaPermitirEdicao)
                });
            }

            avaliacao.NotaGeral = dto.NotaGeral;
            avaliacao.NotaPrecos = dto.NotaPrecos;
            avaliacao.NotaAtendimento = dto.NotaAtendimento;
            avaliacao.TemLojaConveniencia = dto.TemLojaConveniencia;
            avaliacao.TemCalibrador = dto.TemCalibrador;
            avaliacao.TemLavaRapido = dto.TemLavaRapido;
            avaliacao.TemTrocaOleo = dto.TemTrocaOleo;
            avaliacao.TemAreaDescanso = dto.TemAreaDescanso;
            avaliacao.TemCarregadorEletrico = dto.TemCarregadorEletrico;
            avaliacao.Comentario = dto.Comentario;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Avaliação atualizada com sucesso!" });
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
