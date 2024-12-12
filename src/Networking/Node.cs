using System.Net;
using System.Net.Sockets;
using Blockchain.Models;
using Transaction = System.Transactions.Transaction;

namespace Blockchain.Networking;

public class Node
{
    private TcpListener _listener;
    private List<TcpClient> _connectedPeers = new List<TcpClient>();
    private object _lock = new object();
    private bool _running = false;
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
                    lock (_lock) { _connectedPeers.Add(client); }
                    Console.WriteLine("New peer connected: " + client.Client.RemoteEndPoint);
                    Task.Run(() => HandleClient(client));
                }
                catch (ObjectDisposedException) { }
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

                // HandleMessage(msgBuffer, client);
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
}