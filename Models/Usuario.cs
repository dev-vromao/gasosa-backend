using Microsoft.AspNetCore.Identity;
using System;

namespace gasosa_backend.Models
{
    public class Usuario : IdentityUser
    {
        public string Nome { get; set; }
        public string CPF { get; set; }
        public DateTime DataNascimento { get; set; }

        /// <summary>
        /// Créditos sociais — começa em 1, diminui a cada dislike ativo nas
        /// avaliações deste usuário. Quando &lt; 0, vira banido.
        /// </summary>
        public int CreditosSociais { get; set; } = 1;

        /// <summary>
        /// Quando true, o usuário não pode realizar ações de escrita
        /// (criar avaliações, votar, cadastrar postos/preços, enviar fotos).
        /// </summary>
        public bool Banido { get; set; } = false;
    }
}
