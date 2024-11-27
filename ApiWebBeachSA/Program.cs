using ApiWebBeachSA.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

/*var cultureInfo = new CultureInfo("es-CR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;*/

// Add services to the container.

builder.Services.AddControllers();

//se agrega la inyeccion de dependencias al ORM EntityFramework core que implementa 
builder.Services.AddScoped<ApiWebBeachSA.Data.DbContextHotel>();

//Agregar el servicio del ORM con su DbContext ademas su string conexion
builder.Services.AddDbContext<ApiWebBeachSA.Data.DbContextHotel>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("StringConexion")));


//Se agrega el servicio de JWT, con la interface y el objeto que la implementa 
builder.Services.AddScoped<IAutorizacionServices, AutorizacionServices>();
builder.Services.AddScoped<IAutorizacionServicesEmpleado, AutorizacionServicesEmpleado>();

//se procede con la configuracion para el esquema de autenticacion
var key = builder.Configuration.GetValue<string>("JwtSettings:Key");
var keyBytes = Encoding.ASCII.GetBytes(key);

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
