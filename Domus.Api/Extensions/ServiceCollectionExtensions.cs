using System.Text;
using AutoMapper;
using Domus.Api.Exceptions;
using Domus.Api.Settings;
using Domus.Common.Constants;
using Domus.Common.Exceptions;
using Domus.Common.Settings;
using Domus.DAL.Data;
using Domus.DAL.Implementations;
using Domus.DAL.Interfaces;
using Domus.Domain.Entities;
using Domus.Service.AutoMappings;
using Domus.Service.Implementations;
using Domus.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Domus.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration, SwaggerSettings? swaggerSettings = default)
    {
        swaggerSettings ??= configuration.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>() ?? throw new MissingSwaggerSettingsException();
        
        services.AddSwaggerGen(
            options => 
            { 
                options.SwaggerDoc(swaggerSettings.Version, new OpenApiInfo
                {
                    Version = swaggerSettings.Version,
                    Title = swaggerSettings.Title,
                    Description = swaggerSettings.Description,
                    TermsOfService = swaggerSettings.GetTermsOfService(),
                    Contact = swaggerSettings.GetContact(),
                    License = swaggerSettings.GetLicense()
                });
                options.SwaggerGeneratorOptions = new SwaggerGeneratorOptions()
                {
                    Servers = swaggerSettings.GetServers()
                };
                options.AddSecurityDefinition(swaggerSettings.Options.SecurityScheme.Name, swaggerSettings.GetSecurityScheme());
                options.AddSecurityRequirement(swaggerSettings.GetSecurityRequirement());
            }
        );
        
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>() ?? throw new MissingJwtSettingsException();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtSettings.Issuer,
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidAudience = jwtSettings.Audience,
                ValidateAudience = jwtSettings.ValidateAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ClockSkew = TimeSpan.Zero
            };
        });
        return services;
    }

    public static IServiceCollection AddGgAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var googleSettings = configuration.GetSection(nameof(GoogleSettings)).Get<GoogleSettings>() ?? throw new MissingGoogleSettingsException();
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            }).AddCookie()
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = googleSettings.ClientId;
                options.ClientSecret = googleSettings.ClientSecret;
            });
        return services;
    }

    public static IServiceCollection AddDefaultCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>() ??
                           throw new MissingCorsSettingsException();
        services.AddCors(options =>
        {
            options.AddPolicy(CorsConstants.APP_CORS_POLICY, builder =>
            {
                builder.WithOrigins(corsSettings.GetAllowedOriginsArray())
                    .WithHeaders(corsSettings.GetAllowedHeadersArray())
                    .WithMethods(corsSettings.GetAllowedMethodsArray());
                if (corsSettings.AllowCredentials)
                {
                    builder.AllowCredentials();
                }

                builder.Build();
            });
        });
        
        return services;
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IAppDbContext, DomusContext>();
        services.AddScoped<DomusContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // repository register
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleCategoryRepository, ArticleCategoryRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductDetailRepository, ProductDetailRepository>();
        services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
        services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
        services.AddScoped<IProductAttributeValueRepository, ProductAttributeValueRepository>();
        services.AddScoped<IQuotationRepository, QuotationRepository>();
        services.AddScoped<IProductDetailQuotationRepository, ProductDetailQuotationRepository>();
        services.AddScoped<IQuotationNegotiationLogRepository, QuotationNegotiationLogRepository>();
        services.AddScoped<INegotiationMessageRepository, NegotiationMessageRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IPackageImageRepository, PackageImageRepository>();

        // service register
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IJwtService, JwtService>();
		services.AddScoped<IEmailService, EmailService>();
		services.AddScoped<IProductService, ProductService>();
		services.AddScoped<IProductDetailService, ProductDetailService>();
		services.AddScoped<IQuotationService, QuotationService>();
        services.AddIdentity<DomusUser, IdentityRole>()
            .AddEntityFrameworkStores<DomusContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IVnpayService, VnpayService>();
        services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
        services.AddScoped<IPackageService, PackageService>();
        
        var config = new MapperConfiguration(AutoMapperConfiguration.RegisterMaps);
       
        var mapper = config.CreateMapper();
        services.AddSingleton(mapper);
        
        return services;
    }
}
