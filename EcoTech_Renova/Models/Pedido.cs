namespace EcoTech_Renova.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class Pedido
{
    [Key]
    [Display(Name = "ID de Pedido")]
    public Guid IDPedido { get; set; }

    [Display(Name = "ID de Usuario")]
    public string IDUsuario { get; set; }

    [Display(Name = "Fecha de Pedido")]
    public DateTime FechaPedido { get; set; }

    [Display(Name = "Estado")]
    public string Estado { get; set; }

    public Usuario Usuario { get; set; }
}
