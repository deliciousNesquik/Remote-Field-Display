namespace RFD.Models
{
    public class InfoStatus
    {
        public string Header { get; set; }
        public string Status { get; set; }

        public InfoStatus(string _Header, string _Status)
        {
            Header = _Header;
            Status = _Status;
        }
    }
}
