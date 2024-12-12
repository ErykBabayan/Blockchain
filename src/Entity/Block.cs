using System.Security.Cryptography;
using System.Text;

namespace Blockchain.Entity;

public class Block
{
    public int Index { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PreviousHash { get; set; }
    public string Hash { get; set; }
    public List<Transaction> Transactions { get; set; }
    public long Nonce { get; set; }

    public Block(
        DateTime createdAt,
        List<Transaction> transactions,
        string previousHash = ""
    )
    {
        CreatedAt = createdAt;
        Transactions = transactions;
        PreviousHash = previousHash;
        Hash = CalculateHash();
    }

    public string CalculateHash()
    {
        var sha256 = SHA256.Create();

        var rawData = Index + CreatedAt.ToString("O") + PreviousHash + Nonce +
                      string.Join(",", Transactions.Select(t => t.ToString()));

        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

        Console.WriteLine(bytes);
        Console.WriteLine(BytesToHex(bytes));
        
        return BytesToHex(bytes);
    }
    
    private string BytesToHex(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);

        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}