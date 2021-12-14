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
    public class ListController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ListController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public Response<List<Articles>> GetArticleList()
        {
            Response<List<Articles>> result = new Response<List<Articles>>();
            try
            {
                if (HttpContext.Request.Query["list"].Count != 0)
                {
                    string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                    using (SqlConnection connection = new SqlConnection(conn))
                    {

                        string articleList_string = HttpContext.Request.Query["list"];
                        string[] articleList_seperate = articleList_string.Split(',');
                        List<int> articleList = new List<int>();
                        List<Articles> articles = new List<Articles>();
                        string queryString = @"select [Articles].[Id], [User].[Name], [Articles].[Title], [Articles].[CreateDatetime] 
                                                from [ArticleDB].[dbo].[Articles] 
                                                inner join [ArticleDB].[dbo].[User] 
                                                on [Articles].[User_Id]=[User].[Id] 
                                                where [Articles].[Id] = @Id";
                        foreach (string item in articleList_seperate)
                        {
                            articleList.Add(Int32.Parse(item));
                        }
                        foreach (int i in articleList)
                        {
                            SqlCommand command = new SqlCommand(queryString, connection);
                            command.Parameters.AddRange(new SqlParameter[]
                            {
                                new SqlParameter("@Id", i)
                            });
                            Articles temp = new Articles();
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            if(reader.HasRows){
                                if (reader.Read())
                                {
                                    temp.Id = reader.GetInt32("Id");
                                    temp.Title = reader.GetString("Title");
                                    temp.Name = reader.GetString("Name");
                                    temp.CreateDatetime = reader.GetDateTime("CreateDatetime");
                                    articles.Add(temp);
                                }
                            }
                            else
                            {
                                result.StatusCode = Status.NotFound;
                                result.Message = nameof(Status.NotFound);
                                result.Data = null;
                                return result;
                            }
                            connection.Close();
                        }
                        result.StatusCode = Status.OK;
                        result.Message = nameof(Status.OK);
                        result.Data = articles;
                        return result;
                    }
                }
                else
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
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