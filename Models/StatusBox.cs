namespace RFD.Models
{
    public class StatusBox
    {
        public string Header { get; set; }
        public bool Status { get; set; }
        public StatusBox(string header, bool status)
        {
            Header = header;
            Status = status;
        }
    }
}
