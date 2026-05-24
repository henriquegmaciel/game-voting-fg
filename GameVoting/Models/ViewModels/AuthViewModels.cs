using System.ComponentModel.DataAnnotations;

namespace GameVoting.Models.ViewModels;

public class RegisterWithTokenViewModel
{
    [Required(ErrorMessage = "Token obrigatório")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome de usuário obrigatório")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha obrigatória")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmar senha obrigatório")]
    [Compare("Password", ErrorMessage = "As senhas não conferem")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginWithPasswordViewModel
{
    [Required(ErrorMessage = "Nome de usuário obrigatório")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha obrigatória")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
