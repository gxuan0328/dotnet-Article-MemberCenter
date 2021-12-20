using System;
using System.ComponentModel.DataAnnotations;

public class Article
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    public int User_ID { get; set; }
    public string Name { get; set; }
    [Required]
    public string Content { get; set; }
    public DateTime CreateDatetime { get; set; }
    public DateTime UpdateDatetime { get; set; }
    public int Editor { get; set; }
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
public class Search
{
    public int Id { get; set; }
    public string Title { get; set; }
}