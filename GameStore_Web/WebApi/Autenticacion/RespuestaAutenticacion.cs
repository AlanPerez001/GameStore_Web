using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameStore_Web.WebApi.Autenticacion
{
    public class RespuestaAutenticacion
    {
        public string jwtToken { get; set; }
        public string refreshToken { get; set; }
    }
}