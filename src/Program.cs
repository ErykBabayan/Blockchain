using Blockchain.Networking;

namespace Blockchain;

public static class Program
{
    public static void Main(string[] args)
    {
        var port = 5000;
        var isMiner = false;
        var initialPeers = new List<string>();

        // Parse arguments
        // Example usage:
        // dotnet run -- 5000 --miner
        // dotnet run -- 5001 127.0.0.1:5000
        foreach (var t in args)
        {
            if (t == "--miner")
            {
                isMiner = true;
            }
            else if (int.TryParse(t, out int p))
            {
                // First integer argument is port
                if (port == 5000) port = p; // If port wasn't already set
            }
            else if (t.Contains(":"))
            {
                // Peer address
                initialPeers.Add(t);
            }
        }

        var node = new Node(port, isMiner);
        node.Start();

        foreach (var peer in initialPeers)
        {
            var parts = peer.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int peerPort))
            {
                node.ConnectToPeer(parts[0], peerPort);
            }
        }
        
        Console.WriteLine($"Node started on port {port}. Commands:");
        Console.WriteLine("  send From->To:Amount : Add a transaction to the mempool");
        Console.WriteLine("  mine                : (Miner only) Mine a new block and broadcast it");
        Console.WriteLine("  peers               : List connected peers");
        Console.WriteLine("  chain               : Print the blockchain");
        Console.WriteLine("  exit                : Quit");

        // If this is a miner, start a background loop that tries to mine periodically
        if (isMiner)
        {
            // Start a background thread that checks the mempool and mines if there are pending tx
            new Thread(() =>
            {
                while (true)
                {
                    // Sleep for a fixed interval before checking mempool
                    Thread.Sleep(5000); 
                    node.AutoMineIfTransactions(); 
                }
            })
            {
                IsBackground = true
            }.Start();
        }
        
        
        while (true)
        {
            var input = Console.ReadLine();

            if (input == null) continue;

            var command = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            if (command.Length == 0) continue;
            
            switch (command[0].ToLower())
            {
                case "send":
                    if (command.Length > 1)
                        node.AddTransactionToMempool(command[1]);
                    else
                        Console.WriteLine("Usage: send From->To:Amount");
                    break;
                case "mine":
                    if (isMiner)
                        node.MineBlock();
                    else
                        Console.WriteLine("This node is not a miner.");
                    break;
                case "peers":
                    node.ListPeers();
                    break;
                case "chain":
                    node.PrintBlockchain();
                    break;
                case "exit":
                    node.Stop();
                    return;
                default:
                    Console.WriteLine("Unknown command provided");
                    break;
            }


        }
        
    }
}