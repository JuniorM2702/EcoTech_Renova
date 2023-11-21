namespace EcoTech_Renova.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class DetallePedido
{
    [Key]
    [Display(Name = "ID de Detalle de Pedido")]
    public Guid IDDetallePedido { get; set; }

    [Display(Name = "ID de Pedido")]
    public Guid IDPedido { get; set; }

    [Display(Name = "ID de Producto")]
    public string IDProducto { get; set; }

    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; }

    [Display(Name = "Precio Unitario")]
    public decimal PrecioUnitario { get; set; }

    public Pedido Pedido { get; set; }
    public Producto Producto { get; set; }
}
