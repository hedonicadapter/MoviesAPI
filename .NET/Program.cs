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
        <form
            hx-get='http://localhost:5275/api/movies/findMovie'
            hx-trigger='submit'
            hx-target='#main-component'
            hx-swap='innerHTML'
        >
            <div class='flex flex-col gap-7'>
                <input type='hidden' title='Id' id='Id' name='Id' />
                <label for='Title'>Title</label>
                <input
                    type='text'
                    title='Title' id='Title' name='Title'
                    id='title'
                />
                <label for='Year'>Year</label>
                <input
                    type='text'
                    title='Year' id='Year' name='Year'
                    id='year'
                />

                <button
                    type='submit'
                    class='button'
                >
                    Search
                </button>
            </div>
        </form>
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

        string style = @"<style>
                @media (min-width: 768px) {
                    main {
                        padding-right:0 !important;
                    }
                }
            </style>";

        string component = $@"
            <div class='flex flex-col md:flex-row md:gap-10 md:justify-center md:items-center'>
                <div class='md:pb-3'>
                    <img class='rounded-md outline outline-1 md:min-w-[100%]' src='{movie.Poster}'/>
                </div>
                <form class='md:h-[calc(100vh-6rem)] md:pr-[14vw] md:overflow-y-auto md:py-12 md:mt-24 md:w-full' hx-swap='innerHTML' hx-target='#main-component' hx-put='http://localhost:5275/api/movies'>
                    <div class='flex flex-col gap-7 child:flex child:flex-col child:gap-2'>
                        <label for='Title'>Title</label>
                        <input type='text' title='Title' id='Title' name='Title' value='{movie.Title}'/>
                        
                        <label for='Year'>Year</label>
                        <input type='text' title='Year' id='Year' name='Year' value='{movie.Year}'/>

                        <label for='Rated'>Rated</label>
                        <input type='text' title='Rated' id='Rated' name='Rated' value='{movie.Rated}'/>

                        <label for='Plot'>Plot</label>
                        <textarea title='Plot' id='Plot' name='Plot' class=>{movie.Plot}</textarea>

                        <label for='Runtime'>Runtime</label>
                        <input type='text' title='Runtime' id='Runtime' name='Runtime' value='{movie.Runtime}'/>

                        <label for='Genre'>Genre</label>
                        <input type='text' title='Genre' id='Genre' name='Genre' value='{movie.Genre}'/>

                        <label for='Director'>Director</label>
                        <input type='text' title='Director' id='Director' name='Director' value='{movie.Director}'/>

                        <label for='Writer'>{(movie.Writer.Contains(',') ? "Writers" : "Writer")}</label>
                        <input type='text' title='Writer' id='Writer' name='Writer' value='{movie.Writer}'/>

                        <label for='Actors'>Actors</label>
                        <input type='text' title='Actors' id='Actors' name='Actors' value='{movie.Actors}'/>

                        <label for='Poster'>Poster</label>
                        <input class='break-all' type='text' title='Poster' id='Poster' name='Poster' value='{movie.Poster}'/>

                        <label for='Metascore'>Metascore</label>
                        <input type='text' title='Metascore' id='Metascore' name='Metascore' value='{movie.Metascore}'/>

                        <label for='Rating'>Rating</label>
                        <input type='text' title='IMDbRating' id='IMDbRating' name='IMDbRating' value='{movie.IMDbRating}'/>

                        <input type='hidden' value='{movie.IMDbVotes}' title='IMDbVotes' id='IMDbVotes' name='IMDbVotes'/>

                        <label for='IMDbId'>IMDbId</label>
                        <input type='text' title='IMDbId' id='IMDbId' name='IMDbId' value='{movie.IMDbId}'/>
                    </div>

                    <div class='sticky bottom-0 pb-8 gap-3 flex flex-row justify-end items-center'>
                        <a hx-target='#main-component' hx-get='http://localhost:5275/api/movies/findMovieForm' class='button'>Go back</a>
                        <button type='submit' class='button'>{(res != null ? "Save changes" : "Add movie")}</button>
                    </div>
                </form>
                {style}
            </div>
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
    var movie = await dbContext.Movies.Where(m => m.Id.ToString() == id).FirstOrDefaultAsync();
    if (movie == null) return Results.NotFound();

    return Results.Ok(movie);
});



app.MapGet("/api/movies/random", async ([FromServices] MovieContext dbContext) =>
{
    var randomMovie = await dbContext.Movies.OrderBy(r => Guid.NewGuid()).Take(1).FirstOrDefaultAsync();
    if (randomMovie == null) return Results.NotFound();


    string style = @"<style>
            @media (min-width: 768px) {
                main {
                    padding-right:0 !important;
                }
            }
        </style>";

    var component = $@"
    <div class='flex flex-col md:flex-row md:gap-10 md:justify-center md:items-center'>
        <div class='md:pb-3'>
            <div class='flex flex-row justify-between gap-3 items-baseline'>
                <h1>{randomMovie.Title}</h1>
                <p>{randomMovie.Year}</p>
            </div>
            <img class='rounded-md outline outline-1 md:min-w-[100%]' src='{randomMovie.Poster}'/>
        </div>

        <div class='md:h-[calc(100vh-6rem)] md:pr-[14vw] md:overflow-y-auto md:py-12 md:mt-24 md:w-full'>
            <div id='current' class='flex flex-col gap-3 relative'>
                <p class='pt-3 pb-5'>{randomMovie.Plot}</p>

                <div class='flex flex-col gap-3 child:flex child:flex-row child:justify-between child:gap-3'>
                    <div>
                        <p>Title</p>
                        <p>{randomMovie.Title}</p>
                    </div>

                    <div>
                        <p>Year</p>
                        <p>{randomMovie.Year}</p>
                    </div>
                    <div>
                        <p>Rated</p>
                        <p>{randomMovie.Rated}</p>
                    </div>
                    <div>
                        <p>Runtime</p>
                        <p>{randomMovie.Runtime}</p>
                    </div>
                    <div>
                        <p>Genre</p>
                        <p>{randomMovie.Genre}</p>
                    </div>
                    <div>
                        <p>Director</p>
                        <p>{randomMovie.Director}</p>
                    </div>
                    <div>
                        <p>{(randomMovie.Writer.Contains(',') ? "Writers" : "Writer")}</p>
                        <p>{randomMovie.Writer}</p>
                    </div>
                    <div>
                        <p>Actors</p>
                        <p>{randomMovie.Actors}</p>
                    </div>
                    <div>
                        <p>Metascore</p>
                        <p>{randomMovie.Metascore}</p>
                    </div>
                    <div>
                        <p>IMDb rating</p>
                        <p>{randomMovie.IMDbRating} ({randomMovie.IMDbVotes} votes)</p>
                    </div>
                    <div>
                        <p>IMDb ID</p>
                        <p>{randomMovie.IMDbId}</p>
                    </div>
                </div>
            </div>
        </div>
        {style}
    </div>
    ";

    return Results.Content(component, "text/html");
});




//                 <div class='sticky w-screen bottom-0 pb-8 gap-3 flex flex-row justify-evenly items-center'>
//                     <a hx-target='#current' hx-get='http://localhost:5275/api/movies/findMovieForm' class='rounded-full outline outline-2 px-5 pb-2 pt-1'>Go back</a>
//                     <a hx-swap='innerHTML' hx-post='http://localhost:5275/api/movies/' class='rounded-full outline outline-2 px-5 pb-2 pt-1'>Add movie</a>
//                 </div>
//             </div>"


app.Run();