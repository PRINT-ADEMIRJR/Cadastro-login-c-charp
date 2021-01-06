using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RegistroDeUsuario.Models
{
    [MetadataType(typeof(UserMetaData))]
    public partial class Registro
    {
        public string ConfirmarPassword { get; set; }

    }
    public class UserMetaData{

        [Display(Name = "Primeiro nome")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Preenchimento obrigatório")]
        public string PrimeiroNome { get; set; }

        [Display(Name = "Sobre nome")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Preenchimento obrigatório")]
        public string Sobrenome { get; set; }

        [Display(Name = "E-mail")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Preenchimento obrigatório")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString ="{0:MM/dd/yyyy}")]
        public DateTime DataNascimento { get; set; }

        [Display(Name = "Senha")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Preenchimento obrigatório")]
        [DataType(DataType.Password)]
        [MinLength(6,ErrorMessage = "Sua senha deve conter no mínimo 6 caracteres")]
        public string Password { get; set; }

        [Display(Name = "Confirmar senha")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="As senhas não são iguais")]
        public string ConfirmarPassword { get; set; }



    }
}