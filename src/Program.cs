namespace Blockchain;

using Models;

public static class Program
{
    public static void Main(string[] args)
    {
        var blockchain = new Blockchain();
        
        var block = new Block(DateTime.UtcNow, [], "dupa");
        
        blockchain.MineBlock(block);
    }
}