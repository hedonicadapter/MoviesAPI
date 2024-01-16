using System.Net.Http;


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


var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("/allo", async () =>
{
    return Results.Content("<div>Hello World!</div>", "text/html");
    // using (var client = new HttpClient()){
    //     var response = await client.GetAsync("https://localhost:5001/api/Movies/GetRandomMovie");
    //     var content = await response.Content.ReadAsStringAsync();
        
    //     return Results.Ok(content);
    // }

});





app.Run();