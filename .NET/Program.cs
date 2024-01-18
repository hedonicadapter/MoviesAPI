using System.Collections;
using System.Net.Http;
using System.Reflection;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

builder.Services.AddDbContext<MovieContext>();


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

app.MapGet("/api/movies/findMovieForm", async () =>
{
    var component = @"
    <div class='relative'>
        <form
            hx-get='http://localhost:5275/api/movies/findMovie'
            hx-trigger='submit'
            hx-target='this'
            hx-swap='innerHTML'
        >
            <input type='hidden' name='Id' />
            <div class='flex flex-col space-y-4'>
                <label for='Title'>Title</label>
                <input
                    type='text'
                    name='Title'
                    id='title'
                    class='border border-gray-400 rounded-md p-2'
                />
                <label for='Year'>Year</label>
                <input
                    type='text'
                    name='Year'
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


        string component = $@"
            <form hx-swap='innerHTML' hx-put='http://localhost:5275/api/movies' id='current' class='flex flex-col gap-3 relative'>
                <div id='poster' class='rounded-md outline outline-1'>
                    <img class='w-full h-full' src='{movie.Poster}'/>
                </div>

                <div class='flex flex-col gap-7 child:flex child:flex-col child:gap-2'>
                    <div>
                        <label for='Title'>Title</label>
                        <input class='w-full' type='text' name='Title' value='{movie.Title}'/>
                    </div>

                    <div>
                        <label for='Year'>Year</label>
                        <input class='w-full' type='text' name='Year' value='{movie.Year}'/>
                    </div>
                    <div>
                        <label for='Rated'>Rated</label>
                        <input class='w-full' type='text' name='Rated' value='{movie.Rated}'/>
                    </div>
                    <div>
                        <label for='Plot'>Plot</label>
                        <textarea name='Plot' class='block w-full'>{movie.Plot}</textarea>
                    </div>
                    <div>
                        <label for='Runtime'>Runtime</label>
                        <input class='w-full' type='text' name='Runtime' value='{movie.Runtime}'/>
                    </div>
                    <div>
                        <label for='Genre'>Genre</label>
                        <input class='w-full' type='text' name='Genre' value='{movie.Genre}'/>
                    </div>
                    <div>
                        <label for='Director'>Director</label>
                        <input class='w-full' type='text' name='Director' value='{movie.Director}'/>
                    </div>
                    <div>
                        <label for='Writer'>{(movie.Writer.Contains(',') ? "Writers" : "Writer")}</label>
                        <input class='w-full' type='text' name='Writer' value='{movie.Writer}'/>
                    </div>
                    <div>
                        <label for='Actors'>Actors</label>
                        <input class='w-full' type='text' name='Actors' value='{movie.Actors}'/>
                    </div>
                    <div>
                        <label for='Poster'>Poster</label>
                        <input class='w-full break-all' type='text' name='Poster' value='{movie.Poster}'/>
                    </div>
                    <div>
                        <label for='Metascore'>Metascore</label>
                        <input class='w-full' type='text' name='Metascore' value='{movie.Metascore}'/>
                    </div>
                    <div>
                        <label for='Rating'>Rating</label>
                        <input class='w-full' type='text' name='IMDbRating' value='{movie.IMDbRating}'/>
                    </div>
                    <div>
                        <label for='IMDbId'>IMDbId</label>
                        <input class='w-full' type='text' name='IMDbId' value='{movie.IMDbId}'/>
                    </div>
                </div>

                <div class='sticky w-screen bottom-0 pb-8 gap-3 flex flex-row justify-evenly items-center'>
                    <a hx-target='#current' hx-get='http://localhost:5275/api/movies/findMovieForm' class='rounded-full outline outline-2 px-5 pb-2 pt-1'>Go back</a>
                    <button type='submit' class='rounded-full outline outline-2 px-5 pb-2 pt-1'>{(res != null ? "Save changes" : "Add movie")}</button>
                </div>
            </form>
            ";

        return Results.Content(component, "text/html");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/movies", async (HttpContext httpContext, MovieContext dbContext) =>
{
    try
    {
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

        var existingRecord = await dbContext.Movies.Where(m => m.Title == newMovie.Title && m.Year == newMovie.Year && m.Director == newMovie.Director).FirstOrDefaultAsync();

        if (existingRecord != null)
        {
            foreach (PropertyInfo prop in newMovie.GetType().GetProperties())
            {
                if (prop.GetValue(newMovie) == null) continue;
                prop.SetValue(existingRecord, prop.GetValue(newMovie));
            }

            dbContext.Update(existingRecord);
        }
        else
        {
            newMovie.Id = Guid.NewGuid();
            dbContext.Movies.Add(newMovie);
        }


        await dbContext.SaveChangesAsync();

        return Results.Created($"/api/movies/{newMovie.Id}", newMovie);
        // return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// app.MapPatch("/api/movies/{id}", async (HttpContext httpContext,, MovieContext dbContext, string id) =>{
// });



// app.MapGet("/api/movies/{id}", async (HttpContext httpContext,,MovieContext dbContext, string id) =>{
// });



app.MapGet("/api/movies/random", async ([FromServices] MovieContext dbContext) =>
{
    var randomMovie = await dbContext.Movies.OrderBy(r => Guid.NewGuid()).Take(1).FirstOrDefaultAsync();
    if (randomMovie == null) return Results.NotFound();

    var component = $@"
    <div id='current' class='flex flex-col gap-3 relative'>
        <div class='flex flex-row justify-between gap-3 items-baseline'>
            <h1>{randomMovie.Title}</h1>
            <p>{randomMovie.Year}</p>
        </div>
        <div id='poster' class='rounded-md outline outline-1'>
            <img class='w-full h-full' src='{randomMovie.Poster}'/>
        </div>
        <p class='pt-3 pb-5'>{randomMovie.Plot}</p>

        <div class='flex flex-col gap-3'>
            <div class='flex flex-row justify-between gap-3'>
                <p>Title</p>
                <p>{randomMovie.Title}</p>
            </div>

            <div class='flex flex-row justify-between gap-3'>
                <p>Year</p>
                <p>{randomMovie.Year}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>Rated</p>
                <p>{randomMovie.Rated}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>Runtime</p>
                <p>{randomMovie.Runtime}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>Genre</p>
                <p>{randomMovie.Genre}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>Director</p>
                <p>{randomMovie.Director}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>{(randomMovie.Writer.Contains(',') ? "Writers" : "Writer")}</p>
                <p>{randomMovie.Writer}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>Actors</p>
                <p>{randomMovie.Actors}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>Plot</p>
                <p>{randomMovie.Plot}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>Poster</p>
                <p class='break-all'>{randomMovie.Poster}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>Metascore</p>
                <p>{randomMovie.Metascore}</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>IMDb rating</p>
                <p>{randomMovie.IMDbRating} ({randomMovie.IMDbVotes} votes)</p>
            </div>
            <div class='flex flex-row justify-between gap-3'>
                <p>IMDb ID</p>
                <p>{randomMovie.IMDbId}</p>
            </div>
        </div>";

    return Results.Content(component, "text/html");
});




//                 <div class='sticky w-screen bottom-0 pb-8 gap-3 flex flex-row justify-evenly items-center'>
//                     <a hx-target='#current' hx-get='http://localhost:5275/api/movies/findMovieForm' class='rounded-full outline outline-2 px-5 pb-2 pt-1'>Go back</a>
//                     <a hx-swap='innerHTML' hx-post='http://localhost:5275/api/movies/' class='rounded-full outline outline-2 px-5 pb-2 pt-1'>Add movie</a>
//                 </div>
//             </div>"


app.Run();