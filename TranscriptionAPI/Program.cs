using Microsoft.AspNetCore.Identity;
using TranscriptionAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using TranscriptionAPI.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseInMemoryDatabase("AppDb"));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITranscriptionService, TranscriptionService>();
builder.Services.AddAuthorization();
builder.Services.AddIdentity<IdentityUser, IdentityRole>() // Hier wurde IdentityApiEndpoints durch AddIdentity ersetzt
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Hinzugefügt, um DbContext für Identity hinzuzufügen



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapIdentityApi<IdentityUser>();

app.MapGet("/TranscriptionAPI", (HttpContext httpContext) =>
{
    var transcriptionData = new Transcription()
    {
        Date = DateTime.Now,
        TranscriptionLines = new List<TranscriptionData>
        {
            new TranscriptionData() { StartSeconds = 0, EndSeconds = 2, Transcript = "Lorem ipsum dolor sit amet, consetetur sad" },
            new TranscriptionData() { StartSeconds = 2, EndSeconds = 5, Transcript = "cusam et justo duo dolores et ea rebum. Stet clita kasd" },
            new TranscriptionData() { StartSeconds = 5, EndSeconds = 2, Transcript = "dolor sit amet. Lorem ipsum dolor sit am" }
        }
    };
    return transcriptionData;
})
.WithOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
