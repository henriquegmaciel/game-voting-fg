using System.ComponentModel.DataAnnotations;
using GameVoting.Models.Entities;
using GameVoting.Models.Validation;

namespace GameVoting.Models.ViewModels;

public class CreateGameViewModel
{
    [Required(ErrorMessage = "Título obrigatório")]
    public string Title { get; set; } = string.Empty;

    public string? Genre { get; set; }

    [Range(1970, 2100, ErrorMessage = "Ano inválido")]
    public int? ReleaseYear { get; set; }

    public string? ImageUrl { get; set; }

    [StoreUrl(ErrorMessage = "URL inválida ou domínio não permitido.")]
    public string? StorePageUrl { get; set; }

    [Required(ErrorMessage = "Lista obrigatória")]
    public ListType? ListType { get; set; }
}

public class EditGameViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Título obrigatório")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Genre { get; set; }

    [Range(1970, 2100, ErrorMessage = "Ano inválido")]
    public int? ReleaseYear { get; set; }

    public string? ImageUrl { get; set; }

    [StoreUrl(ErrorMessage = "URL inválida ou domínio não permitido.")]
    public string? StorePageUrl { get; set; }

    [Required(ErrorMessage = "Lista obrigatória")]
    public ListType ListType { get; set; }
}
