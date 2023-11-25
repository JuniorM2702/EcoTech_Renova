using EcoTech_Renova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EcoTech_Renova.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IConfiguration _config;


        public UsuariosController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Register(Usuario usuario)
        {
            try
            {
                if (CorreoElectronicoExistente(usuario.CorreoElectronico))
                {
                    ModelState.AddModelError("CorreoElectronico", "Este correo electrónico ya está registrado.");
                    return View(usuario);
                }

                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();

                    using (SqlCommand command = new SqlCommand("SP_REGISTRAR_USUARIO", cn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                        command.Parameters.AddWithValue("@CorreoElectronico", usuario.CorreoElectronico);
                        command.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);

                        command.ExecuteNonQuery();

                        ViewBag.Mensaje = "Usuario registrado correctamente.";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al registrar usuario. Por favor, inténtelo nuevamente.");
                return View(usuario);
            }
        }

        private bool CorreoElectronicoExistente(string correoElectronico)
        {
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();

                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE CorreoElectronico = @CorreoElectronico", cn))
                {
                    command.Parameters.AddWithValue("@CorreoElectronico", correoElectronico);

                    int userCount = (int)command.ExecuteScalar();
                    return userCount > 0;
                }
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Usuario usuario)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();

                    using (SqlCommand command = new SqlCommand("SELECT IDUsuario, Nombre FROM Usuarios WHERE CorreoElectronico = @CorreoElectronico AND Contrasena = @Contrasena", cn))
                    {
                        command.Parameters.AddWithValue("@CorreoElectronico", usuario.CorreoElectronico);
                        command.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string idUsuario = reader["IDUsuario"].ToString();
                                string nombreUsuario = reader["Nombre"].ToString();

                                HttpContext.Session.SetString("IDUsuario", idUsuario);
                                HttpContext.Session.SetString("UsuarioCorreo", usuario.CorreoElectronico);
                                HttpContext.Session.SetString("UsuarioNombre", nombreUsuario);

                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Credenciales inválidas. Por favor, inténtelo nuevamente.");
                                return View(usuario);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al iniciar sesión. Por favor, inténtelo nuevamente.");
                return View(usuario);
            }
        }


        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

      

    }

}

