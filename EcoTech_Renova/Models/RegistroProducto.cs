namespace EcoTech_Renova.Models
{
    public class RegistroProducto
    {
        public string IDProducto { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public decimal Monto { get { return Precio * Stock; } }// Propiedad calculada para el monto

        // Podrías agregar más propiedades si fuera necesario
    }

}
