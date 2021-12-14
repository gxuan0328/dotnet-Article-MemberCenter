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
    public class ArticleIdController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ArticleIdController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public Response<List<int>> GetArticleId()
        {
            Response<List<int>> result = new Response<List<int>>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString = "select [Articles].[Id] from [ArticleDB].[dbo].[Articles]";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    List<int> articleId = new List<int>();
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        articleId.Add(reader.GetInt32("Id"));
                    }
                    connection.Close();
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = articleId;
                    return result;
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