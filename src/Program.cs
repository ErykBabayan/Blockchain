using Blockchain.Networking;

namespace Blockchain;

public static class Program
{
    public static void Main(string[] args)
    {
        var port = 5000;
        var initialPeers = new List<string>();

        if (args.Length > 0)
        {
            int.TryParse(args[0], out port);
        }

        for (var i = 1; i < args.Length; i++)
        {
            initialPeers.Add(args[i]);
        }

        var node = new Node(port);
        node.Start();

        foreach (var peer in initialPeers)
        {
            Console.WriteLine(peer);
            var parts = peer.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int p))
            {
                node.ConnectToPeer(parts[0], p);
            }
        }
        
        Console.WriteLine("Node started. Commands:");
        Console.WriteLine("  send From->To:Amount : Add a transaction to the mempool");
        Console.WriteLine("  mine                : Mine a new block and broadcast it");
        Console.WriteLine("  peers               : List connected peers");
        Console.WriteLine("  chain               : Print the blockchain");
        Console.WriteLine("  exit                : Quit");

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
                    node.MineBlock();
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