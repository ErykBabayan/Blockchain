using System.Net;
using System.Net.Sockets;
using System.Text;
using Blockchain.Models;
using Transaction = Blockchain.Models.Transaction;

namespace Blockchain.Networking;

public class Node
{
    private readonly TcpListener _listener;
    private readonly List<TcpClient> _connectedPeers = new List<TcpClient>();
    private readonly object _lock = new object();
    private readonly int _port;
    private bool _running;

    private readonly List<Transaction> _mempool = new List<Transaction>();
    private readonly Models.Blockchain _blockchain = new Models.Blockchain();

    public Node(int port)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public void Start()
    {
        _listener.Start();
        _running = true;

        Console.WriteLine($"Node listening on port {_port}");

        Task.Run(async () =>
        {
            while (_running)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    lock (_lock)
                    {
                        _connectedPeers.Add(client);
                    }

                    Console.WriteLine("New peer connected: " + client.Client.RemoteEndPoint);
                    await Task.Run(() => HandleClient(client));
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("Error occurred while accepting client.");
                    throw new Exception("Listener stopped.");
                }
            }
        });
    }

    public void Stop()
    {
        _running = false;
        _listener.Stop();

        lock (_lock)
        {
            foreach (var client in _connectedPeers)
            {
                client.Close();
            }
            _connectedPeers.Clear();
        }
        Console.WriteLine("Node Stopped.");
    }

    public void ConnectToPeer(string host, int port)
    {
        try
        {
            var client = new TcpClient();
            client.Connect(host, port);
            lock (_lock)
            {
                _connectedPeers.Add(client);
            }
            Console.WriteLine($"Connected to peer: {host}:{port}");
            Task.Run(() => HandleClient(client));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to connect to {host}:{port} - {e.Message}");
            throw;
        }
    }
    
    public void ListPeers()
    {
        lock (_lock)
        {
            Console.WriteLine("Connected peers:");
            foreach (var p in _connectedPeers)
            {
                Console.WriteLine("- " + p.Client.RemoteEndPoint);
            }
        }
    }
    public void PrintBlockchain()
    {
        Console.WriteLine("Current Blockchain:");
        foreach (var b in _blockchain.Chain)
        {
            var hash = b.Hash ?? "";
            var prevHash = b.PreviousHash ?? "";

            // Safely get a prefix of the hash
            var shortHash = hash.Length >= 16 ? hash.Substring(0, 16) : hash;
            var shortPrevHash = prevHash.Length >= 16 ? prevHash.Substring(0, 16) : prevHash;

            var output =
                $"Index: {b.Index}, Hash: {shortHash}..., Prev: {shortPrevHash}..., TxCount: {b.Transactions.Count}";
            
            if (b.Index == 0)
            {
                output += " (Genesis Block)";
            }
            
            Console.WriteLine(output);
        }
        Console.WriteLine("----------");
    }
    
    //////////////
    ///
    
    private void HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var lengthBuffer = new byte[4];

        try
        {
            while (_running && client.Connected)
            {
                var bytesRead = stream.Read(lengthBuffer, 0, 4);
                if (bytesRead == 0) break;
                if (bytesRead < 4) break;

                var msgLength = BitConverter.ToInt32(lengthBuffer, 0);
                if (msgLength <= 0) break;

                var msgBuffer = new byte[msgLength];
                var totalRead = 0;
                while (totalRead < msgLength)
                {
                    var chunkSize = stream.Read(msgBuffer, totalRead, msgLength - totalRead);
                    if (chunkSize == 0) break;
                    totalRead += chunkSize;
                }

                if (totalRead < msgLength) break;

                HandleMessage(msgBuffer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from peer {client.Client.RemoteEndPoint}: {ex.Message}");
        }

        lock (_lock) { _connectedPeers.Remove(client); }
        client.Close();
        Console.WriteLine("Peer disconnected: " + client.Client.RemoteEndPoint);
    }
    
    
    private void HandleMessage(byte[] msgBuffer)
    {
        if (msgBuffer.Length < 4) return;
        var messageType = Encoding.ASCII.GetString(msgBuffer, 0, 4);

        var payload = new byte[msgBuffer.Length - 4];
        Array.Copy(msgBuffer, 4, payload, 0, payload.Length);

        switch (messageType.Trim())
        {
            case "TX":
                HandleTransactionMessage(payload);
                break;
            case "BLCK":
                HandleBlockMessage(payload);
                break;
            default:
                Console.WriteLine($"Received unknown message type: {messageType}");
                break;
        }
    }
    
    private void HandleTransactionMessage(byte[] payload)
    {
        var trxData = Encoding.UTF8.GetString(payload);
        var parts = trxData.Split(["->", ":"], StringSplitOptions.None);

        // We should be getting 3 parts From -> To and : Amount
        if (parts.Length != 3) return;
        
        var tx = new Transaction { From = parts[0], To = parts[1], Amount = decimal.Parse(parts[2]) };
        Console.WriteLine("Received Transaction: " + tx);
        lock (_mempool) { _mempool.Add(tx); }
    }
    
    private void HandleBlockMessage(byte[] payload)
        {
            var data = Encoding.UTF8.GetString(payload);
            var parts = data.Split(';');

            if (parts.Length < 5)
            {
                Console.WriteLine("Received malformed block");
                return;
            }

            var index = int.Parse(parts[0]);
            var timestamp = DateTime.Parse(parts[1]);
            var prevHash = parts[2];
            var nonce = long.Parse(parts[3]);
            var txStrings = parts[4].Split('|').Where(s => !string.IsNullOrEmpty(s)).ToList();
            
            var transactions = (
                from tStr 
                in txStrings 
                select tStr.Split(["->", ":"], StringSplitOptions.None) 
                into tParts 
                where tParts.Length == 3 
                select new Transaction { From = tParts[0], To = tParts[1], Amount = decimal.Parse(tParts[2]) }
                ).ToList();
            
            // foreach version of the above LINQ query for reference, we are creating a list of transactions
            // foreach (var tStr in txStrings)
            // {
            //     var tParts = tStr.Split(["->", ":"], StringSplitOptions.None);
            //     if (tParts.Length == 3)
            //     {
            //         transactions.Add(new Transaction { From = tParts[0], To = tParts[1], Amount = decimal.Parse(tParts[2]) });
            //     }
            // }

            var block = new Block(timestamp, transactions, prevHash)
            {
                Index = index,
                Nonce = nonce
            };
            block.Hash = block.CalculateHash();

            if (ValidateAndAddBlock(block))
            {
                Console.WriteLine("New block added from peer: " + block.Hash);
                PrintBlockchain();
            }
            else
            {
                Console.WriteLine("Received invalid block. Ignoring.");
            }
        }

    private bool ValidateAndAddBlock(Block block)
    {
        var latest = _blockchain.GetLatestBlock();
        Console.WriteLine(block.PreviousHash);
        Console.WriteLine(latest.Hash);
        if (block.PreviousHash != latest.Hash)
        {
            Console.WriteLine("Received block with invalid previous hash.");
            return false;
        }

        // This is our target difficulty staring with 0000... based on setting made in Blockchain.cs
        var target = new string('0', _blockchain.Difficulty);
        
        if (!block.Hash.StartsWith(target))
        {
            Console.WriteLine("Received block with invalid hash.");
            return false;
        }

        if (block.Hash != block.CalculateHash())
        {
            Console.WriteLine("Received block with invalid hash.");
            return false;
        }

        _blockchain.Chain.Add(block);
        return true;
    }

    public void BroadcastTransaction(Transaction tx)
    {
        var txData = $"{tx.From}->{tx.To}:{tx.Amount}";
        var payload = Encoding.UTF8.GetBytes(txData);
        BroadcastMessage("TX  ", payload);
    }

    private void BroadcastBlock(Block block)
    {
        var txData = string.Join("|", block.Transactions.Select(t => t.ToString()));
        var blockData = $"{block.Index};{block.CreatedAt};{block.PreviousHash};{block.Nonce};{txData}";
        var payload = Encoding.UTF8.GetBytes(blockData);
        BroadcastMessage("BLCK", payload);
    }

    private void BroadcastMessage(string messageType, byte[] payload)
    {
        messageType = messageType.PadRight(4, ' ');
        if (messageType.Length > 4) messageType = messageType.Substring(0, 4);

        var typeBytes = Encoding.ASCII.GetBytes(messageType);
        var msg = new byte[4 + payload.Length];
        Array.Copy(typeBytes, 0, msg, 0, 4);
        Array.Copy(payload, 0, msg, 4, payload.Length);

        var lengthBytes = BitConverter.GetBytes(msg.Length);

        lock (_lock)
        {
            var toRemove = new List<TcpClient>();
            foreach (var peer in _connectedPeers)
            {
                try
                {
                    var stream = peer.GetStream();
                    stream.Write(lengthBytes, 0, 4);
                    stream.Write(msg, 0, msg.Length);
                    stream.Flush();
                }
                catch
                {
                    Console.WriteLine("Failed to send message to a peer, removing them.");
                    toRemove.Add(peer);
                }
            }
            foreach (var p in toRemove)
            {
                _connectedPeers.Remove(p);
                p.Close();
            }
        }
    }
    
    public void AddTransactionToMempool(string fromToAmount)
    {
        var parts = fromToAmount.Split(["->", ":"], StringSplitOptions.None);
        if (parts.Length == 3)
        {
            var tx = new Transaction { From = parts[0], To = parts[1], Amount = decimal.Parse(parts[2]) };
            lock (_mempool) { _mempool.Add(tx); }
            Console.WriteLine($"Added transaction to mempool: {tx}");
        }
        else
        {
            Console.WriteLine("Transaction format incorrect. Use From->To:Amount");
        }
    }

    public void MineBlock()
    {
        List<Transaction> blockTxs;
        lock (_mempool)
        {
            blockTxs = [.._mempool];
            _mempool.Clear();
        }

        if (blockTxs.Count == 0)
        {
            Console.WriteLine("No transactions to mine.");
            return;
        }
        
        Console.WriteLine(_blockchain.GetLatestBlock());

        var block = new Block(DateTime.UtcNow, blockTxs, _blockchain.GetLatestBlock().Hash);
        _blockchain.AddBlock(block);
        Console.WriteLine($"Mined a new block: {block.Hash}");

        BroadcastBlock(block);
        PrintBlockchain();
    }
}