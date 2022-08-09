using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameStore_Web.WebApi.Autenticacion
{
    public class DatosUsuarioRegistro
    {
        public DatosUsuarioRegistro()
        {

        }
        public DatosUsuarioRegistro(int idUsuario, string nombre, string condominio, 
            string edificio, string departamento, string perfil, string correo)
        {
            IdUsuario = idUsuario;
            Nombre = nombre;
            Condominio = condominio;
            Edificio = edificio;
            Departamento = departamento;
            Perfil = perfil;
            Correo = correo;
        }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Condominio { get; set; }
        public string Edificio { get; set; }
        public string Departamento { get; set; }
        public string Perfil { get; set; }
        public string Correo { get; set; }
    }
}