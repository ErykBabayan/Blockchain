using System.Numerics;

namespace Blockchain.Models;

public class Blockchain
{
    public List<Block> Chain { get; set; } = [CreateGenesisBlock()]; // Blockchain is created with a list of block and 1st one being Genesis Block
    public int Difficulty { get; set; } = 2; // Simplified version. Could be remade into dynamic difficulty adjustment just like real blockchain does
    
    private static Block CreateGenesisBlock()
    {
        // Genesis block has an empty list of transactions holds no value. No previous hash and index is 0.
        var block = new Block(DateTime.UtcNow, [], "0")
        {
            Index = 0,
        };
        
        block.Hash = block.CalculateHash();

        return block;
    }

    public Block GetLatestBlock()
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

    public void MineBlock(Block block)
    {
        // Target is a string of 0s with length of Difficulty. At current implementation sha256 hash is converted to hex string
        // so 1 hex char is equal to 4 bits. So 2 Difficulty is equal to 8 bits. 4 Difficulty is equal to 16 bits.
        var target = new string('0', Difficulty);
        
        while (!block.Hash.StartsWith(target))
        {
            block.Nonce++;
            block.Hash = block.CalculateHash();
        }
        Console.WriteLine($"Block mined with hash: {block.Hash}");
    }
}