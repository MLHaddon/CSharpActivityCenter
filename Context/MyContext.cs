using ExamProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamProject.Context
{
    public class MyContext : DbContext 
    {
        public MyContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users {get; set;}
        public DbSet<DojoActivity> DojoActivities {get; set;}
        public DbSet<Association> Associations {get; set;}
    }
}