using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MoviesAPI;

namespace MoviesAPI.Models;

public partial class ShLabb2webbDbContext : DbContext
{
    public ShLabb2webbDbContext()
    {
    }

    public ShLabb2webbDbContext(DbContextOptions<ShLabb2webbDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer($"Server=tcp:{Environment.GetEnvironmentVariable("SQL_SERVER")},1433;Initial Catalog={Environment.GetEnvironmentVariable("DATABASE")};Persist Security Info=False;User ID={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
