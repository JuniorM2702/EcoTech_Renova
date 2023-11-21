using EcoTech_Renova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Data;

namespace EcoTech_Renova.Controllers
{
    public class ProductosController : Controller
    {
        IConfiguration _config;
        public ProductosController(IConfiguration config)
        {
           _config = config;
        }

        IEnumerable<Producto> getProductos()
        {
            List<Producto> temporal = new List<Producto>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SP_LISTAR_PRODUCTOS", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    temporal.Add(new Producto()
                    {
                        IDProducto = dr.GetString(0),
                        Nombre = dr.GetString(1),
                        Descripcion = dr.GetString(2),
                        Precio = dr.GetDecimal(3),
                        Stock = dr.GetInt32(4),
                        IDCategoria = dr.GetString(5),
                        IDUsuario = dr.GetString(6)
                    });
                }
                dr.Close();
            }
            return temporal;
        }

        public async Task<IActionResult> Productos()
        {
            if (HttpContext.Session.GetString("canasta") == null)
            {
                HttpContext.Session.SetString("canasta",
                    JsonConvert.SerializeObject(new List<RegistroProducto>()));
            }

            List<Producto> productos = (List<Producto>)await Task.Run(() => getProductos());
            List<RegistroProducto> carrito = JsonConvert.DeserializeObject<List<RegistroProducto>>(HttpContext.Session.GetString("canasta"));

            foreach (var producto in productos)
            {
                if (carrito.Any(registro => registro.IDProducto == producto.IDProducto))
                {
                    producto.YaRegistrado = true;
                }
            }

            return View(productos);
        }

        public async Task<IActionResult> Canasta()
        {
            //mostrar todos los productos seleccionados
            List<RegistroProducto> temporal = JsonConvert.DeserializeObject<List<RegistroProducto>>(
                        HttpContext.Session.GetString("canasta"));

            //envio la lista
            return View(await Task.Run(() => temporal));
        }

        [HttpPost]
        public async Task<IActionResult> Canasta(int codigo, int cantidad)
        {
            List<RegistroProducto> temporal = JsonConvert.DeserializeObject<List<RegistroProducto>>(
                        HttpContext.Session.GetString("canasta"));

            return View(await Task.Run(() => temporal));
        }

    }
}
