using Microsoft.EntityFrameworkCore;

public class Movie
{
    public Guid? Id { get; set; }
    public string Title { get; set; }
    public string Year { get; set; }
    public string Rated { get; set; }
    public string? Runtime { get; set; } = "N/A";
    public string Genre { get; set; }
    public string Director { get; set; }
    public string Writer { get; set; }
    public string? Actors { get; set; }
    public string? Plot { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
    public string Poster { get; set; }
    public string? Metascore { get; set; }
    public string? IMDbRating { get; set; }
    public string? IMDbVotes { get; set; }
    public string? IMDbId { get; set; }
}

public class MovieContext : DbContext
{
    public DbSet<Movie> Movies { get; set; }

    public MovieContext(DbContextOptions<MovieContext> options)
        : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer($"Server=tcp:{Environment.GetEnvironmentVariable("SQL_SERVER")},1433;Initial Catalog={Environment.GetEnvironmentVariable("DATABASE")};Persist Security Info=False;User ID={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
    }
}