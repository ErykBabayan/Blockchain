using Blockchain.Networking;

namespace Blockchain;

using Models;

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
            var parts = peer.Split(':');

            if (parts.Length == 2 && int.TryParse(parts[1], out var p))
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
                    Console.WriteLine("Send");
                    break;
                case "mine":
                    Console.WriteLine("Mine");
                    break;
                case "peers":
                    Console.WriteLine("PeersList");
                    break;
                case "chain":
                    Console.WriteLine("THE CHAIN");
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