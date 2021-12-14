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
    public class SearchController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SearchController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public Response<List<int>> GetSearchIdList()
        {
            Response<List<int>> result = new Response<List<int>>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string title = HttpContext.Request.Query["Title"], author = HttpContext.Request.Query["author"],
                        fromDate = HttpContext.Request.Query["fromDate"], toDate = HttpContext.Request.Query["toDate"],
                        option = "";
                    int userId = 0;
                    List<string> options = new List<string>();
                    bool name = false;

                    if (!String.IsNullOrEmpty(title))
                    {
                        title = "%" + title + "%";
                        options.Add("[Title] like @Title");
                    }
                    if (!String.IsNullOrEmpty(author))
                    {
                        string queryString1 = "select [User].[Id] from [ArticleDB].[dbo].[User] where [User].[Name]=@Author";
                        SqlCommand command1 = new SqlCommand(queryString1, connection);
                        command1.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@Author", author)
                        });
                        connection.Open();
                        SqlDataReader reader = command1.ExecuteReader();
                        Console.WriteLine(reader.HasRows);
                        if (reader.HasRows)
                        {
                            if (reader.Read())
                            {
                                name = true;
                                userId = reader.GetInt32("Id");
                                options.Add("[User_Id] = @User_Id");
                            }
                        }
                        connection.Close();
                    }
                    if (!String.IsNullOrEmpty(fromDate))
                    {
                        options.Add("[CreateDatetime] >= @FromDate");
                    }
                    if (!String.IsNullOrEmpty(toDate))
                    {
                        options.Add("[CreateDatetime] <= @ToDate");
                    }
                    string queryString2 = "select [Articles].[Id] from [ArticleDB].[dbo].[Articles]";
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
                    if (!String.IsNullOrEmpty(option))
                    {
                        SqlCommand command2 = new SqlCommand(queryString2 + option, connection);
                        List<int> articleId = new List<int>();
                        if (!String.IsNullOrEmpty(title))
                        {
                            command2.Parameters.Add(new SqlParameter("@Title", title)); 
                        }
                        if (userId != 0)
                        {
                            command2.Parameters.Add(new SqlParameter("@User_Id", userId));
                        }
                        if (!String.IsNullOrEmpty(fromDate))
                        {
                            command2.Parameters.Add(new SqlParameter("@FromDate", fromDate));
                        }
                        if (!String.IsNullOrEmpty(toDate))
                        {
                            command2.Parameters.Add(new SqlParameter("@ToDate", toDate));
                        }
                        connection.Open();
                        SqlDataReader reader = command2.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                articleId.Add(reader.GetInt32("Id"));
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
                        result.Data = articleId;
                        return result;
                    }
                    else if (!name)
                    {
                        result.StatusCode = Status.NotFound;
                        result.Message = nameof(Status.NotFound);
                        result.Data = null;
                        return result;
                    }
                    else
                    {
                        result.StatusCode = Status.BadRequest;
                        result.Message = nameof(Status.BadRequest);
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

        [HttpGet("{title}")]
        public Response<List<Search>> GetSearchList(string title)
        {
            Response<List<Search>> result = new Response<List<Search>>();
            try
            {
                string conn = _configuration.GetValue<string>("ConnectionStrings:DevConnection");
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string queryString = "select [Id], [Title] from [ArticleDB].[dbo].[Articles] where [Title] like @Title";
                    title = "%" + title + "%";
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
    }
}