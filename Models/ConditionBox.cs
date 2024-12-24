namespace RFD.Models;

public class ConditionBox
{
    public string Header { get; set; }
    public bool Condition { get; set; }
    public ConditionBox(string header, bool condition)
    {
        Header = header;
        Condition = condition;
    }
}