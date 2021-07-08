using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YukiCMS.Jobs;
using YukiCMS.Models;
using YukiCMS.Models.JWT;
using YukiCMS.Service;
using YukiCMS.Service.JWT;

namespace YukiCMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<YukiDatabaseSettings>(Configuration.GetSection(nameof(YukiDatabaseSettings)));
            services.AddSingleton<IYukiDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<YukiDatabaseSettings>>().Value);
            services.Configure<YukiEmailSettings>(Configuration.GetSection(nameof(YukiEmailSettings)));
            services.AddSingleton<IYukiEmailSettings>(sp =>
                sp.GetRequiredService<IOptions<YukiEmailSettings>>().Value);


            JWTTokenOptions _gtokenOptions = new JWTTokenOptions();
            var keyParams = RSAUtils.generateKey();
            _gtokenOptions.Key = new RsaSecurityKey(keyParams);
            _gtokenOptions.Issuer = Configuration["YukiJwtSettings:issuer"];
            _gtokenOptions.Credentials = new SigningCredentials(_gtokenOptions.Key, SecurityAlgorithms.RsaSha256Signature);
            services.AddSingleton(_gtokenOptions);

            JWTTokenOptions _atokenOptions = new JWTTokenOptions();
            _atokenOptions.Key = new RsaSecurityKey(RSAUtils.getPublicKeyParas());
            _atokenOptions.Issuer = Configuration["YukiJwtSettings:issuer"];
            _atokenOptions.Audience = Configuration["YukiJwtSettings:audience"];
            _atokenOptions.Credentials = new SigningCredentials(_atokenOptions.Key, SecurityAlgorithms.RsaSha256Signature);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = _atokenOptions.Key,
                    ValidAudience = _atokenOptions.Audience, 
                    ValidIssuer = _atokenOptions.Issuer, 
                    ValidateLifetime = true
                };
            });

            services.AddSingleton<JwtTokenGenService>();

            services.AddSingleton<YukiFindPasswordTokenService>();
            services.AddSingleton<YukiGlobalService>();
            services.AddSingleton<YukiUserService>();
     
            services.AddSingleton<YukiSeatService>();

            services.AddSingleton<YukiReviewTemplateService>();
            services.AddSingleton<YukiQuestionService>();
            services.AddSingleton<YukiReviewService>();

            services.AddSingleton<YukiPermissionService>();
            services.AddSingleton<YukiPermissionGroupService>();

            services.AddSingleton<YukiEmailService>();

            services.AddSingleton<YukiFileService>();
            services.AddSingleton<YukiFileGroupService>();
            services.AddSingleton<YukiCommitteeService>();

            services.AddSingleton<YukiPaymentService>();
            services.AddSingleton<YukiAccommodationService>();

            
            services.AddSingleton<YukiExcelService>();

            services.AddTimedJob();
            string[] cors = Configuration.GetSection("YukiCORSAllowedOrigin").Get<string[]>();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(cors).AllowAnyMethod().AllowAnyHeader();
                    });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(opt =>{
                    opt.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseTimedJob();
            app.UseCors();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
