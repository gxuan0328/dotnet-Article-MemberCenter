using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Article_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ArticleController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost]
        public Response<Article> PostArticle(Article article)
        {
            Response<Article> result = new Response<Article>();
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
                    string queryString = @"insert into [ArticleDB].[dbo].[Articles] ([Title], [User_Id], [Content], [Editor]) 
                                            values (@Title, @User_Id, @Content, @User_Id)";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Title", article.Title),
                        new SqlParameter("@User_Id", article.User_Id),
                        new SqlParameter("@Content", article.Content),
                    });
                    connection.Open();
                    int check = command.ExecuteNonQuery();
                    connection.Close();

                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = null;
                    return result;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR: "+e.Message);
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
                return result;
            }
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
                    string queryString = @"select [Articles].[Id], [Articles].[Title], [Articles].[User_Id], [User].[Name], [Articles].[Content], [Articles].[CreateDatetime], [Articles].[UpdateDatetime], [Articles].[Editor] 
                                            from [ArticleDB].[dbo].[Articles] 
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
                    if (!reader.HasRows)
                    {
                        result.StatusCode = Status.NotFound;
                        result.Message = nameof(Status.NotFound);
                        result.Data = null;
                        return result;
                    }
                    if (reader.Read())
                    {
                        temp.Id = reader.GetInt32("Id");
                        temp.Title = reader.GetString("Title");
                        temp.User_Id = reader.GetInt32("User_ID");
                        temp.Name = reader.GetString("Name");
                        temp.Content = reader.GetString("Content");
                        temp.CreateDatetime = reader.GetDateTime("CreateDatetime");
                        temp.UpdateDatetime = reader.GetDateTime("UpdateDatetime");
                        temp.Editor = reader.GetInt32("Editor");
                        article.Add(temp);
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

        [Authorize]
        [HttpPut("{id}")]
        public Response<Article> PutArticle(int id, Article article)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                UserDetail token = (UserDetail)HttpContext.Items["Token"];
                if (!ModelState.IsValid)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                    return result;
                }
                if (id != article.Id)
                {
                    result.StatusCode = Status.NotFound;
                    result.Message = nameof(Status.NotFound);
                    result.Data = null;
                    return result;
                }
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString1 = @"select [Id] 
                                            from [ArticleDB].[dbo].[Articles] 
                                            where [Id]=@Id 
                                            and [User_Id]=@Token_Id";
                    SqlCommand command1 = new SqlCommand(queryString1, connection);
                    command1.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Token_Id", token.Id)
                    });
                    connection.Open();
                    SqlDataReader reader = command1.ExecuteReader();
                    if (!reader.HasRows && token.Status != 2)
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
                    new SqlParameter("@Token_Id", token.Id),
                    new SqlParameter("@Id", article.Id)
                    });
                    connection.Open();
                    int check = command2.ExecuteNonQuery();
                    connection.Close();
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
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

        [Authorize]
        [HttpDelete("{id}")]
        public Response<Article> DeleteArticle(int id)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                UserDetail token =  (UserDetail)HttpContext.Items["Token"];
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString1 = @"select [Id] 
                                            from [ArticleDB].[dbo].[Articles] 
                                            where [Id]=@Id 
                                            and [User_Id]=@Token_Id";
                    SqlCommand command1 = new SqlCommand(queryString1, connection);
                    command1.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Token_Id", token.Id)
                    });
                    connection.Open();
                    SqlDataReader reader = command1.ExecuteReader();
                    if (!reader.HasRows && token.Status != 2)
                    {
                        result.StatusCode = Status.Forbidden;
                        result.Message = nameof(Status.Forbidden);
                        result.Data = null;
                        return result;
                    }
                    connection.Close();
                    string queryString2 = @"delete [ArticleDB].[dbo].[Articles] 
                                            where [Id]=@Id";
                    SqlCommand command2 = new SqlCommand(queryString2, connection);
                    command2.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Id", id)
                    });
                    connection.Open();
                    int check = command2.ExecuteNonQuery();
                    connection.Close();
                    if (check == 0)
                    {
                        result.StatusCode = Status.NotFound;
                        result.Message = nameof(Status.NotFound);
                        result.Data = null;
                        return result;
                    }
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
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

        [HttpGet("id")]
        public Response<List<int>> GetArticleId()
        {
            Response<List<int>> result = new Response<List<int>>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString = @"select [Articles].[Id] 
                                            from [ArticleDB].[dbo].[Articles]";
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
        [Authorize]
        [HttpGet("id/personal")]
        public Response<List<int>> GetPersonalArticleId()
        {
            Response<List<int>> result = new Response<List<int>>();
            try
            {
                UserDetail token = (UserDetail)HttpContext.Items["Token"];
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString = @"select [Articles].[Id] 
                                            from [ArticleDB].[dbo].[Articles] 
                                            where [Articles].[User_Id]=@Token_Id";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Token_Id", token.Id)
                    });
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


        [HttpGet("search")]
        public Response<List<int>> GetSearchIdList(string title, string author, string fromDate, string toDate)
        {
            Response<List<int>> result = new Response<List<int>>();
            try
            {
                if(String.IsNullOrEmpty(title+author+fromDate+toDate))
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                    return result;
                }
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string option = "";
                    List<string> options = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    if (!String.IsNullOrEmpty(title))
                    {
                        title = $"%{title}%";
                        options.Add("[Title] like @Title");
                        parameters.Add(new SqlParameter("@Title", title));
                    }
                    if (!String.IsNullOrEmpty(author))
                    {
                        options.Add("[User].[Name] = @Author");
                        parameters.Add(new SqlParameter("@Author", author));

                    }
                    if (!String.IsNullOrEmpty(fromDate))
                    {
                        options.Add("[Articles].[CreateDatetime] >= @FromDate");
                        parameters.Add(new SqlParameter("@FromDate", fromDate));

                    }
                    if (!String.IsNullOrEmpty(toDate))
                    {
                        options.Add("[Articles].[CreateDatetime] <= @ToDate");
                        parameters.Add(new SqlParameter("@ToDate", toDate));

                    }
                    string queryString = @"select [Articles].[Id] 
                                        from [ArticleDB].[dbo].[Articles] 
                                        inner join [ArticleDB].[dbo].[User] 
                                        on [Articles].[User_Id]=[User].[Id]";
                    foreach (var item in options.Select((value, index) => new { value, index }))
                    {
                        if (item.index == 0)
                        {
                            option = String.Concat(option, " where ", item.value);
                        }
                        else
                        {
                            option = String.Concat(option, " and ", item.value);
                        }
                    }
                    SqlCommand command = new SqlCommand(queryString + option, connection);
                    command.Parameters.AddRange(parameters.ToArray());
                    List<int> articleId = new List<int>();
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        result.StatusCode = Status.NotFound;
                        result.Message = nameof(Status.NotFound);
                        result.Data = null;
                        return result;
                    }
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
            catch(Exception e)
            {
                Console.WriteLine("Error: "+e.Message);
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
                return result;
            }
        }

        [HttpGet("search/{title}")]
        public Response<List<Search>> GetSearchList(string title)
        {
            Response<List<Search>> result = new Response<List<Search>>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString = @"select [Id], [Title] 
                                            from [ArticleDB].[dbo].[Articles] 
                                            where [Title] like @Title";
                    title = $"%{title}%";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                            new SqlParameter("@Title", title)
                    });
                    List<Search> search = new List<Search>();
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Search temp = new Search();
                        temp.Id = reader.GetInt32("Id");
                        temp.Title = reader.GetString("Title");
                        search.Add(temp);
                    }
                    connection.Close();
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = search;
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

        [HttpGet("list")]
        public Response<List<Articles>> GetArticleList(string list)
        {
            Response<List<Articles>> result = new Response<List<Articles>>();
            try
            {
                if (String.IsNullOrEmpty(list))
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                    return result;
                }
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {

                    string[] articleList = list.Split(',');
                    List<Articles> articles = new List<Articles>();
                    string options = "";
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    foreach (var item in articleList.Select((value, index) => new { value, index }))
                    {
                        if (item.index == 0)
                        {
                            options = String.Concat(options, $"@Id_{item.index}");
                            parameters.Add(new SqlParameter($"@Id_{item.index}", Int32.Parse(item.value)));
                        }
                        else
                        {
                            options = String.Concat(options, ",", $"@Id_{item.index}");
                            parameters.Add(new SqlParameter($"@Id_{item.index}", Int32.Parse(item.value)));
                        }
                    }
                    string queryString = @$"select [Articles].[Id], [User].[Name], [Articles].[Title], [Articles].[CreateDatetime] 
                                            from [ArticleDB].[dbo].[Articles] 
                                            inner join [ArticleDB].[dbo].[User] 
                                            on [Articles].[User_Id]=[User].[Id] 
                                            where [Articles].[Id] in ({options})";

                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(parameters.ToArray());
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        result.StatusCode = Status.NotFound;
                        result.Message = nameof(Status.NotFound);
                        result.Data = null;
                        return result;
                    }
                    while (reader.Read())
                    {
                        Articles temp = new Articles();
                        temp.Id = reader.GetInt32("Id");
                        temp.Title = reader.GetString("Title");
                        temp.Name = reader.GetString("Name");
                        temp.CreateDatetime = reader.GetDateTime("CreateDatetime");
                        articles.Add(temp);
                    }
                    connection.Close();
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = articles;
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