using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameStore_Web.Models.Catalogo
{
    public class RespuestaObtenerVideoJuegos
    {
        public int idResponse { get; set; }
        public string Response { get; set; }
        public List<VideoJuegos> Datos { get; set; }
    }

    public class VideoJuegos
    {
        public int idJuego { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string AnioPublicacion { get; set; }
        public int Calificacion { get; set; }
        public string Consolas { get; set; }
        public string Genero { get; set; }
    }
    public class MVReporte
    {
        public RespuestaObtenerVideoJuegos RespuestaVideoJuegos { get; set; }
        public List<VideoJuegos> CatVideoJuegos { get; set; }
        public MVDetalleJuego Detalles { get; set; }
        public MVFiltroGenero FiltroGenero { get; set; }
        public MVFiltroConsola FiltroConsola { get; set; }
    }
    public class MVDetalleJuego
    {
        public int idJuego { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string AnioPublicacion { get; set; }
        public int Calificacion { get; set; }
        public string Consolas { get; set; }
        public int idConsola { get; set; }
        public string Genero { get; set; }
        public int idGenero { get; set; }
    }
    public class MVFiltroGenero
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
    }
    public class MVFiltroConsola
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
    }
}

