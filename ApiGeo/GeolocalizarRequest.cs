using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGeo
{
    public class GeolocalizarRequest
    {
        public string calle { get; set; }
        public string numero { get; set; }
        public string ciudad { get; set; }
        public string codigo_postal { get; set; }
        public string provincia { get; set; }
        public string pais { get; set; }
    }

    public class GeolocalizarResponse
    {
        public int Id;
    }
}
