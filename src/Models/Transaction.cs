namespace Blockchain.Models;

public class Transaction
{
    public required string From { get; set; }
    
    public required string To { get; set; }
    
    public decimal Amount { get; set; }

    public override string ToString()
    {
        return $"{From}>{To}:{Amount}";
    }
}