using System.ComponentModel.DataAnnotations;

namespace EcoTech_Renova.Models
{
    public class Usuario 
    {
        [Display(Name = "ID del Usuario")]
        public string IDUsuario { get; set; }

        [Display(Name = "Nombre de Usuario/Entidad")]
        public string Nombre { get; set; }

        [Display(Name = "Correo Electrónico")]
        public string CorreoElectronico { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contrasena { get; set; }

        public Rol Rol {  get; set; }

    }
}
