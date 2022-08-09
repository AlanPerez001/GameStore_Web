using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameStore_Web.WebApi.Autenticacion
{
    public class WAPeticionLogin
    {
        public WAPeticionLogin(string Usuario, string Password, string App, string Version, string TokenApp, string Plataforma, string IpCliente)
        {
            this.Usuario = Usuario;
            this.Password = Password;
            this.App = App;
            this.Version = Version;
            this.TokenApp = TokenApp;
            this.Plataforma = Plataforma;
            this.IpCliente = IpCliente;
        }

        public string Usuario { get; set; }
        public string Password { get; set; }
        public string App { get; set; }
        public string Version { get; set; }
        public string TokenApp { get; set; }
        public string Plataforma { get; set; }
        public string IpCliente { get; set; }
    }
}