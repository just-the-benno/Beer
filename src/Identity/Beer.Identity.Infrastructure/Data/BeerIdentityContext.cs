using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.Identity.Infrastructure.Data
{
    public class BeerIdentityContext : IdentityDbContext<BeerUser>
    {
        public BeerIdentityContext(DbContextOptions<BeerIdentityContext> options)
            : base(options)
        {
        }
    }
   
}
