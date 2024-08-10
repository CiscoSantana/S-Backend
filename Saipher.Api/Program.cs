using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Saipher.Application.Interfaces;
using Saipher.Application.Services;
using Saipher.Domain.Interfaces;
using Saipher.Infrastructure.Data;
using Saipher.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection")));

//Registro das depend�ncias/servi�os
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Saipher API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    c.EnableAnnotations();
});

//Configura��o do cors para aceitar conex�o do swagger e do insomnia 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSecret"]);
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthentication(options =>
{
    //define o esquema padr�o de autentica��o, faz com que o middleware procure token jwt nas requisi��es http
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;    
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.UseSecurityTokenValidators = true;//Ativa o uso de validadores de token de seguran�a. Assegura que o token � valido
    options.RequireHttpsMetadata = false;//Indica se os metadados do token podem ser acessados via HTTPS. Deixei false por ser debug
    options.SaveToken = true;//Indica se o token deve ser salvo depois da autentica��o. �til para acessar posterioremente.
    options.TokenValidationParameters = new TokenValidationParameters//Define como o token deve ser avaliado
    {
        ValidateIssuerSigningKey = true,//Impede que token falsificado sejam aceitos
        IssuerSigningKey = new SymmetricSecurityKey(key),//Deve ser utilizada a mesma chave usada para assinar o token
        ValidateIssuer = false,// Para prod deve ser true para saber se o token � de uma fonte confi�vel.
        ValidateAudience = false,//Para prod deve ser true, para saber se o token realmente � para essa aplica��o
        ClockSkew = TimeSpan.Zero//Define a margem de erro de valida��o quando o token expira, no caso n�o h� margem de erro 
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>//Acionado quando o token � recebido
        {
            Console.WriteLine($"Token received: {context.Token}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>//Acionado quando a autentica��o falha
        {
            Console.WriteLine("Authentication failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>//Acionado quando o token � validado com sucesso.
        {
            Console.WriteLine("Token validated: " + context.SecurityToken);
            return Task.CompletedTask;
        }
    };
});

// Registrando o servi�o hospedado em background, 
builder.Services.AddHostedService<UserCleanupService>();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Saipher API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
