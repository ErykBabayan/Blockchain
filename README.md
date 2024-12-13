# Simple Blockchain in C#
This project is a simplified blockchain implementation written in C#. It demonstrates the core concepts of blockchain technology, including:

A distributed peer-to-peer network of nodes.
A simple Proof-of-Work consensus mechanism.
Transactions are broadcasted between peers.
Mining nodes that collect transactions into blocks and broadcast newly mined blocks.

## Note: This implementation is intentionally minimal and not production-ready. 
### It omits features like cryptographic signatures, longest chain rule, and transaction validation, security and many many many more

## Usage

Basically to use this simple blockchain you need to open 3 terminals ( or expose ports to the internet and run the nodes in different machines) and run the following commands:

Miner Node:
```bash
dotnet run -- {port} --miner
```
Non-Miner Node 1:
```bash
dotnet run -- {port} {minerIpAddres}:{minerPort}
```

```bash
dotnet run -- {port} {minerIpAddres}:{minerPort}
```


## Features

### Genesis Block:
A hardcoded genesis block ensures all nodes start from the same state.

### Blocks & Transactions:
Each block references the hash of the previous block, contains a timestamp, nonce, and a list of transactions. Transactions are simple records (e.g., From->To:Amount).

### Proof-of-Work (PoW):
Nodes must find a nonce that produces a block hash with a certain number of leading zeros. This simulates computational effort required to mine a block.

### P2P Network:
Nodes communicate using TCP sockets. Each node can:

Connect to known peers.
Broadcast transactions.
Receive and validate blocks mined by a miner node.


### Miner vs. Non-Miner Nodes:

Miner nodes continuously check their transaction pool (mempool) and mine blocks if transactions are available.
Non-miner nodes can send transactions to the miner node and receive newly mined blocks.