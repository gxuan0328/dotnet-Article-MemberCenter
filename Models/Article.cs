using System;
using System.ComponentModel.DataAnnotations;

public class Article
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public int User_Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Content { get; set; }
    [Required]
    public DateTime CreateDatetime { get; set; }
    [Required]
    public DateTime UpdateDatetime { get; set; }
}

public class NewArticle
{
    [Required]
    public string Title { get; set; }
    [Required]
    public int User_Id { get; set; }
    [Required]
    public string Content { get; set; }
}

public class ArticleId
{
    public int Id { get; set; }
}
public class Articles
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Name { get; set; }
    public DateTime CreateDatetime { get; set; }
}
public class Result
{
    public int Id { get; set; }
    public string Title { get; set; }
}
public class Search
{
    [Required(AllowEmptyStrings = true)]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string Title { get; set; }
    [Required(AllowEmptyStrings = true)]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string Author { get; set; }
    [Required]
    public DateTime FromDate { get; set; }
    [Required]
    public DateTime ToDate { get; set; }
}