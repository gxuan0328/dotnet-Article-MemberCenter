using System;
using System.Data;
using Article_Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Article_Backend.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly JwtSetting _jwt;
        private readonly ConnectionStrings _connect;

        public UserController(IOptions<JwtSetting> jwt, IOptions<ConnectionStrings> connect)
        {
            _jwt = jwt.Value;
            _connect = connect.Value;
        }

        [HttpPost("login")]
        public Response<string> PostLogin([FromBody] Account user)
        {
            Response<string> result = new Response<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                }
                else
                {
                    string token = "";
                    using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                    {
                        string queryString = @"select [Id], [Name], [Status] 
                                                from [ArticleDB].[dbo].[User] 
                                                where [Name]=@Name 
                                                and [Password]=@Password";
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Name", SqlDbType.NVarChar, 20){Value = user.UserName},
                            new SqlParameter("@Password", SqlDbType.NVarChar, 20) {Value = user.Password},
                        });
                        UserDetail userDetail = new UserDetail();
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                result.StatusCode = Status.NotFound;
                                result.Message = nameof(Status.NotFound);
                                result.Data = null;
                                reader.Close();
                                connection.Close();
                                return result;
                            }
                            if (reader.Read())
                            {
                                userDetail.Id = reader.GetInt32("Id");
                                userDetail.Name = reader.GetString("Name");
                                userDetail.Status = reader.GetInt32("Status");
                            }
                            reader.Close();
                        }
                        connection.Close();
                        token = new JwtService().CreateToken(_jwt.Key, userDetail);
                        string queryString1 = @"update [ArticleDB].[dbo].[Token] 
                                                set [Token]=@Token, [UpdateDatetime]=GETUTCDATE() 
                                                where [User_Id]=@User_Id";
                        SqlCommand command1 = new SqlCommand(queryString1, connection);
                        command1.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Token", SqlDbType.NVarChar, 500){Value = token},
                            new SqlParameter("@User_Id", SqlDbType.Int){Value = userDetail.Id}
                        });
                        connection.Open();
                        int check = command1.ExecuteNonQuery();
                        connection.Close();
                        if (check != 1)
                        {
                            throw new Exception("command1 execute failed");
                        }
                    }
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = token;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
            }
            return result;
        }


        [HttpPost("sign")]
        public Response<string> PostSign([FromBody] Account user)
        {
            Response<string> result = new Response<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                    {
                        string queryString1 = @"select [Id] 
                                                from [ArticleDB].[dbo].[User] 
                                                where [Name]=@Name";
                        SqlCommand command1 = new SqlCommand(queryString1, connection);
                        command1.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Name", SqlDbType.NVarChar, 20){Value = user.UserName}
                        });
                        connection.Open();
                        using (SqlDataReader reader = command1.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                result.StatusCode = Status.NotFound;
                                result.Message = nameof(Status.NotFound);
                                result.Data = null;
                                reader.Close();
                                connection.Close();
                                return result;
                            }
                            reader.Close();
                        }
                        connection.Close();
                        string queryString2 = @"insert into[ArticleDB].[dbo].[User]
                                            ([Name], [Password], [Status]) 
                                            values(@Name, @Password, @Status);
                                            select cast(SCOPE_IDENTITY() as int) as Id";
                        SqlCommand command2 = new SqlCommand(queryString2, connection);
                        command2.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Name", SqlDbType.NVarChar, 20){Value = user.UserName},
                            new SqlParameter("@Password", SqlDbType.NVarChar, 20){Value = user.Password},
                            new SqlParameter("@Status", SqlDbType.Int){Value = 1}
                        });
                        connection.Open();
                        int id = Convert.ToInt32(command2.ExecuteScalar());
                        connection.Close();
                        string queryString3 = @"insert into [ArticleDB].[dbo].[Token] ([User_Id]) values(@Id)";
                        SqlCommand command3 = new SqlCommand(queryString3, connection);
                        command3.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Id", SqlDbType.Int){Value = id}
                        });
                        connection.Open();
                        int check = command3.ExecuteNonQuery();
                        connection.Close();
                        if (check != 1)
                        {
                            throw new Exception("command3 execute failed");
                        }
                    }
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
            }
            return result;
        }
        
        [MiddlewareFilter(typeof(AuthorizePipeline))]
        [HttpPut("logout")]
        public Response<string> PutLogout()
        {
            Response<string> result = new Response<string>();
            try
            {
                UserDetail token = (UserDetail)HttpContext.Items["Token"];
                using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                {
                    string queryString = @"update [ArticleDB].[dbo].[Token] 
                                            set [Token]=@Token, [UpdateDatetime]=GETUTCDATE() 
                                            where [User_Id]=@Token_Id";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Token", SqlDbType.NVarChar, 500){Value = ""},
                        new SqlParameter("@Token_Id", SqlDbType.Int){Value = token.Id}
                    });
                    connection.Open();
                    int check = command.ExecuteNonQuery();
                    connection.Close();
                    if (check != 1)
                    {
                        throw new Exception("command execute failed");
                    }
                }
                result.StatusCode = Status.OK;
                result.Message = nameof(Status.OK);
                result.Data = null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
            }
            return result;
        }
    }
}