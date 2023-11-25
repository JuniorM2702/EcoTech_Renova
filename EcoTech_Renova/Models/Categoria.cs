namespace EcoTech_Renova.Models;
using System.ComponentModel.DataAnnotations;

public class Categoria
{
    [Display(Name = "ID de Categoría")]
    public int IDCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de categoría es obligatorio")]
    [Display(Name = "Nombre de Categoría")]
    public string NombreCategoria { get; set; }
}
