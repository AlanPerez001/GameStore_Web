using GameStore_Web.WebApi.Autenticacion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GameStore_Web.WebApi
{
    /// <summary>
    /// Clase para llamar al web api
    /// </summary>
    public class ClienteWebApi
    {
        private Uri url = new Uri(ConfigurationManager.AppSettings["UrlWebApi"].ToString());
        private string nameJwt = ConfigurationManager.AppSettings["nameJWT"].ToString();
        private string nameRefreshToken = ConfigurationManager.AppSettings["nameRefreshToken"].ToString();
        private string nameUser = ConfigurationManager.AppSettings["nameUser"].ToString();
        private const int timeoutWebApi = 120;

        /// <summary>
        /// Metodo para hacer llamadas post sin autorizacion al wep api. Inicio de sesion por ejemplo.
        /// </summary>
        /// <param name="metodoYAccion">Metodo</param>
        /// <param name="postBody">Datos a enviar por post</param>
        /// <returns></returns>
        public async Task<RespuestaApiObjeto> callWebApiSinAutorizacion(string metodoYAccion, string postBody)
        {
            RespuestaApiObjeto res = null;
            try
            {
                using (var cliente = new HttpClient())
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                    var cts = new CancellationTokenSource();
                    var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                    var requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                    using (var response = await cliente.PostAsync(uri, requestContent))
                    {
                        //var statusCode = response.StatusCode;
                        var content = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(content);
                        res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(postBody);
            }
            return res;
        }

        public async Task<RespuestaApiObjeto> callWebApiSinAutorizacionGetObjeto(string metodoYAccion)
        {
            var res = new RespuestaApiObjeto();
            try
            {
                using (var cliente = new HttpClient())
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                    var cts = new CancellationTokenSource();
                    var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                    using (var response = await cliente.GetAsync(uri))
                    {
                        //var statusCode = response.StatusCode;
                        var content = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(content);
                        res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return res;
        }

        public async Task<RespuestaApiLista> callWebApiSinAutorizacionGetLista(string metodoYAccion)
        {
            var res = new RespuestaApiLista();
            try
            {
                using (var cliente = new HttpClient())
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                    var cts = new CancellationTokenSource();
                    var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                    using (var response = await cliente.GetAsync(uri))
                    {
                        //var statusCode = response.StatusCode;
                        var content = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(content);
                        res = JsonConvert.DeserializeObject<RespuestaApiLista>(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return res;
        }

        //Metodo para cuando el web api devuelva un objeto en el valor datos
        public async Task<RespuestaApiObjeto> callWebApiAutorizacionPostObjeto(string metodoYAccion, string postBody)
        {
            var res = new RespuestaApiObjeto();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        var requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                        using (var response = await cliente.PostAsync(uri, requestContent))
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(content);
                            res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                            if (res != null)
                            {
                                //Si el web api responde 401 no autorizado se hace una refresh token 
                                if (res.statusCode == 401)
                                {
                                    //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                    //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                    var actualizaToken = await callWebApiRefreshToken();
                                    if (actualizaToken)
                                    {
                                        sessionJwt = HttpContext.Current.Session[nameJwt];
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                        requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                                        using (var response2 = await cliente.PostAsync(uri, requestContent))
                                        {
                                            content = await response2.Content.ReadAsStringAsync();
                                            res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                                            if (res.statusCode == 401)
                                            {
                                                res = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        res = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(postBody);
            }
            return res;
        }
        //Metodo para cuando el web api devuelva un objeto en el valor datos
        public async Task<RespuestaApiObjeto> callWebApiAutorizacionGetObjeto(string metodoYAccion)
        {
            var res = new RespuestaApiObjeto();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        using (var response = await cliente.GetAsync(uri))
                        {
                            //var statusCode = response.StatusCode;
                            var content = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(content);
                            res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                            if (res != null)
                            {
                                //Si el web api responde 401 no autorizado se hace una refresh token 
                                if (res.statusCode == 401)
                                {
                                    //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                    //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                    var actualizaToken = await callWebApiRefreshToken();
                                    if (actualizaToken)
                                    {
                                        sessionJwt = HttpContext.Current.Session[nameJwt];
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                        using (var response2 = await cliente.GetAsync(uri))
                                        {
                                            content = await response2.Content.ReadAsStringAsync();
                                            res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                                            if (res.statusCode == 401)
                                            {
                                                res = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        res = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return res;
        }

        //Metodo para cuando el web api devuelva una lista en el valor datos
        public async Task<RespuestaApiLista> callWebApiAutorizacionPostLista(string metodoYAccion, string postBody)
        {
            var res = new RespuestaApiLista();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        var requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                        using (var response = await cliente.PostAsync(uri, requestContent))
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(content);
                            res = JsonConvert.DeserializeObject<RespuestaApiLista>(content);
                            if (res != null)
                            {
                                //Si el web api responde 401 no autorizado se hace una refresh token 
                                if (res.statusCode == 401)
                                {
                                    //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                    //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                    var actualizaToken = await callWebApiRefreshToken();
                                    if (actualizaToken)
                                    {
                                        sessionJwt = HttpContext.Current.Session[nameJwt];
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                        requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                                        using (var response2 = await cliente.PostAsync(uri, requestContent))
                                        {
                                            content = await response2.Content.ReadAsStringAsync();
                                            res = JsonConvert.DeserializeObject<RespuestaApiLista>(content);
                                            if (res.statusCode == 401)
                                            {
                                                res = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        res = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(postBody);
            }
            return res;
        }

        //Metodo para cuando el web api devuelva una lista en el valor datos
        public async Task<RespuestaApiLista> callWebApiAutorizacionGetLista(string metodoYAccion)
        {
            var res = new RespuestaApiLista();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        using (var response = await cliente.GetAsync(uri))
                        {
                            //var statusCode = response.StatusCode;
                            var content = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(content);
                            res = JsonConvert.DeserializeObject<RespuestaApiLista>(content);
                            if (res != null)
                            {
                                //Si el web api responde 401 no autorizado se hace una refresh token 
                                if (res.statusCode == 401)
                                {
                                    //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                    //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                    var actualizaToken = await callWebApiRefreshToken();
                                    if (actualizaToken)
                                    {
                                        sessionJwt = HttpContext.Current.Session[nameJwt];
                                        Debug.WriteLine(sessionJwt);
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                        using (var response2 = await cliente.GetAsync(uri))
                                        {
                                            content = await response2.Content.ReadAsStringAsync();
                                            res = JsonConvert.DeserializeObject<RespuestaApiLista>(content);
                                            if (res.statusCode == 401)
                                            {
                                                res = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        res = null;
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return res;
        }

        //Metodo para cuando el web api devuelva un valor primitivo en el valor datos
        public async Task<RespuestaApiValor> callWebApiAutorizacionPostValor(string metodoYAccion, string postBody)
        {
            var res = new RespuestaApiValor();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        var requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                        using (var response = await cliente.PostAsync(uri, requestContent))
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(content);
                            res = JsonConvert.DeserializeObject<RespuestaApiValor>(content);
                            if (res != null)
                            {
                                //Si el web api responde 401 no autorizado se hace una refresh token 
                                if (res.statusCode == 401)
                                {
                                    //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                    //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                    var actualizaToken = await callWebApiRefreshToken();
                                    if (actualizaToken)
                                    {
                                        sessionJwt = HttpContext.Current.Session[nameJwt];
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                        requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                                        using (var response2 = await cliente.PostAsync(uri, requestContent))
                                        {
                                            content = await response2.Content.ReadAsStringAsync();
                                            res = JsonConvert.DeserializeObject<RespuestaApiValor>(content);
                                            if (res.statusCode == 401)
                                            {
                                                res = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        res = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(postBody);
            }
            return res;
        }

        //Metodo para cuando el web api devuelva un valor primitivo en el valor datos
        public async Task<RespuestaApiValor> callWebApiAutorizacionGetValor(string metodoYAccion)
        {
            var res = new RespuestaApiValor();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        using (var response = await cliente.GetAsync(uri))
                        {
                            //var statusCode = response.StatusCode;
                            var content = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(content);
                            res = JsonConvert.DeserializeObject<RespuestaApiValor>(content);
                            if (res != null)
                            {
                                //Si el web api responde 401 no autorizado se hace una refresh token 
                                if (res.statusCode == 401)
                                {
                                    //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                    //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                    var actualizaToken = await callWebApiRefreshToken();
                                    if (actualizaToken)
                                    {
                                        sessionJwt = HttpContext.Current.Session[nameJwt];
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                        using (var response2 = await cliente.GetAsync(uri))
                                        {
                                            content = await response2.Content.ReadAsStringAsync();
                                            res = JsonConvert.DeserializeObject<RespuestaApiValor>(content);
                                            if (res.statusCode == 401)
                                            {
                                                res = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        res = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return res;
        }

        public async Task<RespuestaApiObjeto> callWebApiAutorizacionPostMultipartObjeto(string metodoYAccion, List<ByteArrayContent> campos
           , List<ByteArrayContent> campos2)
        {
            var res = new RespuestaApiObjeto();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        using (var requestContent = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                        {
                            foreach (var campo in campos)
                            {
                                requestContent.Add(campo);
                            }
                            using (var response = await cliente.PostAsync(uri, requestContent))
                            {
                                var content = await response.Content.ReadAsStringAsync();
                                Debug.WriteLine(content);
                                res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                                if (res != null)
                                {
                                    //Si el web api responde 401 no autorizado se hace una refresh token 
                                    if (res.statusCode == 401)
                                    {
                                        //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                        //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                        var actualizaToken = await callWebApiRefreshToken();
                                        if (actualizaToken)
                                        {
                                            sessionJwt = HttpContext.Current.Session[nameJwt];
                                            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                            using (var requestContent2 = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                                            {
                                                foreach (var campo in campos2)
                                                {
                                                    requestContent2.Add(campo);
                                                }
                                                using (var response2 = await cliente.PostAsync(uri, requestContent2))
                                                {
                                                    content = await response2.Content.ReadAsStringAsync();
                                                    res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                                                    if (res.statusCode == 401)
                                                    {
                                                        res = null;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            res = null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }

        public async Task<bool> callWebApiRefreshToken()
        {
            var resultado = false;
            var cookieRefreshToken = HttpContext.Current.Request.Cookies[ConfigurationManager.AppSettings["nameRefreshToken"].ToString()];
            var sessionUser = HttpContext.Current.Session[nameUser];
            var sessionJwt = HttpContext.Current.Session[nameJwt];
            if (cookieRefreshToken != null && sessionUser != null)
            {
                string refreshToken = cookieRefreshToken.Value;
                string user = sessionUser.ToString();
                string jwt = sessionJwt.ToString();
                var peticion = new ActualizarToken(refreshToken, user, jwt);
                try
                {
                    using (var cliente = new HttpClient())
                    {
                        var res = new RespuestaApiObjeto();
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        var cts = new CancellationTokenSource();
                        var postBody = JsonConvert.SerializeObject(peticion);
                        var uri = new Uri(string.Format("{0}{1}", url, "Autenticacion/RefreshToken"));
                        var requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                        var response = await cliente.PostAsync(uri, requestContent);
                        var content = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(content);
                        res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                        if (res != null)
                        {
                            if (res.statusCode == 200)
                            {
                                ////Se guarda el JWt en Session y el RefreshToken en Cookie
                                RespuestaAutenticacion modeloAprobado = res.respuesta.Datos.ToObject<RespuestaAutenticacion>();
                                HttpContext.Current.Session[ConfigurationManager.AppSettings["nameJWT"].ToString()] = modeloAprobado.jwtToken;
                                HttpContext.Current.Session[ConfigurationManager.AppSettings["nameUser"].ToString()] = res.respuesta.idResponse;
                                HttpCookie refreshTokenCookie = new HttpCookie(ConfigurationManager.AppSettings["nameRefreshToken"].ToString());
                                refreshTokenCookie.Value = modeloAprobado.refreshToken;
                                refreshTokenCookie.Expires = DateTime.Now.AddDays(30);
                                refreshTokenCookie.HttpOnly = true;
                                //refreshTokenCookie.Secure = true;
                                HttpContext.Current.Response.SetCookie(refreshTokenCookie);
                                resultado = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return resultado;
        }

        public bool callWebApiRefreshTokenSincrono()
        {
            var resultado = false;
            var cookieRefreshToken = HttpContext.Current.Request.Cookies[ConfigurationManager.AppSettings["nameRefreshToken"].ToString()];
            var sessionUser = HttpContext.Current.Session[nameUser];
            var sessionJwt = HttpContext.Current.Session[nameJwt];
            if (cookieRefreshToken != null && sessionUser != null)
            {
                string refreshToken = cookieRefreshToken.Value;
                string user = sessionUser.ToString();
                string jwt = sessionJwt.ToString();
                var peticion = new ActualizarToken(refreshToken, user, jwt);
                try
                {
                    using (var cliente = new HttpClient())
                    {
                        var res = new RespuestaApiObjeto();
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        var cts = new CancellationTokenSource();
                        var postBody = JsonConvert.SerializeObject(peticion);
                        var uri = new Uri(string.Format("{0}{1}", url, "Autenticacion/RefreshToken"));
                        var requestContent = new StringContent(postBody, Encoding.UTF8, "application/json");
                        var response = cliente.PostAsync(uri, requestContent).Result;
                        var content = response.Content.ReadAsStringAsync().Result;
                        Debug.WriteLine(content);
                        res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                        if (res != null)
                        {
                            if (res.statusCode == 200)
                            {
                                ////Se guarda el JWt en Session y el RefreshToken en Cookie
                                RespuestaAutenticacion modeloAprobado = res.respuesta.Datos.ToObject<RespuestaAutenticacion>();
                                HttpContext.Current.Session[ConfigurationManager.AppSettings["nameJWT"].ToString()] = modeloAprobado.jwtToken;
                                HttpContext.Current.Session[ConfigurationManager.AppSettings["nameUser"].ToString()] = res.respuesta.idResponse;
                                HttpCookie refreshTokenCookie = new HttpCookie(ConfigurationManager.AppSettings["nameRefreshToken"].ToString());
                                refreshTokenCookie.Value = modeloAprobado.refreshToken;
                                refreshTokenCookie.Expires = DateTime.Now.AddDays(30);
                                refreshTokenCookie.HttpOnly = true;
                                //refreshTokenCookie.Secure = true;
                                HttpContext.Current.Response.SetCookie(refreshTokenCookie);
                                resultado = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return resultado;
        }

        public RespuestaApiLista callWebApiAutorizacionGetListaSincrono(string metodoYAccion)
        {
            var res = new RespuestaApiLista();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        using (var response = cliente.GetAsync(uri).Result)
                        {
                            //var statusCode = response.StatusCode;
                            var content = response.Content.ReadAsStringAsync().Result;
                            Debug.WriteLine(content);
                            res = JsonConvert.DeserializeObject<RespuestaApiLista>(content);
                            if (res != null)
                            {
                                //Si el web api responde 401 no autorizado se hace una refresh token 
                                if (res.statusCode == 401)
                                {
                                    //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                    //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                    var actualizaToken = callWebApiRefreshTokenSincrono();
                                    if (actualizaToken)
                                    {
                                        sessionJwt = HttpContext.Current.Session[nameJwt];
                                        Debug.WriteLine(sessionJwt);
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                        using (var response2 = cliente.GetAsync(uri).Result)
                                        {
                                            content = response2.Content.ReadAsStringAsync().Result;
                                            res = JsonConvert.DeserializeObject<RespuestaApiLista>(content);
                                            if (res.statusCode == 401)
                                            {
                                                res = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        res = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return res;
        }

        public RespuestaApiObjeto callWebApiAutorizacionGetObjetoSincrono(string metodoYAccion)
        {
            var res = new RespuestaApiObjeto();
            try
            {
                var sessionJwt = HttpContext.Current.Session[nameJwt];
                if (sessionJwt != null)
                {
                    using (var cliente = new HttpClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        cliente.Timeout = TimeSpan.FromSeconds(timeoutWebApi);
                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                        var cts = new CancellationTokenSource();
                        var uri = new Uri(string.Format("{0}{1}", url, metodoYAccion));
                        using (var response = cliente.GetAsync(uri).Result)
                        {
                            //var statusCode = response.StatusCode;
                            var content = response.Content.ReadAsStringAsync().Result;
                            Debug.WriteLine(content);
                            res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                            if (res != null)
                            {
                                //Si el web api responde 401 no autorizado se hace una refresh token 
                                if (res.statusCode == 401)
                                {
                                    //Si se pudo hacer el refrsh token se hace una nueva peticion del metodo original
                                    //Si no se devuelve null y se debe iniciar sesion de nuevo.
                                    var actualizaToken = callWebApiRefreshTokenSincrono();
                                    if (actualizaToken)
                                    {
                                        sessionJwt = HttpContext.Current.Session[nameJwt];
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionJwt.ToString());
                                        using (var response2 = cliente.GetAsync(uri).Result)
                                        {
                                            content = response2.Content.ReadAsStringAsync().Result;
                                            res = JsonConvert.DeserializeObject<RespuestaApiObjeto>(content);
                                            if (res.statusCode == 401)
                                            {
                                                res = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        res = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return res;
        }



        public ByteArrayContent convierteModeloAStringContent(string peticionJson, string name)
        {
            var datosContent = new StringContent(peticionJson, Encoding.UTF8, "application/json");
            datosContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            datosContent.Headers.ContentDisposition.Name = name;
            return datosContent;
        }

        public ByteArrayContent convierteHttpPostedFileBaseAByteArrayContent(HttpPostedFileBase file, string name)
        {
            byte[] data;
            var fileName = Path.GetFileName(file.FileName);
            MemoryStream memoryStream = new MemoryStream();
            file.InputStream.CopyTo(memoryStream);
            data = memoryStream.ToArray();
            ByteArrayContent bytecontent = new ByteArrayContent(data);
            bytecontent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            bytecontent.Headers.ContentDisposition.Name = name;
            bytecontent.Headers.ContentDisposition.FileName = fileName;
            bytecontent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            memoryStream.Dispose();
            return bytecontent;
        }

        public ByteArrayContent convierteStringPathAByteArrayContent(string pathFile, string name)
        {
            string mimeType = MimeMapping.GetMimeMapping(pathFile);
            var file = System.IO.File.OpenRead(pathFile);
            byte[] data;
            var fileName = Path.GetFileName(pathFile);
            MemoryStream memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            data = memoryStream.ToArray();
            ByteArrayContent bytecontent = new ByteArrayContent(data);
            bytecontent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            bytecontent.Headers.ContentDisposition.Name = name;
            bytecontent.Headers.ContentDisposition.FileName = fileName;
            bytecontent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            memoryStream.Dispose();
            return bytecontent;
        }


    }
}