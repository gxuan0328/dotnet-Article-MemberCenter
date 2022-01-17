using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Article_Backend.Controllers
{
    [Route("article")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly ConnectionStrings _connect;
        public ArticleController(IOptions<ConnectionStrings> connect)
        {
            _connect = connect.Value;
        }

        [TypeFilter(typeof(AuthorizeAttribute))]
        [HttpPost]
        public Response<Article> PostArticle([FromBody] NewArticle article)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                UserDetail userDetail = (UserDetail)HttpContext.Items["UserDetail"];
                if (!ModelState.IsValid)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                }
                else if (article.User_Id != userDetail.Id)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                    {
                        string queryString = @"insert into [ArticleDB].[dbo].[Articles] ([Title], [User_Id], [Content], [Editor]) 
                                                values (@Title, @UserDetail_Id, @Content, @UserDetail_Id)";
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Title", SqlDbType.NVarChar, 100){Value = article.Title},
                            new SqlParameter("@UserDetail_Id", SqlDbType.Int){Value = userDetail.Id},
                            new SqlParameter("@Content", SqlDbType.NVarChar, 2000){Value = article.Content},
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

        [HttpGet("{id}")]
        public Response<Article> GetArticle([FromRoute] int id)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                {
                    string queryString = @"select [Articles].[Id], [Articles].[Title], [Articles].[User_Id], [User].[Name], [Articles].[Content], [Articles].[CreateDatetime], [Articles].[UpdateDatetime], [Articles].[Editor] 
                                            from [ArticleDB].[dbo].[Articles] 
                                            inner join [ArticleDB].[dbo].[User] 
                                            on [Articles].[User_Id]=[User].[Id] 
                                            where [Articles].[Id]= @Id";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Id", SqlDbType.Int){Value = id}
                    });
                    Article article = new Article();
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
                            article.Id = reader.GetInt32("Id");
                            article.Title = reader.GetString("Title");
                            article.User_Id = reader.GetInt32("User_ID");
                            article.Name = reader.GetString("Name");
                            article.Content = reader.GetString("Content");
                            article.CreateDatetime = reader.GetDateTime("CreateDatetime");
                            article.UpdateDatetime = reader.GetDateTime("UpdateDatetime");
                        }
                        reader.Close();
                    }
                    connection.Close();
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = article;
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

        [TypeFilter(typeof(AuthorizeAttribute))]
        [HttpPut("{id}")]
        public Response<Article> PutArticle([FromRoute] int id, [FromBody] Article article)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                if (!ModelState.IsValid)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                }
                else if (id != article.Id)
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                    {
                        UserDetail userDetail = (UserDetail)HttpContext.Items["UserDetail"];
                        string queryString1 = @"select [Id] 
                                                from [ArticleDB].[dbo].[Articles] 
                                                where [Id]=@Id ";
                        if (userDetail.Status != 2)
                        {
                            queryString1 = String.Concat(queryString1, "and [User_Id]=@UserDetail_Id");
                        }
                        SqlCommand command1 = new SqlCommand(queryString1, connection);
                        command1.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Id", SqlDbType.Int){Value = id},
                            new SqlParameter("@UserDetail_Id", SqlDbType.Int){Value = userDetail.Id}
                        });
                        connection.Open();
                        using (SqlDataReader reader = command1.ExecuteReader())
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
                            reader.Close();
                        }
                        connection.Close();
                        string queryString2 = @"update [ArticleDB].[dbo].[Articles] 
                                                set [Title]=@Title, [Content]=@Content, [UpdateDatetime]=GETUTCDATE(), [Editor]=@UserDetail_Id 
                                                where [Id]=@Id";
                        SqlCommand command2 = new SqlCommand(queryString2, connection);
                        command2.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Title", SqlDbType.NVarChar, 100){Value = article.Title},
                            new SqlParameter("@Content",SqlDbType.NVarChar, 2000 ){Value = article.Content},
                            new SqlParameter("@UserDetail_Id", SqlDbType.Int){Value = userDetail.Id},
                            new SqlParameter("@Id", SqlDbType.Int){Value = article.Id}
                        });
                        connection.Open();
                        int check = command2.ExecuteNonQuery();
                        connection.Close();
                        if (check != 1)
                        {
                            throw new Exception("command2 execute failed");
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

        [TypeFilter(typeof(AuthorizeAttribute))]
        [HttpDelete("{id}")]
        public Response<Article> DeleteArticle([FromRoute] int id)
        {
            Response<Article> result = new Response<Article>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                {
                    UserDetail userDetail = (UserDetail)HttpContext.Items["UserDetail"];
                    string queryString1 = @"select [Id] 
                                            from [ArticleDB].[dbo].[Articles] 
                                            where [Id]=@Id ";
                    if (userDetail.Status != 2)
                    {
                        queryString1 = String.Concat(queryString1, "and [User_Id]=@UserDetail_Id");
                    }
                    SqlCommand command1 = new SqlCommand(queryString1, connection);
                    command1.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Id", SqlDbType.Int){Value = id},
                        new SqlParameter("@UserDetail_Id", SqlDbType.Int){Value = userDetail.Id}
                    });
                    connection.Open();
                    using (SqlDataReader reader = command1.ExecuteReader())
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
                        reader.Close();
                    }
                    connection.Close();
                    string queryString2 = @"delete [ArticleDB].[dbo].[Articles] 
                                            where [Id]=@Id";
                    SqlCommand command2 = new SqlCommand(queryString2, connection);
                    command2.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@Id", SqlDbType.Int){Value = id}
                    });
                    connection.Open();
                    int check = command2.ExecuteNonQuery();
                    connection.Close();
                    if (check != 1)
                    {
                        throw new Exception("command2 execute failed");
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

        [HttpGet("id")]
        public Response<List<int>> GetArticleId()
        {
            Response<List<int>> result = new Response<List<int>>();
            try
            {
                List<int> articleId = new List<int>();
                using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                {
                    string queryString = @"select [Articles].[Id] 
                                            from [ArticleDB].[dbo].[Articles]";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            articleId.Add(reader.GetInt32("Id"));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
                result.StatusCode = Status.OK;
                result.Message = nameof(Status.OK);
                result.Data = articleId;
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

        [TypeFilter(typeof(AuthorizeAttribute))]
        [HttpGet("id/personal")]
        public Response<List<int>> GetPersonalArticleId()
        {
            Response<List<int>> result = new Response<List<int>>();
            try
            {
                List<int> articleId = new List<int>();
                using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                {
                    UserDetail userDetail = (UserDetail)HttpContext.Items["UserDetail"];
                    string queryString = @"select [Articles].[Id] 
                                            from [ArticleDB].[dbo].[Articles] 
                                            where [Articles].[User_Id]=@UserDetail_Id";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddRange(new SqlParameter[]
                    {
                        new SqlParameter("@UserDetail_Id", SqlDbType.Int){Value = userDetail.Id}
                    });
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            articleId.Add(reader.GetInt32("Id"));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
                result.StatusCode = Status.OK;
                result.Message = nameof(Status.OK);
                result.Data = articleId;
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

        [HttpGet("search")]
        public Response<List<int>> GetSearchIdList([FromQuery] Search search)
        {
            Response<List<int>> result = new Response<List<int>>();
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
                    List<int> articleId = new List<int>();
                    using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                    {
                        string option = "";
                        List<string> options = new List<string>();
                        List<SqlParameter> parameters = new List<SqlParameter>();
                        if (!String.IsNullOrEmpty(search.Title))
                        {
                            search.Title = $"%{search.Title}%";
                            options.Add("[Title] like @Title");
                            parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 100) { Value = search.Title });
                        }
                        if (!String.IsNullOrEmpty(search.Author))
                        {
                            options.Add("[User].[Name] = @Author");
                            parameters.Add(new SqlParameter("@Author", SqlDbType.NVarChar, 20) { Value = search.Author });
                        }

                        options.Add("[Articles].[CreateDatetime] >= @FromDate");
                        parameters.Add(new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = search.FromDate });

                        options.Add("[Articles].[CreateDatetime] <= @ToDate");
                        parameters.Add(new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = search.ToDate });

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
                        SqlCommand command = new SqlCommand(String.Concat(queryString, option), connection);
                        command.Parameters.AddRange(parameters.ToArray());
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
                            while (reader.Read())
                            {
                                articleId.Add(reader.GetInt32("Id"));
                            }
                            reader.Close();
                        }
                        connection.Close();
                    }
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = articleId;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                result.StatusCode = Status.SystemError;
                result.Message = nameof(Status.SystemError);
                result.Data = null;
            }
            return result;
        }

        [HttpGet("list")]
        public Response<List<Articles>> GetArticleList([FromQuery] string list)
        {
            Response<List<Articles>> result = new Response<List<Articles>>();
            try
            {
                if (String.IsNullOrEmpty(list))
                {
                    result.StatusCode = Status.BadRequest;
                    result.Message = nameof(Status.BadRequest);
                    result.Data = null;
                }
                else
                {
                    List<Articles> articles = new List<Articles>();
                    using (SqlConnection connection = new SqlConnection(_connect.DevConnection))
                    {
                        string[] articleList = list.Split(',');
                        string options = "";
                        List<SqlParameter> parameters = new List<SqlParameter>();
                        foreach (var item in articleList.Select((value, index) => new { value, index }))
                        {
                            if (item.index == 0)
                            {
                                options = String.Concat(options, $"@Id_{item.index}");
                                parameters.Add(new SqlParameter($"@Id_{item.index}", SqlDbType.Int) { Value = Int32.Parse(item.value) });
                            }
                            else
                            {
                                options = String.Concat(options, ",", $"@Id_{item.index}");
                                parameters.Add(new SqlParameter($"@Id_{item.index}", SqlDbType.Int) { Value = Int32.Parse(item.value) });
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
                            while (reader.Read())
                            {
                                Articles temp = new Articles();
                                temp.Id = reader.GetInt32("Id");
                                temp.Title = reader.GetString("Title");
                                temp.Name = reader.GetString("Name");
                                temp.CreateDatetime = reader.GetDateTime("CreateDatetime");
                                articles.Add(temp);
                            }
                            reader.Close();
                        }
                        connection.Close();
                    }
                    result.StatusCode = Status.OK;
                    result.Message = nameof(Status.OK);
                    result.Data = articles;
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
    }
}