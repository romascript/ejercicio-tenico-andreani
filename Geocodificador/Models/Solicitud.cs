using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Geocodificador.Models
{
    public class Solicitud
    {
        public int Id { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Estado { get; set; }
        public string Calle { get; set; }
        public string Numero { get; set; }
        public string Ciudad { get; set; }
        public string Codigo_Postal { get; set; }
        public string Provincia { get; set; }
        public string Pais { get; set; }
        public DateTime At_Created { get; set; }
    }
}
