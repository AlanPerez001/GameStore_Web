using GameStore_Web.Models;
using GameStore_Web.Models.Catalogo;
using GameStore_Web.WebApi;
using GameStore_Web.WebApi.Autenticacion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GameStore_Web.Controllers
{
    public class HomeController : Controller
    {
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            var modeloRespuesta = new MVReporte();
            var autoLogin = ConfigurationManager.AppSettings["autologin"];
            if (autoLogin != null && autoLogin.ToString() == "1")
            {
                string plataforma = ConfigurationManager.AppSettings["Plataforma"].ToString();
                string ipCliente = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Request.ServerVariables["REMOTE_ADDR"];
                string app = ConfigurationManager.AppSettings["App"].ToString();
                string version = ConfigurationManager.AppSettings["Version"].ToString();
                WAPeticionLogin peticion = new WAPeticionLogin(ConfigurationManager.AppSettings["userHardCode"].ToString(),
                    ConfigurationManager.AppSettings["passHardCode"].ToString(), app, version, "", plataforma, ipCliente);
                ClienteWebApi clienteWebApi = new ClienteWebApi();
                var respuestaWebApi = await clienteWebApi.callWebApiSinAutorizacion("Autenticacion/Login",
                    JsonConvert.SerializeObject(peticion));
                if (respuestaWebApi != null && respuestaWebApi.statusCode == 200)
                {
                    ////Se guarda el JWt en Session y el RefreshToken en Cookie
                    RespuestaAutenticacion modeloAprobado = respuestaWebApi.respuesta.Datos.ToObject<RespuestaAutenticacion>();
                    Session[ConfigurationManager.AppSettings["nameJWT"].ToString()] = modeloAprobado.jwtToken;
                    Session[ConfigurationManager.AppSettings["nameUser"].ToString()] = respuestaWebApi.respuesta.idResponse;

                    HttpCookie refreshTokenCookie = new HttpCookie(ConfigurationManager.AppSettings["nameRefreshToken"].ToString());
                    refreshTokenCookie.Value = modeloAprobado.refreshToken;
                    refreshTokenCookie.Expires = DateTime.Now.AddDays(30);
                    refreshTokenCookie.HttpOnly = true;
                    //refreshTokenCookie.Secure = true;
                    Response.SetCookie(refreshTokenCookie);
                    FormsAuthentication.SetAuthCookie(Session[ConfigurationManager.AppSettings["nameJWT"].ToString()].ToString(), false);


                    modeloRespuesta.CatVideoJuegos = new List<VideoJuegos>();
                    var filtroGenero = new List<DropGeneral>();
                    var filtroConsola = new List<DropGeneral>();
                    var respuestaWebApiObjeto = await clienteWebApi.callWebApiAutorizacionGetLista($"Catalogo/ObtenerVidejuegos");
                    if (respuestaWebApiObjeto != null && respuestaWebApiObjeto.statusCode == 200)
                    {
                        modeloRespuesta.CatVideoJuegos = respuestaWebApiObjeto.respuesta.Datos.ToObject<List<VideoJuegos>>();
                    }
                    var respuestaWebApiLista = await clienteWebApi.callWebApiAutorizacionGetLista($"Catalogo/ObtenerFiltros/{1}");
                    if (respuestaWebApiLista != null && respuestaWebApiLista.statusCode == 200)
                    {
                        filtroGenero = respuestaWebApiLista.respuesta.Datos.ToObject<List<DropGeneral>>();
                    }
                    var respuestaWebApiLista2 = await clienteWebApi.callWebApiAutorizacionGetLista($"Catalogo/ObtenerFiltros/{2}");
                    if (respuestaWebApiLista2 != null && respuestaWebApiLista2.statusCode == 200)
                    {
                        filtroConsola = respuestaWebApiLista2.respuesta.Datos.ToObject<List<DropGeneral>>();
                    }
                    ViewBag.filtroGenero = filtroGenero;
                    ViewBag.filtroConsola = filtroConsola;
                    ViewBag.VideoJuegos = modeloRespuesta.CatVideoJuegos;
                    return View(modeloRespuesta);

                }
                else
                {
                    TempData["respuesta"] = respuestaWebApi == null ? "Por el momento el servicio esta fuera de linea, intentalo más tarde." : respuestaWebApi.message;
                }
            }
            return View(modeloRespuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EliminaJuego(int IdJuegoEliminar)
        {
            try
            {

                ClienteWebApi clienteWebApi = new ClienteWebApi();
                PeticionEliminar peticion = new PeticionEliminar() { IdJuego = IdJuegoEliminar };
                var respuestaWebApi = await clienteWebApi.callWebApiAutorizacionPostValor("Catalogo/EliminaRegistro",
                    JsonConvert.SerializeObject(peticion));
                if (respuestaWebApi != null && respuestaWebApi.statusCode == 200)
                {
                    TempData["idRespuesta"] = respuestaWebApi.respuesta.idResponse;
                    TempData["respuesta"] = respuestaWebApi.respuesta.Response;
                    if (respuestaWebApi.respuesta.idResponse > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["idRespuesta"] = 0;
                    TempData["respuesta"] = respuestaWebApi.message;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message} - {ex.StackTrace} ");
                TempData["idRespuesta"] = 0;
                //TempData["respuesta"] = Generales.mensajeExcepcion();
            }
            return RedirectToAction("Index"); //EditarRegistro
        }

        [HttpGet]
        public async Task<ActionResult> EditarRegistro(int idJuego)
        {
            var modelo = new MVReporte();
            modelo.Detalles = new MVDetalleJuego();
            try
            {
                ClienteWebApi clienteWebApi = new ClienteWebApi();
                if (idJuego > 0)
                {
                    if (!string.IsNullOrEmpty(idJuego.ToString()))
                    {

                        var idReporte = 0;

                        var respuestaWebApiObjeto = await clienteWebApi.callWebApiAutorizacionGetObjeto($"Catalogo/ObtenerDetalleJuego/{idJuego}");
                        if (respuestaWebApiObjeto != null && respuestaWebApiObjeto.statusCode == 200)
                        {
                            modelo.Detalles = respuestaWebApiObjeto.respuesta.Datos.ToObject<MVDetalleJuego>();
                            modelo.Detalles.idJuego = idJuego;
                            ViewBag.Titulo = modelo.Detalles.Titulo;
                            ViewBag.Descripcion = modelo.Detalles.Descripcion;
                            ViewBag.AnioPublicacion = modelo.Detalles.AnioPublicacion;
                            ViewBag.Consolas = modelo.Detalles.Consolas;
                            ViewBag.Genero = modelo.Detalles.Genero;
                            ViewBag.Calificacion = modelo.Detalles.Calificacion;
                        }
                    }

                }
                else
                {
                    var filtroGenero = new List<DropGeneral>();
                    var filtroConsola = new List<DropGeneral>();
                    var respuestaWebApiLista = await clienteWebApi.callWebApiAutorizacionGetLista($"Catalogo/ObtenerFiltros/{1}");
                    if (respuestaWebApiLista != null && respuestaWebApiLista.statusCode == 200)
                    {
                        filtroGenero = respuestaWebApiLista.respuesta.Datos.ToObject<List<DropGeneral>>();
                    }
                    var respuestaWebApiLista2 = await clienteWebApi.callWebApiAutorizacionGetLista($"Catalogo/ObtenerFiltros/{2}");
                    if (respuestaWebApiLista2 != null && respuestaWebApiLista2.statusCode == 200)
                    {
                        filtroConsola = respuestaWebApiLista2.respuesta.Datos.ToObject<List<DropGeneral>>();
                    }
                    ViewBag.filtroGenero = filtroGenero;
                    ViewBag.filtroConsola = filtroConsola;
                }
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> JuegosFiltro(MVReporte model)
        {

            var modelo = new MVReporte();
            modelo.CatVideoJuegos = new List<VideoJuegos>();
            var filtroGenero = new List<DropGeneral>();
            var filtroConsola = new List<DropGeneral>();

            try
            {
                ClienteWebApi clienteWebApi = new ClienteWebApi();
                var respuestaWebApiObjeto = await clienteWebApi.callWebApiAutorizacionGetLista($"Catalogo/ObtenerjuegosFiltro/{model.FiltroGenero.Id}/{model.FiltroConsola.Id}");
                if (respuestaWebApiObjeto != null && respuestaWebApiObjeto.statusCode == 200)
                {
                    modelo.CatVideoJuegos = respuestaWebApiObjeto.respuesta.Datos.ToObject<List<VideoJuegos>>();
                }
                var respuestaWebApiLista = await clienteWebApi.callWebApiAutorizacionGetLista($"Catalogo/ObtenerFiltros/{1}");
                if (respuestaWebApiLista != null && respuestaWebApiLista.statusCode == 200)
                {
                    filtroGenero = respuestaWebApiLista.respuesta.Datos.ToObject<List<DropGeneral>>();
                }
                var respuestaWebApiLista2 = await clienteWebApi.callWebApiAutorizacionGetLista($"Catalogo/ObtenerFiltros/{2}");
                if (respuestaWebApiLista2 != null && respuestaWebApiLista2.statusCode == 200)
                {
                    filtroConsola = respuestaWebApiLista2.respuesta.Datos.ToObject<List<DropGeneral>>();
                }
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
            ViewBag.filtroGenero = filtroGenero;
            ViewBag.filtroConsola = filtroConsola;
            ViewBag.VideoJuegos = modelo.CatVideoJuegos;
            return View("Index", modelo);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GuardarJuego(MVReporte model)
        {
            try
            {

                PeticionGuardarRegistro peticion = new PeticionGuardarRegistro();
                peticion.IdJuego = model.Detalles.idJuego;
                peticion.Titulo = model.Detalles.Titulo;
                peticion.Descripcion = model.Detalles.Descripcion;
                peticion.AnioPublicacion = model.Detalles.AnioPublicacion;
                peticion.Calificacion = model.Detalles.Calificacion;
                peticion.idConsola = model.Detalles.idConsola;
                peticion.idGenero = model.Detalles.idGenero;
                ClienteWebApi clienteWebApi = new ClienteWebApi();

                var respuestaWebApi = await clienteWebApi.callWebApiAutorizacionPostObjeto($"Catalogo/GuardaRegistro", JsonConvert.SerializeObject(peticion));
                if (respuestaWebApi != null && respuestaWebApi.statusCode == 200)
                {
                    TempData["idRespuesta"] = respuestaWebApi.respuesta.idResponse;
                    TempData["respuesta"] = respuestaWebApi.respuesta.Response;

                    if (respuestaWebApi.respuesta.idResponse > 0)
                    {
                        return RedirectToAction("EditarRegistro", new { idJuego = respuestaWebApi.respuesta.idResponse });
                    }
                    else
                    {
                        TempData["modelo"] = model;
                    }
                }
                else
                {
                    TempData["idRespuesta"] = 0;
                    TempData["respuesta"] = respuestaWebApi.message;
                    TempData["modelo"] = model;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message} - {ex.StackTrace} ");
                TempData["idRespuesta"] = 0;
            }
            //cargaTemporalesUsuario(model, ImagenUsuario);
            return RedirectToAction("EditarRegistro", new { idJuego = model.Detalles.idJuego });

        }
    }
}