﻿namespace EcoTech_Renova.Models
{
    public class Rol
    {
        public int IDRol {  get; set; }
        public string NombreRol { get; set; }
        public List<Usuario> Usuarios { get; set; }

    }
}
