using System.Collections;
using System.Net.Http;
using System.Reflection;
using Htmx;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<MovieContext>();


var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();


app.MapGet("/api/movies/findMovie", async (MovieContext dbContext, string title, string? year) =>
{
    var movie = new Movie();

    try
    {
        var res = await dbContext.Movies.Where(m => m.Title == title && m.Year == year).FirstOrDefaultAsync();

        if (res != null)
        {
            movie = res;
        }
        else
        {
            using var client = new HttpClient();
            string queryParams = $"t={title}";
            if (year != null) queryParams += $"&y={year}";

            var response = await client.GetAsync($"http://www.omdbapi.com/?{queryParams}&apikey={Environment.GetEnvironmentVariable("OMDB_API_KEY")}");
            movie = await response.Content.ReadFromJsonAsync<Movie>();
            if (movie == null) return Results.BadRequest();
        }

        return Results.Ok(movie);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/movies", async (HttpContext httpContext) =>
{
    try
    {
        var dbContext = httpContext.RequestServices.GetRequiredService<MovieContext>();
        var formData = await httpContext.Request.ReadFormAsync();
        var newMovie = new Movie();

        newMovie.Title = formData["Title"];
        newMovie.Year = formData["Year"];
        newMovie.Rated = formData["Rated"];
        newMovie.Plot = formData["Plot"];
        newMovie.Runtime = formData["Runtime"];
        newMovie.Genre = formData["Genre"];
        newMovie.Director = formData["Director"];
        newMovie.Writer = formData["Writer"];
        newMovie.Actors = formData["Actors"];
        newMovie.Poster = formData["Poster"];
        newMovie.Metascore = formData["Metascore"];
        newMovie.IMDbId = formData["IMDbId"];
        newMovie.IMDbRating = formData["IMDbRating"];
        newMovie.IMDbVotes = formData["IMDbVotes"];

        var existingRecord = await dbContext.Movies.Where(m => m.Title == newMovie.Title && m.Year == newMovie.Year && m.Director == newMovie.Director).FirstOrDefaultAsync();

        if (existingRecord != null)
        {
            foreach (PropertyInfo prop in newMovie.GetType().GetProperties())
            {
                if (prop.GetValue(newMovie) == null) continue;
                prop.SetValue(existingRecord, prop.GetValue(newMovie));
            }

            dbContext.Update(existingRecord);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        }
        else
        {
            newMovie.Id = Guid.NewGuid();
            dbContext.Movies.Add(newMovie);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/movies/{newMovie.Id}", newMovie);
        }

    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapDelete("/api/movies/{id}", async (MovieContext dbContext, string id) =>
{
    try
    {
        var movie = await dbContext.Movies.Where(m => m.Id.ToString() == id).FirstOrDefaultAsync();
        if (movie == null) return Results.NotFound();

        dbContext.Movies.Remove(movie);
        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});



app.MapGet("/api/movies/{id}", async (MovieContext dbContext, string id) =>
{
    try
    {
        var movie = await dbContext.Movies.Where(m => m.Id.ToString() == id).FirstOrDefaultAsync();
        if (movie == null) return Results.NotFound();

        return Results.Ok(movie);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});


app.MapGet("/api/movies/random", async ([FromServices] MovieContext dbContext) =>
{
    try
    {
        var randomMovie = await dbContext.Movies.OrderBy(r => Guid.NewGuid()).Take(1).FirstOrDefaultAsync();
        if (randomMovie == null) return Results.NotFound();


        return Results.Ok(randomMovie);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.Run();