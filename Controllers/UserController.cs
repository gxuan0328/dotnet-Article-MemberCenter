using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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

        // [HttpPost("login")]


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