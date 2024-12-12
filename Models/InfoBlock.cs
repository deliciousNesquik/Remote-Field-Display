namespace RFD.Models
{
    public class InfoBlock
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Inscription { get; set; }

        public InfoBlock(string _Title, string _Content, string _Inscription) 
        {
            Title = _Title;
            Content = _Content;
            Inscription = _Inscription;
        }
    }
}
