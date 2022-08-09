using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GameStore_Web.Models
{
    public class MVLogin
    {
        public ModeloLogin modeloVistaLogin { get; set; }
        public MVContacto modeloVistaContacto { get; set; }



    }
    public class ModeloLogin
    {
        [Required]
        [Display(Name = "Usuario/Correo")]
        //[EmailAddress]
        public string Correo { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }
    }

    public class MVContacto
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; }
        [Required]
        [Display(Name = "Usuario/Correo")]
        //[EmailAddress]
        public string Correo { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Telefono")]
        public string Telefono { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Edificio { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Mensaje { get; set; }
    }

}