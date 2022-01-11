// startup.cs 驗證寫法
// services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateAudience = false,
//             ValidateIssuer = false,
//             ClockSkew = TimeSpan.Zero,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtSetting:Key"]))
//         };
//         options.Events = new JwtBearerEvents
//         {
//             OnChallenge = context =>
//             {
//                 try
//                 {
//                     context.HandleResponse();
//                     Response<string> result = new Response<string>();
//                     result.StatusCode = Status.Unauthorized;
//                     result.Message = nameof(Status.Unauthorized);
//                     result.Data = null;
//                     context.Response.StatusCode = 200;
//                     context.Response.ContentType = "application/json";
//                     context.Response.WriteAsync(JsonConvert.SerializeObject(result));
//                     return Task.CompletedTask;
//                 }
//                 catch (Exception e)
//                 {
//                     Console.WriteLine($"ERROR: {e.Message}");
//                     Response<string> result = new Response<string>();
//                     result.StatusCode = Status.SystemError;
//                     result.Message = nameof(Status.SystemError);
//                     result.Data = null;
//                     context.Response.StatusCode = 200;
//                     context.Response.ContentType = "application/json";
//                     context.Response.WriteAsync(JsonConvert.SerializeObject(result));
//                     return Task.CompletedTask;
//                 }

//             },
//             OnTokenValidated = context =>
//             {
//                 try
//                 {
//                     string authorization = context.Request.Headers["Authorization"];
//                     authorization = authorization.Replace("Bearer ", "");
//                     JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
//                     JwtSecurityToken token = handler.ReadJwtToken(authorization);
//                     UserDetail decode = new UserDetail
//                     {
//                         Id = Convert.ToInt32(token.Payload["Id"]),
//                         Name = token.Payload["Name"].ToString(),
//                         Status = Convert.ToInt32(token.Payload["Status"])
//                     };
//                     using (SqlConnection connection = new SqlConnection(Configuration["ConnectionStrings:DevConnection"]))
//                     {
//                         string queryString = "select [Token] from [ArticleDB].[dbo].[Token] where [User_Id]=@User_Id";
//                         SqlCommand command = new SqlCommand(queryString, connection);
//                         command.Parameters.AddRange(new SqlParameter[]
//                         {
//                                         new SqlParameter("@User_Id", SqlDbType.Int) { Value = decode.Id }
//                         });
//                         connection.Open();
//                         using (SqlDataReader reader = command.ExecuteReader())
//                         {
//                             if (!reader.HasRows)
//                             {
//                                 Response<string> result = new Response<string>();
//                                 result.StatusCode = Status.Forbidden;
//                                 result.Message = nameof(Status.Forbidden);
//                                 result.Data = null;
//                                 context.Response.StatusCode = 200;
//                                 context.Response.ContentType = "application/json";
//                                 context.Response.WriteAsync(JsonConvert.SerializeObject(result));
//                                 return Task.CompletedTask;
//                             }
//                             if (reader.Read())
//                             {
//                                 if (reader.GetString("Token") != authorization)
//                                 {
//                                     Response<string> result = new Response<string>();
//                                     result.StatusCode = Status.Forbidden;
//                                     result.Message = nameof(Status.Forbidden);
//                                     result.Data = null;
                                    // context.Response.StatusCode = 200;
                                    // context.Response.ContentType = "application/json";
//                                     context.Response.WriteAsync(JsonConvert.SerializeObject(result));
//                                     return Task.CompletedTask;
//                                 }
//                             }
//                             reader.Close();
//                         }
//                         connection.Close();
//                     }
//                     context.HttpContext.Items.Add("Token", decode);
//                     return Task.CompletedTask;
//                 }
//                 catch (Exception e)
//                 {
//                     Console.WriteLine($"ERROR: {e.Message}");
//                     Response<string> result = new Response<string>();
//                     result.StatusCode = Status.SystemError;
//                     result.Message = nameof(Status.SystemError);
//                     result.Data = null;
//                     context.Response.StatusCode = 200;
//                     context.Response.ContentType = "application/json";
//                     context.Response.WriteAsync(JsonConvert.SerializeObject(result));
//                     return Task.CompletedTask;
//                 }
//             }
//         };
//     });