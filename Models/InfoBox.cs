namespace RFD.Models;
public class InfoBox
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Inscription { get; set; }

    public InfoBox(string title, string content, string inscription) 
    {
        Title = title;
        Content = content;
        Inscription = inscription;
    }
}