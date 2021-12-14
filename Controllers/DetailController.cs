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
    public class DetailController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public DetailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{id}")]
        public Response<Article> GetArticle(int id)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString = @"select [Articles].[Id], [Articles].[Title], [Articles].[User_Id], [User].[Name], [Articles].[Content], [Articles].[CreateDatetime], [Articles].[UpdateDatetime], [Articles].[Editor] from [ArticleDB].[dbo].[Articles] 
                                                inner join [ArticleDB].[dbo].[User] 
                                                on [Articles].[User_Id]=[User].[Id] 
                                                where [Articles].[Id]= @Id";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                            new SqlParameter("@Id", id)
                    });
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    List<Article> article = new List<Article>();
                    Article temp = new Article();
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            temp.Id = reader.GetInt32("Id");
                            temp.Title = reader.GetString("Title");
                            temp.User_ID = reader.GetInt32("User_ID");
                            temp.Name = reader.GetString("Name");
                            temp.Content = reader.GetString("Content");
                            temp.CreateDatetime = reader.GetDateTime("CreateDatetime");
                            temp.UpdateDatetime = reader.GetDateTime("UpdateDatetime");
                            temp.Editor = reader.GetInt32("Editor");
                            article.Add(temp);
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
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = article[0];
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

        [HttpPut("{id}")]
        public Response<Article> PutArticle(int id, Article article)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                if (string.IsNullOrEmpty(article.Title) || string.IsNullOrEmpty(article.Content))
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                    return result;
                }
                else if (id == article.Id)
                {
                    string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        string queryString1 = @"select [Id] from [ArticleDB].[dbo].[Articles] where [Id]=@Id and [User_Id]=@Token_Id";
                        SqlCommand command1 = new SqlCommand(queryString1, connection);
                        command1.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Id", id),
                            //TODO:
                            //使用JWT後，將article.User_ID改為token內的user_Id
                            new SqlParameter("@Token_Id", article.User_ID)
                        });
                        List<ArticleId> articleId = new List<ArticleId>();
                        ArticleId temp = new ArticleId();
                        connection.Open();
                        SqlDataReader reader = command1.ExecuteReader();
                        //TODO:
                        //使用JWT後，加上比對token.status==2
                        if (reader.HasRows)
                        {
                            if (reader.Read())
                            {
                                temp.Id = reader.GetInt32("Id");
                                articleId.Add(temp);
                            }
                        }
                        else
                        {
                            result.StatusCode = Status.Forbidden;
                            result.Message = nameof(Status.Forbidden);
                            result.Data = null;
                            return result;
                        }
                        connection.Close();
                        string queryString2 = @"update [ArticleDB].[dbo].[Articles] 
                                                set [Title]=@Title, [Content]=@Content, [UpdateDatetime]=GETUTCDATE(), [Editor]=@Token_Id 
                                                where [Id]=@Id";
                        SqlCommand command2 = new SqlCommand(queryString2, connection);
                        command2.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Title", article.Title),
                            new SqlParameter("@Content", article.Content),
                            //TODO:
                            //使用JWT後，將article.User_ID改為token內的user_Id
                            new SqlParameter("@Token_Id", article.User_ID),
                            new SqlParameter("@Id", article.Id)
                        });
                        connection.Open();
                        var check = command2.ExecuteNonQuery();
                        connection.Close();
                        result.StatusCode = Status.OK;
                        result.Message = nameof(Status.OK);
                        result.Data = null;
                        return result;
                    }
                }
                else
                {
                    result.StatusCode = Status.NotFound;
                    result.Message = nameof(Status.NotFound);
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

        [HttpDelete("{id}")]
        public Response<Article> DeleteArticle(int id)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString1 = @"select [Id] from [ArticleDB].[dbo].[Articles] where [Id]=@Id and [User_Id]=@Token_Id";
                    SqlCommand command1 = new SqlCommand(queryString1, connection);
                    command1.Parameters.AddRange(new SqlParameter[]
                    {
                            new SqlParameter("@Id", id),
                            //TODO:
                            //使用JWT後，將2改為token內的user_Id
                            new SqlParameter("@Token_Id", 2)
                    });
                    List<ArticleId> articleId = new List<ArticleId>();
                    ArticleId temp = new ArticleId();
                    connection.Open();
                    SqlDataReader reader = command1.ExecuteReader();
                    //TODO:
                    //使用JWT後，加上比對token.status==2
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            temp.Id = reader.GetInt32("Id");
                            articleId.Add(temp);
                        }
                    }
                    else
                    {
                        result.StatusCode = Status.Forbidden;
                        result.Message = nameof(Status.Forbidden);
                        result.Data = null;
                        return result;
                    }
                    connection.Close();
                    string queryString2 = "delete [ArticleDB].[dbo].[Articles] where [Id]=@Id";
                    SqlCommand command2 = new SqlCommand(queryString2, connection);
                    command2.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Id", id)
                    });
                    connection.Open();
                    var check = command2.ExecuteNonQuery();
                    connection.Close();
                    if(check == 1)
                    {
                        result.StatusCode = Status.OK;
                        result.Message = nameof(Status.OK);
                        result.Data = null;
                        return result;
                    }
                    else
                    {
                        result.StatusCode = Status.NotFound;
                        result.Message = nameof(Status.NotFound);
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