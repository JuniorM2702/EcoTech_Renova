using EcoTech_Renova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Data;

namespace EcoTech_Renova.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IConfiguration _config;

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
                        IDProducto = dr["IDProducto"] as string,
                        Nombre = dr["Nombre"] as string,
                        Descripcion = dr["Descripcion"] as string,
                        Precio = Convert.ToDecimal(dr["Precio"]),
                        Stock = Convert.ToInt32(dr["Stock"]),
                        IDCategoria = Convert.ToInt32(dr["IDCategoria"]),
                        IDUsuario = dr["IDUsuario"] as string,
                    });
                }

                dr.Close();
            }
            return temporal;
        }

        public async Task<IActionResult> Productos()
        {
            try
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
            catch (Exception ex)
            {
                // Loguea el error (puedes utilizar ILogger aquí)
                Console.WriteLine($"Error en el método Productos: {ex.Message}");

                // Puedes redirigir a una vista de error o realizar alguna otra acción apropiada
                return View("Error"); // Asegúrate de tener una vista llamada "Error" en tu carpeta "Views"
            }
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

        [HttpGet]
        public IActionResult AgregarProducto()
        {
            // Obtener el ID del vendedor de la sesión actual
            string idVendedor = HttpContext.Session.GetString("IDUsuario");

            // Si el usuario no está autenticado, redirigir a la página de inicio de sesión
            if (string.IsNullOrEmpty(idVendedor) && !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Usuarios"); // Ajusta la acción y el controlador según tu configuración real
            }

            // Resto del código para obtener y pasar las categorías
            var categorias = ObtenerCategorias();
            ViewBag.Categorias = new SelectList(categorias, "IDCategoria", "NombreCategoria");

            // Pasa el ID del vendedor a la vista
            ViewBag.IDVendedor = idVendedor;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarProducto(Producto nuevoProducto, IFormFile imagen)
        {
            try
            {
                // Obtener el IDUsuario de la sesión actual
                string userId = HttpContext.Session.GetString("IDUsuario");

                // Si el usuario no está autenticado, redirigir a la página de inicio de sesión
                if (string.IsNullOrEmpty(userId) && !User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Usuarios");
                }
                
                // Asigna el ID del usuario al nuevo producto.
                nuevoProducto.IDUsuario = userId;

                // Resto del código para insertar el nuevo producto en la base de datos

                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    await cn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("SP_INSERTAR_PRODUCTOS", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Agregar parámetros al comando
                    cmd.Parameters.AddWithValue("@p_Nombre", nuevoProducto.Nombre);
                    cmd.Parameters.AddWithValue("@p_Descripcion", nuevoProducto.Descripcion);
                    cmd.Parameters.AddWithValue("@p_Precio", nuevoProducto.Precio);
                    cmd.Parameters.AddWithValue("@p_Stock", nuevoProducto.Stock);
                    cmd.Parameters.AddWithValue("@p_IDCategoria", nuevoProducto.IDCategoria);  // Asegúrate de que esta propiedad se llame correctamente
                    cmd.Parameters.AddWithValue("@p_IDUsuario", nuevoProducto.IDUsuario);

                    // Ejecutar el comando
                    await cmd.ExecuteNonQueryAsync();
                }
                if (imagen != null && imagen.Length > 0)
                {
                    var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    var rutaGuardar = Path.Combine("D:\\Workspace\\Ciclo 5\\Desarrollo de Servicios WEB I\\EcoTech_Renova\\EcoTech_Renova\\wwwroot\\img", nombreArchivo);

                    using (var stream = new FileStream(rutaGuardar, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }
                }

                return RedirectToAction("Productos");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar el producto: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Error al registrar el producto. Por favor, inténtelo nuevamente.");

                // Vuelve a pasar las categorías y el ID del vendedor a la vista
                var categorias = ObtenerCategorias();
                ViewBag.Categorias = new SelectList(categorias, "IDCategoria", "NombreCategoria");
                ViewBag.IDVendedor = HttpContext.Session.GetString("IDUsuario");

                return View(nuevoProducto);
            }
        }


        private List<Categoria> ObtenerCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SELECT IDCategoria, NombreCategoria FROM Categorias", cn);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    categorias.Add(new Categoria
                    {
                        IDCategoria = dr.GetInt32(0),
                        NombreCategoria = dr.GetString(1)
                    });
                }

                dr.Close();
            }

            return categorias;
        }

    }
}
