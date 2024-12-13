namespace Blockchain.Models;

public class Transaction
{
    // The public key of the sender //TODO
    public string From { get; set; }
    
    // The public key of the receiver //TODO
    public string To { get; set; }
    
    // The amount of the transaction
    public decimal Amount { get; set; }
    
    // // The signature of the transaction //TODO 
    // public int Signature { get; set; }

    public override string ToString()
    {
        return $"{From}>{To}:{Amount}";
    }
}