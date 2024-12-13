using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using Blockchain.Models;
using Transaction = Blockchain.Models.Transaction;

namespace Blockchain.Networking;

public class Node
{
    private TcpListener _listener;
    private List<TcpClient> _connectedPeers = new List<TcpClient>();
    private object _lock = new object();
    private bool _running;
    private int _port;

    private List<Transaction> _mempool = new List<Transaction>();
    private Models.Blockchain _blockchain = new Models.Blockchain();

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

    private void HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[4];
        
        try
        {
            while (_running && client.Connected)
            {
                var bytesRead = stream.Read(buffer, 0, 4);
                if (bytesRead == 0) break;
                if (bytesRead < 4) break;

                var msgLength = BitConverter.ToInt32(buffer, 0);
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

                 //HandleMessage(msgBuffer); //TODO
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
    public void PrintBlockchain()
    {
        Console.WriteLine("Current Blockchain:");
        foreach (var b in _blockchain.Chain)
        {
            Console.WriteLine($"Index: {b.Index}, Hash: {b.Hash.Substring(0, 16)}..., Prev: {b.PreviousHash.Substring(0, 16)}..., TxCount: {b.Transactions.Count}");
        }
        Console.WriteLine("----------");
    }
}