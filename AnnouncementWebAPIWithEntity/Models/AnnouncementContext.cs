using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnouncementWebAPIWithEntity.Models
{
    public class AnnouncementsContext:DbContext
    {
        public DbSet<Announcement> Announcements { get; set; }
        public AnnouncementsContext(DbContextOptions<AnnouncementsContext> options)
            : base(options)
        {
            //use if database created for new propert in datamodel
           // Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Database=AnnouncementsDb;Trusted_Connection=True;");
        //}

    }
}
