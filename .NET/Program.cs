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

app.MapPost("/api/findMovie", async (HttpContext httpContext) =>{
    var form = await httpContext.Request.ReadFormAsync();
    var title = form["title"];
    var year = form["year"];
    using var client = new HttpClient();
    
    try
    {
        var response = await client.GetAsync($"http://www.omdbapi.com/?t={title}&y={year}&apikey={Environment.GetEnvironmentVariable("OMDB_API_KEY")}");
        var movie = await response.Content.ReadFromJsonAsync<Movie>();
        if (movie == null) return Results.BadRequest();

        string component = $@"<div>
                <div class='flex flex-row justify-between gap-3 items-baseline'>
                    <h1>{movie.Title}</h1>
                    <p>{movie.Year}</p>
                </div>
                <div id='poster' class='py-5 px-4 rounded-md outline outline-1'>
                    <img src='{movie.Poster}'/>
                </div>
                <p>{movie.Plot}</p>
            </div>";

        return Results.Content(component, "text/html");
    }
    catch
    {
        return Results.BadRequest();
    }
});





app.Run();