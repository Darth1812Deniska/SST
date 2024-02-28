using Microsoft.EntityFrameworkCore;
using SSTCommonClasses.Infrastructure;
using SSTCommonClasses.Models;
using SSTCommonClasses.Tags;

namespace SqlScriptToolClient;

public class ClientDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<ServerInfo> ServerInfos { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Model> Models { get; set; }
    public DbSet<ModelItem> ModelItems { get; set; }
    public DbSet<SqlTemplate> SqlTemplates { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TagTemplate> TagsTemplates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=SqlScriptToolClient.db");
    }
}