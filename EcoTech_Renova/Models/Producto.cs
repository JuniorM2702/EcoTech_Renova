using System.ComponentModel.DataAnnotations;
namespace EcoTech_Renova.Models
{

    public class Producto
    {
        [Display(Name = "ID de Producto")]
        public string IDProducto { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        [Display(Name = "Precio", Prompt = "Ingrese el precio")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        public int Stock { get; set; }

        [Display(Name = "Categoría")]
        public int IDCategoria { get; set; }

        [Display(Name = "Vendedor")]
        public string IDUsuario { get; set; }

        [Required]
        [Display(Name = "Imagen del Producto")]
        public IFormFile ImagenProducto { get; set; }

        public bool YaRegistrado { get; set; }
    }
}