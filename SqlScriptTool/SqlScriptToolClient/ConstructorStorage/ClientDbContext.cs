using Microsoft.EntityFrameworkCore;
using SSTCommonClasses.Infrastructure;
using SSTCommonClasses.Models;
using SSTCommonClasses.Tags;
using System;
using System.IO;

namespace SqlScriptToolClient.ConstructorStorage;

public class ClientDbContext : DbContext
{
    private const string ClientDbName = "SqlScriptToolClient.db";
    private static string DbPath { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ServerInfo> ServerInfos { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Model> Models { get; set; }
    public DbSet<ModelItem> ModelItems { get; set; }
    public DbSet<SqlTemplate> SqlTemplates { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TagTemplate> TagsTemplates { get; set; }
    public ClientDbContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var appDataFolderPath = AppDomain.CurrentDomain.BaseDirectory;
        DbPath = Path.Join(appDataFolderPath, ClientDbName);
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}