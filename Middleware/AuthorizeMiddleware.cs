using System;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using Article_Backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public class AuthorizePipeline
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<AuthorizeMiddleware>();
    }
}

public class AuthorizeMiddleware : IMiddleware
{
    private readonly JwtSetting _jwt;
    private readonly ConnectionStrings _connect;

    public AuthorizeMiddleware(IOptions<JwtSetting> jwt, IOptions<ConnectionStrings> connect)
    {
        _jwt = jwt.Value;
        _connect = connect.Value;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Response<string> result = new Response<string>();
        try
        {
            if (!context.Request.Headers.TryGetValue("Authorization", out StringValues outValue))
            {
                result.StatusCode = Status.BadRequest;
                result.Message = nameof(Status.BadRequest);
                result.Data = null;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            }
            else
            {
                string token = outValue.ToString().Replace("Bearer ", "");
                ClaimsPrincipal claimsPrincipal = new JwtService().DecodeToken(_jwt.Key, token);
                UserDetail decode = new UserDetail
                {
                    Id = Convert.ToInt32(claimsPrincipal.FindFirstValue("Id")),
                    Name = claimsPrincipal.FindFirstValue("Name"),
                    Status = Convert.ToInt32(claimsPrincipal.FindFirstValue("Status"))
                };
                using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                {
                    string queryString = @"select [Token] 
                                            from [ArticleDB].[dbo].[Token] 
                                            where [User_Id]=@User_Id";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@User_Id", SqlDbType.Int) { Value = decode.Id }
                    });
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            result.StatusCode = Status.TokenNotFound;
                            result.Message = nameof(Status.TokenNotFound);
                            result.Data = null;
                            context.Response.ContentType = "application/json";
                            context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                        }
                        if (reader.Read())
                        {
                            if (reader.GetString("Token") != token)
                            {
                                result.StatusCode = Status.TokenChanged;
                                result.Message = nameof(Status.TokenChanged);
                                result.Data = null;
                                context.Response.ContentType = "application/json";
                                context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                            }
                            else
                            {
                                context.Items.Add("Token", decode);
                                next.Invoke(context);
                            }
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
        }
        catch (SecurityTokenExpiredException e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
            result.StatusCode = Status.TokenExpired;
            result.Message = nameof(Status.TokenExpired);
            result.Data = null;
            context.Response.ContentType = "application/json";
            context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        catch (SecurityTokenValidationException e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
            result.StatusCode = Status.TokenInvalid;
            result.Message = nameof(Status.TokenInvalid);
            result.Data = null;
            context.Response.ContentType = "application/json";
            context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
            result.StatusCode = Status.SystemError;
            result.Message = nameof(Status.SystemError);
            result.Data = null;
            context.Response.ContentType = "application/json";
            context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        return Task.CompletedTask;
    }
}