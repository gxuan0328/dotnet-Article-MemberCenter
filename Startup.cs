using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Article_Backend
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
            services.AddControllers();

            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddCors(options => options.AddDefaultPolicy(builder => builder.WithOrigins("http://localhost:4200")
                                                                                   .AllowAnyHeader()
                                                                                   .AllowAnyMethod()));

            //禁止自動回傳status code 400
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtSetting:Key"]))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            try
                            {
                                context.HandleResponse();
                                Response<string> result = new Response<string>();
                                result.StatusCode = Status.Unauthorized;
                                result.Message = nameof(Status.Unauthorized);
                                result.Data = null;
                                context.Response.StatusCode = 200;
                                context.Response.ContentType = "application/json";
                                context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                                return Task.CompletedTask;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"ERROR: {e.Message}");
                                Response<string> result = new Response<string>();
                                result.StatusCode = Status.SystemError;
                                result.Message = nameof(Status.SystemError);
                                result.Data = null;
                                context.Response.StatusCode = 200;
                                context.Response.ContentType = "application/json";
                                context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                                return Task.CompletedTask;
                            }

                        },
                        OnTokenValidated = context =>
                        {
                            try
                            {
                                string authorization = context.Request.Headers["Authorization"];
                                authorization = authorization.Replace("Bearer ", "");
                                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                                JwtSecurityToken token = handler.ReadJwtToken(authorization);
                                UserDetail decode = new UserDetail
                                {
                                    Id = Convert.ToInt32(token.Payload["Id"]),
                                    Name = token.Payload["Name"].ToString(),
                                    Status = Convert.ToInt32(token.Payload["Status"])
                                };
                                string conn = Configuration.GetValue<string>("ConnectionStrings:DevConnection");
                                using (SqlConnection connection = new SqlConnection(conn))
                                {
                                    string queryString = "select [Token] from [ArticleDB].[dbo].[Token] where [User_Id]=@User_Id";
                                    SqlCommand command = new SqlCommand(queryString, connection);
                                    command.Parameters.AddRange(new SqlParameter[]
                                    {
                                        new SqlParameter("@User_Id", SqlDbType.Int) { Value = decode.Id }
                                    });
                                    connection.Open();
                                    SqlDataReader reader = command.ExecuteReader();
                                    if (!reader.HasRows)
                                    {
                                        Response<string> result = new Response<string>();
                                        result.StatusCode = Status.Forbidden;
                                        result.Message = nameof(Status.Forbidden);
                                        result.Data = null;
                                        context.Response.StatusCode = 200;
                                        context.Response.ContentType = "application/json";
                                        context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                                        return Task.CompletedTask;
                                    }
                                    if (reader.Read())
                                    {
                                        if (reader.GetString("Token") != authorization)
                                        {
                                            Response<string> result = new Response<string>();
                                            result.StatusCode = Status.Forbidden;
                                            result.Message = nameof(Status.Forbidden);
                                            result.Data = null;
                                            context.Response.StatusCode = 200;
                                            context.Response.ContentType = "application/json";
                                            context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                                            return Task.CompletedTask;
                                        }
                                    }
                                    connection.Close();
                                }
                                context.HttpContext.Items.Add("Token", decode);
                                return Task.CompletedTask;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"ERROR: {e.Message}");
                                Response<string> result = new Response<string>();
                                result.StatusCode = Status.SystemError;
                                result.Message = nameof(Status.SystemError);
                                result.Data = null;
                                context.Response.StatusCode = 200;
                                context.Response.ContentType = "application/json";
                                context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                                return Task.CompletedTask;
                            }
                        }
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
