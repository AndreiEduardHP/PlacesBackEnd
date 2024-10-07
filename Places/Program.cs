using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Places.Data;
using Places.Interfaces;
using Places.Repository;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using SignalRChat.Hubs;
using Stripe;
using AutoMapper;
using Places.Helpers;
using Places.MapperConfig;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"];
StripeConfiguration.ApiKey = "sk_test_51Op4EpBTlGDnVojp7fAOStbwP6UWDhCuTl7lYanpTB84WwB9QATjChhePSWyedyn0nrukU741EdMCahZRNphF2nJ00dko64UCH";

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Parse("192.168.1.228"), 8080); //192.168.1.6 //10.158.5.58 //10.167.6.133 //192.168.1.5
});

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  // .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));


// Add services to the container.


builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IChatRepository, ChatsRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserProfileEventRepository, UserProfileEventRepository>();
builder.Services.AddScoped<FriendService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PlacesContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PlacesConnectionString"));
});
builder.Services.AddCors(policyBuilder =>
    policyBuilder.AddDefaultPolicy(policy =>
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyHeader())
);
builder.Services.AddAutoMapper(typeof(EventMapper));
builder.Services.AddAutoMapper(typeof(EventAlbumImagesMapper));

// Utilizați o cheie secretă sigură

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
         ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();
app.UseCors();

app.UseAuthentication();
//app.UseAuthorization();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  //app.UseSwagger();
//app.UseSwaggerUI();
}
app.MapHub<ChatHub>("/chatHub");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


