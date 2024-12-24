namespace RFD.Models
{
    public class InfoStatus
    {
        public string Header { get; set; }
        public bool Status { get; set; }

        public InfoStatus(string header, bool status)
        {
            Header = header;
            Status = status;
        }
    }
}
