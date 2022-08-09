using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameStore_Web.Models.Catalogo
{
    public class PeticionGuardarRegistro
    {
        public int IdJuego { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string AnioPublicacion { get; set; }
        public int Calificacion { get; set; }
        public int idConsola { get; set; }
        public int idGenero { get; set; }
    }
}