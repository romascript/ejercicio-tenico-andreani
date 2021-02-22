using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGeo.Data
{
    public class SolicitudContext : DbContext
    {
        public SolicitudContext(DbContextOptions<SolicitudContext> option):base(option) { }
        public DbSet<Models.Solicitud> SolicitudItems {get; set;}
    }
}
