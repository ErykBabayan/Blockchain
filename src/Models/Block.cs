using System.Security.Cryptography;
using System.Text;

namespace Blockchain.Models;

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

        var rawData = Index + CreatedAt.ToString("O") + PreviousHash + Nonce +
                      string.Join(",", Transactions.Select(t => t.ToString()));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

        return BytesToHex(bytes);
    }

    public byte[] CalculateHashBytes()
    {
        var rawData = Index + CreatedAt.ToString("O") + PreviousHash + Nonce +
                      string.Join(",", Transactions.Select(t => t.ToString()));
    
        return SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
    }

    private static string BytesToHex(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);

        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}