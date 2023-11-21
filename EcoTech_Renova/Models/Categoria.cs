namespace EcoTech_Renova.Models;
using System.ComponentModel.DataAnnotations;

public class Categoria
{
    [Key]
    [Display(Name = "ID de Categoría")]
    public string IDCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de categoría es obligatorio")]
    [Display(Name = "Nombre de Categoría")]
    public string NombreCategoria { get; set; }
}
