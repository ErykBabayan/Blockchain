namespace Blockchain.Models;

public class Transaction
{
    public string From { get; set; }
    
    public string To { get; set; }
    
    public decimal Amount { get; set; }

    public override string ToString()
    {
        return $"{From}>{To}:{Amount}";
    }
}