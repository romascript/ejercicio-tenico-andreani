using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGeo.Util
{
    public class ParamVerify
    {
        public void requiredParam(string param, string msj)
        {
            if (param == null || param == "")
                throw new Exception(msj);
        }
    }
}
