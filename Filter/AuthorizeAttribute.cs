using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public class QAQAttribute : IAuthorizationFilter
{
    public QAQAttribute(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    private readonly IConfiguration Configuration;


    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // throw new System.NotImplementedException();

        Console.WriteLine("in the QAQ");

        if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues outvalue))
        {
            Console.WriteLine($"成功 {outvalue}");
            Console.WriteLine($"configuration {Configuration["JwtSetting:Key"]}");

        }
        else
        {
            Console.WriteLine("殘念");
            Response<string> result = new Response<string>();
            result.StatusCode = Status.OK;
            result.Message = nameof(Status.Unauthorized);
            result.Data = "AAAAAAAAAA";
            context.Result = new JsonResult(result);
        }
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtSetting:Key"]))
        };
        string a = outvalue;
        Console.WriteLine($"before token: {a}");
        Console.WriteLine($"before2 token: {a.Replace("Bearer ", "")}");

        handler.ValidateToken(a.Replace("Bearer ", ""), tokenValidationParameters, out SecurityToken token);
        Console.WriteLine($"token: {token}");



        
        // Response<string> result = new Response<string>();
        // result.StatusCode = Status.OK;
        // result.Message = nameof(Status.Unauthorized);
        // result.Data = "AAAAAAAAAA";
        // context.HttpContext.Response.ContentType = "application/json";
        // context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
        // context.Result = new JsonResult(result);
    }
}