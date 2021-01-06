using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RegistroDeUsuario.Models
{
    public class RegistroLogin
    {
        [Display(Name ="E-mail de usuário")]
        [Required(AllowEmptyStrings = false, ErrorMessage ="O E-mail de usuário é obrigatório")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Lembrar")]
        public bool RememberMe { get; set; }
    }
}