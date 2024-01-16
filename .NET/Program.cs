using System.Net.Http;
using Htmx;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});


var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseCors();

app.MapRazorPages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("/api/movies/findMovieForm", async() => {
    var component = @"
    <div class='relative'>
        <form
            hx-get='http://localhost:5275/api/movies/findMovie'
            hx-trigger='submit'
            hx-target='this'
            hx-swap='innerHTML'
        >
            <input type='hidden' name='id' />
            <div class='flex flex-col space-y-4'>
                <label for='title'>Title</label>
                <input
                    type='text'
                    name='title'
                    id='title'
                    class='border border-gray-400 rounded-md p-2'
                />
                <label for='year'>Year</label>
                <input
                    type='text'
                    name='year'
                    id='year'
                    class='border border-gray-400 rounded-md p-2'
                />

                <button
                    type='submit'
                    class='bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded'
                >
                    Search
                </button>
            </div>
        </form>
    </div>
    ";

    return Results.Content(component, "text/html");
});

app.MapGet("/api/movies/findMovie", async (string title, string year) =>{
    using var client = new HttpClient();
    
    try
    {
        var response = await client.GetAsync($"http://www.omdbapi.com/?t={title}&y={year}&apikey={Environment.GetEnvironmentVariable("OMDB_API_KEY")}");
        var movie = await response.Content.ReadFromJsonAsync<Movie>();
        if (movie == null) return Results.BadRequest();

        string component = $@"
            <div id='current' class='flex flex-col gap-3 relative'>
                <div class='flex flex-row justify-between gap-3 items-baseline'>
                    <h1>{movie.Title}</h1>
                    <p>{movie.Year}</p>
                </div>
                <div id='poster' class='rounded-md outline outline-1'>
                    <img class='w-full h-full' src='{movie.Poster}'/>
                </div>
                <p class='pt-3 pb-5'>{movie.Plot}</p>

                <div class='flex flex-col gap-3'>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Title</p>
                        <p>{movie.Title}</p>
                    </div>

                    <div class='flex flex-row justify-between gap-3'>
                        <p>Year</p>
                        <p>{movie.Year}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Rated</p>
                        <p>{movie.Rated}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Released</p>
                        <p>{movie.Released}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Runtime</p>
                        <p>{movie.Runtime}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Genre</p>
                        <p>{movie.Genre}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Director</p>
                        <p>{movie.Director}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>{(movie.Writer.Contains(',') ? "Writers" : "Writer")}</p>
                        <p>{movie.Writer}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Actors</p>
                        <p>{movie.Actors}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Plot</p>
                        <p>{movie.Plot}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Poster</p>
                        <p class='break-all'>{movie.Poster}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Ratings</p>
                        <p>{movie.Ratings}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>Metascore</p>
                        <p>{movie.Metascore}</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>IMDb rating</p>
                        <p>{movie.imdbRating} ({movie.imdbVotes} votes)</p>
                    </div>
                    <div class='flex flex-row justify-between gap-3'>
                        <p>imdbID</p>
                        <p>{movie.imdbID}</p>
                    </div>
                </div>

                <div class='sticky w-screen bottom-0 pb-8 gap-3 flex flex-row justify-evenly items-center'>
                    <a hx-target='#current' hx-get='http://localhost:5275/api/movies/findMovieForm' class='rounded-full outline outline-2 px-5 pb-2 pt-1'>Go back</a>
                    <a hx-swap='innerHTML' hx-post='http://localhost:5275/api/movies/' class='rounded-full outline outline-2 px-5 pb-2 pt-1'>Add movie</a>
                </div>
            </div>";

        return Results.Content(component, "text/html");
    }
    catch
    {
        return Results.BadRequest();
    }
});

// app.MapPost("/api/movies", async (HttpContext httpContext, MovieContext dbContext) =>{
//     try{
//         Movie newMovie = (Movie)await httpContext.Request.ReadFormAsync();

//         newMovie.Id = Guid.NewGuid();

//         dbContext.Movies.Add(newMovie);
//         await dbContext.SaveChangesAsync();
        
//         return Results.Created($"/api/movies/{newMovie.Id}", newMovie);
//     } catch {
//         return Results.BadRequest();
//     }
    
// });

// app.MapPatch("/api/movies/{id}", async (HttpContext httpContext, MovieContext dbContext, string id) =>{
// });

// app.MapGet("/api/movies/{id}", async (HttpContext httpContext, MovieContext dbContext, string id) =>{
// });

// app.MapGet("/api/movies/random", async (HttpContext httpContext, MovieContext dbContext) =>{

// });





app.Run();