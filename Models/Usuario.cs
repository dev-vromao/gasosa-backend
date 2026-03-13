using Microsoft.AspNetCore.Identity;
using System;

namespace gasosa_backend.Models
{
    public class Usuario : IdentityUser
    {
        public string Nome { get; set; }
        public string CPF { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}
