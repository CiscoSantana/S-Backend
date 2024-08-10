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

//Registro das dependências/serviços
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

//Configuração do cors para aceitar conexão do swagger e do insomnia 
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
    //define o esquema padrão de autenticação, faz com que o middleware procure token jwt nas requisições http
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;    
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.UseSecurityTokenValidators = true;//Ativa o uso de validadores de token de segurança. Assegura que o token é valido
    options.RequireHttpsMetadata = false;//Indica se os metadados do token podem ser acessados via HTTPS. Deixei false por ser debug
    options.SaveToken = true;//Indica se o token deve ser salvo depois da autenticação. Útil para acessar posterioremente.
    options.TokenValidationParameters = new TokenValidationParameters//Define como o token deve ser avaliado
    {
        ValidateIssuerSigningKey = true,//Impede que token falsificado sejam aceitos
        IssuerSigningKey = new SymmetricSecurityKey(key),//Deve ser utilizada a mesma chave usada para assinar o token
        ValidateIssuer = false,// Para prod deve ser true para saber se o token é de uma fonte confiável.
        ValidateAudience = false,//Para prod deve ser true, para saber se o token realmente é para essa aplicação
        ClockSkew = TimeSpan.Zero//Define a margem de erro de validação quando o token expira, no caso não há margem de erro 
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>//Acionado quando o token é recebido
        {
            Console.WriteLine($"Token received: {context.Token}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>//Acionado quando a autenticação falha
        {
            Console.WriteLine("Authentication failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>//Acionado quando o token é validado com sucesso.
        {
            Console.WriteLine("Token validated: " + context.SecurityToken);
            return Task.CompletedTask;
        }
    };
});

// Registrando o serviço hospedado em background, 
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
