using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A Palavra-passe é obrigatória.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Lembrar-me")]
        public bool LembrarMe { get; set; }
    }

    public class RegistarViewModel
    {
        [Required(ErrorMessage = "O Primeiro Nome é obrigatório.")]
        [Display(Name = "Primeiro Nome")]
        public string PrimeiroNome { get; set; }

        [Required(ErrorMessage = "O Último Nome é obrigatório.")]
        [Display(Name = "Último Nome")]
        public string UltimoNome { get; set; }

        [Required(ErrorMessage = "O Nome de Utilizador é obrigatório.")]
        [Display(Name = "Nome de Utilizador")]
        public string NomeUtilizador { get; set; }

        [Required(ErrorMessage = "O Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A Palavra-passe é obrigatória.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "A {0} tem de ter pelo menos {2} e um máximo de {1} caracteres.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Palavra-passe")]
        [Compare("Password", ErrorMessage = "A palavra-passe e a sua confirmação não correspondem.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Fotografia de Perfil")]
        public IFormFile? FotoFile { get; set; }
    }

    public class PerfilViewModel
    {
        public Utilizador User { get; set; } = null!;
        public List<Captura> Capturas { get; set; } = new();
    }

    public class EditarPerfilViewModel
    {
        [Required(ErrorMessage = "O Primeiro Nome é obrigatório.")]
        [Display(Name = "Primeiro Nome")]
        public string PrimeiroNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Último Nome é obrigatório.")]
        [Display(Name = "Último Nome")]
        public string UltimoNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Nome de Utilizador é obrigatório.")]
        [Display(Name = "Nome de Utilizador")]
        public string NomeUtilizador { get; set; } = string.Empty;

        [Display(Name = "Nova Fotografia de Perfil")]
        public IFormFile? FotoFile { get; set; }

        public string? FotoAtual { get; set; }
    }
}
