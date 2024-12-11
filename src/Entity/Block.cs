namespace Blockchain.Entity;

public class Block
{
    public int Index { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string PreviousHash { get; set; }
    public required string Hash { get; set; }
    public required List<Transaction> Transactions { get; set; }
    public long Nonce { get; set; }

    public Block() //TODO
    {
        //TODO
    }

    public string CalculateHash()
    {
        return ""; //TODO
    }
    
    private string BytesToHex(byte[] bytes)
    {
        return ""; //TODO
    }
}