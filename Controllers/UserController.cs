using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Article_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public Response<string> PostLogin(Account user)
        {
            Response<string> result = new Response<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                    return result;
                }

                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString = @"select [Id], [Name], [Status] 
                                            from [ArticleDB].[dbo].[User] 
                                            where [Name]=@Name 
                                            and [Password]=@Password";

                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Name", user.UserName),
                        new SqlParameter("@Password", user.Password),
                    });
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if(!reader.HasRows)
                    {
                        result.StatusCode = Status.NotFound;
                        result.Message = nameof(Status.NotFound);
                        result.Data = null;
                        return result;
                    }
                    UserDetail userDetail = new UserDetail();
                    if(reader.Read())
                    {
                        userDetail.Id = reader.GetInt32("Id");
                        userDetail.Name = reader.GetString("Name");
                        userDetail.Status = reader.GetInt32("Status");
                        Console.WriteLine($"content: {userDetail.Id} and {userDetail.Name} and {userDetail.Status}");
                    }
                    connection.Close();
                    byte[] key = Encoding.ASCII.GetBytes(_configuration["JwtSetting:Key"]);
                    SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim("Id",userDetail.Id.ToString(),ClaimValueTypes.Integer32),
                            new Claim("Name",userDetail.Name),
                            new Claim("Status",userDetail.Status.ToString(),ClaimValueTypes.Integer32)
                        }),
                        Expires = DateTime.Now.AddHours(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                    SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
                    string token = tokenHandler.WriteToken(securityToken);
                    Console.WriteLine($"jwt token: {token}");
                    string queryString1 = @"update [ArticleDB].[dbo].[Token] 
                                            set [Token]=@Token, [UpdateDatetime]=GETUTCDATE() 
                                            where [User_Id]=@User_Id";
                    SqlCommand command1 = new SqlCommand(queryString1, connection);
                    command1.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Token", token),
                        new SqlParameter("@User_Id", userDetail.Id)
                    });
                    connection.Open();
                    int check = command1.ExecuteNonQuery();
                    Console.WriteLine("check is update or not? "+check);
                    connection.Close();
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = token;
                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
                return result;
            }
        }


        [HttpPost("sign")]
        public Response<string> PostSign(Account user)
        {
            Response<string> result = new Response<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                    return result;
                }
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString1 = @"select [Id] 
                                            from [ArticleDB].[dbo].[User] 
                                            where [Name]=@Name";
                    SqlCommand command1 = new SqlCommand(queryString1, connection);
                    command1.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Name", user.UserName),
                    });
                    connection.Open();
                    SqlDataReader reader = command1.ExecuteReader();
                    if (reader.HasRows)
                    {
                        result.StatusCode = Status.NotFound;
                        result.Message = nameof(Status.NotFound);
                        result.Data = null;
                        return result;
                    }
                    connection.Close();
                    string queryString2 = @"insert into[ArticleDB].[dbo].[User]
                                            ([Name], [Password], [Status]) 
                                            values(@Name, @Password, @Status);
                                            select cast(SCOPE_IDENTITY() as int) as Id";
                    SqlCommand command2 = new SqlCommand(queryString2, connection);
                    command2.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Name", user.UserName),
                        new SqlParameter("@Password", user.Password),
                        new SqlParameter("@Status", 1),
                    });
                    connection.Open();
                    int id = Convert.ToInt32(command2.ExecuteScalar());
                    connection.Close();
                    string queryString3 = @"insert into [ArticleDB].[dbo].[Token] ([User_Id]) values(@Id)";
                    SqlCommand command3 = new SqlCommand(queryString3, connection);
                    command3.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Id", id),
                    });
                    connection.Open();
                    int check2 = command3.ExecuteNonQuery();
                    connection.Close();
                }
                result.StatusCode = Status.OK;
                result.Message = nameof(Status.OK);
                result.Data = null;
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
                return result;
            }
        }
    }
}