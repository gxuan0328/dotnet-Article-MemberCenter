using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Article_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public PostController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public Response<Article> PostArticle(Article article)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                if (string.IsNullOrEmpty(article.Title) || article.User_ID == 0 || string.IsNullOrEmpty(article.Content))
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                    return result;
                }
                else
                {
                    string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        string queryString = @"insert into [ArticleDB].[dbo].[Articles] ([Title], [User_Id], [Content], [Editor]) 
                                                values (@Title, @User_Id, @Content, @User_Id)";
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Title", article.Title),
                            new SqlParameter("@User_Id", article.User_ID),
                            new SqlParameter("@Content", article.Content),
                        });
                        connection.Open();
                        var check = command.ExecuteNonQuery();
                        connection.Close();

                        result.StatusCode = Status.OK;
                        result.Message = nameof(Status.OK);
                        result.Data = null;
                        return result;
                    }
                }
            }
            catch
            {
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
                return result;
            }
        }
    }
}