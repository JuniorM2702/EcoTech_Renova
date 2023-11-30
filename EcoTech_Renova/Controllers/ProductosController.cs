using EcoTech_Renova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace EcoTech_Renova.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductosController(IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _config = config;
            _webHostEnvironment = webHostEnvironment;

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
                    string idUsuario = dr["IDUsuario"] as string;
                    ViewBag.Categorias = ObtenerCategorias();
                    // string nombreUsuario = ObtenerNombreUsuario(idUsuario);

                    temporal.Add(new Producto()
                    {
                        IDProducto = dr["IDProducto"] as string,
                        Nombre = dr["Nombre"] as string,
                        Descripcion = dr["Descripcion"] as string,
                        Precio = Convert.ToDecimal(dr["Precio"]),
                        Stock = Convert.ToInt32(dr["Stock"]),
                        IDCategoria = Convert.ToInt32(dr["IDCategoria"]),
                        IDUsuario = idUsuario,
                        ImagenProducto = dr["ImagenProducto"] as IFormFile
                    });
                }
                dr.Close();
            }
            return temporal;
        }

        [HttpPost]
        public async Task<IActionResult> DetalleProducto(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("Error");
                }

                Producto producto = await DetallesProducto(id);

                if (producto == null)
                {
                    return RedirectToAction("Error");
                }

                // Obtener y establecer el nombre del usuario en el ViewBag
                string nombreUsuario = ObtenerNombreUsuario(producto.IDUsuario);
                ViewBag.NombreUsuario = nombreUsuario;

                return View("DetalleProducto", producto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el método DetalleProducto: {ex.Message}");
                return View("Error");
            }
        }

        private async Task<Producto> DetallesProducto(string idProducto)
        {
            Producto producto = null;

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                await cn.OpenAsync();
                SqlCommand cmd = new SqlCommand("SP_DETALLE_PRODUCTO", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Agregar parámetros al comando
                cmd.Parameters.AddWithValue("@IDProducto", idProducto);

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    if (dr.Read())
                    {
                        producto = new Producto()
                        {
                            IDProducto = dr["IDProducto"] as string,
                            Nombre = dr["Nombre"] as string,
                            Descripcion = dr["Descripcion"] as string,
                            Precio = Convert.ToDecimal(dr["Precio"]),
                            Stock = Convert.ToInt32(dr["Stock"]),
                            IDCategoria = Convert.ToInt32(dr["IDCategoria"]),
                            IDUsuario = dr["IDUsuario"] as string,
                            ImagenProducto = dr["ImagenProducto"] as IFormFile
                        };
                    }
                }
            }

            return producto;
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

                return View("Error"); 
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
            string idVendedor = HttpContext.Session.GetString("IDUsuario");

            if (string.IsNullOrEmpty(idVendedor) && !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Usuarios"); 
            }

            var categorias = ObtenerCategorias();
            ViewBag.Categorias = new SelectList(categorias, "IDCategoria", "NombreCategoria");

            ViewBag.IDVendedor = idVendedor;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarProducto(Producto nuevoProducto)
        {
            try
            {
                string userId = HttpContext.Session.GetString("IDUsuario");

                if (string.IsNullOrEmpty(userId) && !User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Usuarios");
                }
                
                nuevoProducto.IDUsuario = userId;

                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    await cn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("SP_INSERTAR_PRODUCTOS", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_Nombre", nuevoProducto.Nombre);
                    cmd.Parameters.AddWithValue("@p_Descripcion", nuevoProducto.Descripcion);
                    cmd.Parameters.AddWithValue("@p_Precio", nuevoProducto.Precio);
                    cmd.Parameters.AddWithValue("@p_Stock", nuevoProducto.Stock);
                    cmd.Parameters.AddWithValue("@p_IDCategoria", nuevoProducto.IDCategoria);
                    cmd.Parameters.AddWithValue("@p_IDUsuario", nuevoProducto.IDUsuario);
                    if (nuevoProducto.ImagenProducto != null)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            nuevoProducto.ImagenProducto.CopyTo(stream);
                            byte[] imagenEnBytes = stream.ToArray();
                            cmd.Parameters.AddWithValue("@p_ImagenProducto", imagenEnBytes);
                        }
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@p_ImagenProducto", DBNull.Value);
                    }
                    await cmd.ExecuteNonQueryAsync();
                }             
                return RedirectToAction("Productos");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "Error al registrar el producto. Por favor, inténtelo nuevamente.");

                var categorias = ObtenerCategorias();
                ViewBag.Categorias = new SelectList(categorias, "IDCategoria", "NombreCategoria");
                ViewBag.IDVendedor = HttpContext.Session.GetString("IDUsuario");

                return View(nuevoProducto);
            }
        }

        public List<Categoria> ObtenerCategorias()
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

        public IActionResult ObtenerImagen(string id)
        {
            // Obtener la imagen de la base de datos según el ID del producto
            byte[] imagenBytes = ObtenerImagenDesdeBaseDeDatos(id);

            if (imagenBytes != null && imagenBytes.Length > 0)
            {
                // Convertir los bytes de la imagen a un Stream
                using (MemoryStream stream = new MemoryStream(imagenBytes))
                {
                    // Devolver la imagen como un FileResult
                    return File(stream.ToArray(), "image/jpg");
                }
            }

            return NotFound();
        }

        private byte[] ObtenerImagenDesdeBaseDeDatos(string id)
        {
            byte[] imagenBytes = null;

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ImagenProducto FROM Productos WHERE IDProducto = @IDProducto", cn);

                cmd.Parameters.AddWithValue("@IDProducto", id);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        imagenBytes = dr["ImagenProducto"] as byte[];
                    }
                }
            }

            return imagenBytes;
        }

        private string ObtenerNombreUsuario(string idUsuario)
        {
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Nombre FROM Usuarios WHERE IDUsuario = @IDUsuario", cn);
                cmd.Parameters.AddWithValue("@IDUsuario", idUsuario);

                object resultado = cmd.ExecuteScalar();

                return resultado != null ? resultado.ToString() : string.Empty;
            }
        }

        [HttpPost]
        public IActionResult AgregarAlCarrito(string id)
        {
            try
            {
                // Obtener el producto correspondiente al ID
                Producto producto = DetallesProducto(id).Result; // Usamos Result para obtener el resultado sincrónicamente

                if (producto != null)
                {
                    // Obtener o inicializar la canasta desde la sesión
                    List<RegistroProducto> carrito = JsonConvert.DeserializeObject<List<RegistroProducto>>(
                        HttpContext.Session.GetString("canasta")) ?? new List<RegistroProducto>();

                    // Verificar si el producto ya está en el carrito
                    if (!carrito.Any(registro => registro.IDProducto == producto.IDProducto))
                    {
                        // Agregar el producto al carrito con una cantidad inicial de 1
                        carrito.Add(new RegistroProducto
                        {
                            IDProducto = producto.IDProducto,
                            Nombre = producto.Nombre,
                            Precio = producto.Precio,
                            Cantidad = 1,
                            IDUsuario = producto.IDUsuario,
                            Stock = producto.Stock 
                        });

                        // Actualizar la sesión con la nueva canasta
                        HttpContext.Session.SetString("canasta", JsonConvert.SerializeObject(carrito));

                        // Devolver una respuesta exitosa
                        return Json(new { success = true });
                    }

                    // El producto ya está en el carrito, puedes manejar esto de acuerdo a tus requerimientos
                    return Json(new { success = false, message = "El producto ya está en el carrito" });
                }

                // Producto no encontrado, puedes manejar esto de acuerdo a tus requerimientos
                return Json(new { success = false, message = "Producto no encontrado" });
            }
            catch (Exception ex)
            {
                // Loguea el error (puedes utilizar ILogger aquí)
                Console.WriteLine($"Error en el método AgregarAlCarrito: {ex.Message}");

                // Devolver una respuesta de error
                return Json(new { success = false, message = "Error al agregar el producto al carrito" });
            }
        }

        public async Task<IActionResult> FiltrarPorCategoria(int categoriaId)
        {
            try
            {
                // Obtén la lista de productos según la categoría seleccionada
                List<Producto> productos = (List<Producto>)await Task.Run(() => getProductos().Where(p => p.IDCategoria == categoriaId).ToList());

                // Obtener el nombre de la categoría
                string nombreCategoria = ObtenerNombreCategoria(categoriaId);
                ViewBag.NombreCategoria = nombreCategoria;

                // Resto del código para manejar la sesión y otros detalles si es necesario

                return View("Productos", productos);
            }
            catch (Exception ex)
            {
                // Loguea el error (puedes utilizar ILogger aquí)
                Console.WriteLine($"Error en el método FiltrarPorCategoria: {ex.Message}");

                return View("Error");
            }
        }

        private string ObtenerNombreCategoria(int categoriaId)
        {
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SELECT NombreCategoria FROM Categorias WHERE IDCategoria = @IDCategoria", cn);
                cmd.Parameters.AddWithValue("@IDCategoria", categoriaId);

                object resultado = cmd.ExecuteScalar();

                return resultado != null ? resultado.ToString() : string.Empty;
            }
        }
    }
}
