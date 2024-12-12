using System.Numerics;

namespace Blockchain.Models;

public class Blockchain
{
    public List<Block> Chain { get; set; } = [CreateGenesisBlock()]; // Blockchain is created with a list of block and 1st one being Genesis Block
    public int Difficulty { get; set; } = 2; // Simplified version. Could be remade into dynamic difficulty adjustment just like real blockchain does
    
    private static Block CreateGenesisBlock()
    {
        // Genesis block has an empty list of transactions holds no value
        var block = new Block(DateTime.UtcNow, [], "0")
        {
            Index = 0
        };

        return block;
    }

    private Block GetLatestBlock()
    {
        return Chain.Last();
    }

    public void AddBlock(Block block)
    {
        block.Index = Chain.Count;
        block.PreviousHash = GetLatestBlock().Hash;

        MineBlock(block);
        Chain.Add(block);
    }
    
    public bool IsChainValid()
    {
        for (var i = 1; i < Chain.Count; i++)
        {
            var currentBlock = Chain[i];
            var previousBlock = Chain[i - 1];

            if (currentBlock.Hash != currentBlock.CalculateHash()) return false;
            if (currentBlock.PreviousHash != previousBlock.Hash) return false;
        }
        return true;
    }

    // Calculates target based on the difficulty, for simplicity, let's say difficulty = nr of leading zero hex chars required
    // Difficulty 5 means 20 leading zero bits in a given hash
    private BigInteger CalculateTarget()
    {
        var maxVal = (BigInteger.One << 256) - 1;
        var shiftBits = 4 * Difficulty; // each unit is 4 bits

        var target = maxVal >> shiftBits;

        return target;
    }

    public void MineBlock(Block block)
    {
        var target = CalculateTarget();

        BigInteger hashValue;

        // Loop until hashValue <= target
        // Convert hash bytes to BigInteger for comparison
        // BigInteger in .NET assumes little-endian by default, so we might need to reverse.
        // SHA256 is big-endian, so we reverse bytes before creating the BigInteger.
        do
        {
            block.Nonce++;
            var hashBytes = block.CalculateHashBytes();
            
            // Reverse because BigInteger expects the least significant byte first
            var reversed = (byte[])hashBytes.Clone();
            Array.Reverse(reversed);
            hashValue = new BigInteger(reversed, isUnsigned: true, isBigEndian: false);
            
        } while (hashValue > target);
        
        Console.WriteLine($"Block mined with hash: {block.Hash}");
    }

    // private string BytesToBinary(byte[] bytes)
    // {
    //     return string.Join("", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    // }
}