namespace DirectiveAthena.Website.Models;

public class Post {
    public string Slug { get; set; } = "";
    public Dictionary<string, string> Title { get; set; } = new();
    public Dictionary<string, string> Summary { get; set; } = new();
    public string Date { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public string File { get; set; } = "";
}