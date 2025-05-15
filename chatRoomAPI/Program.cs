using ChatRoomAPI.TokenService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Front Site ����
    options.SwaggerDoc("front", new OpenApiInfo
    {
        Version = "v1",
        Title = "Front Site API",
        Description = "API for Front-end Site"
    });

    // Back Site ����
    options.SwaggerDoc("back", new OpenApiInfo
    {
        Version = "v1",
        Title = "Back Site API",
        Description = "API for Back-end Site"
    });

    // �ھڱ���W�� GroupName �ӨM�w�k�ݭ��@�� SwaggerDoc
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var groupName = apiDesc.GroupName ?? "";
        return docName.Equals(groupName, StringComparison.OrdinalIgnoreCase);
    });

    // �Y�A�Ʊ�C�� Controller ��ܦۤv�b Group �����W�١A�i�[�o�@��
    options.TagActionsBy(api =>
        new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization"
        });
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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
                    new string[] {}
                }
        });
});

// �]�w JWT �{��
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Secret"])),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthentication();   

builder.Services.AddTransient<ITokenService, TokenService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // ���\���e�ݰ�W
              .AllowAnyHeader()                      // ���\�Ҧ��� headers
              .AllowAnyMethod();                     // ���\�Ҧ� HTTP ��k (GET, POST, etc.)
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2) // �]�w�O���s�u���ɶ����j
};

app.UseCors("AllowSpecificOrigin");

app.UseWebSockets(webSocketOptions);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/front/swagger.json", "Front Site API");
    options.SwaggerEndpoint("/swagger/back/swagger.json", "Back Site API");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
