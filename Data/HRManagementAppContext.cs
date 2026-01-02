using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRManagementApp.Models;

namespace HRManagementApp.Data
{
    public class HRManagementAppContext : DbContext
    {
        public HRManagementAppContext (DbContextOptions<HRManagementAppContext> options)
            : base(options)
        {
        }

        public DbSet<HRManagementApp.Models.Employee> Employee { get; set; } = default!;
    }
}
