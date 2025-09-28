# Localised Blockchain Emulation Software (LBES)

## Overview

This project implements a **localised blockchain system in C#** that emulates key aspects of decentralised networks, including **mining, optional mining strategies, transaction validation, and adaptive difficulty**. It features a **GUI interface** for wallets and transactions, **cryptographic key generation**, **digital signatures**, and **multithreaded mining**, providing a professional-level demonstration of blockchain principles in a controlled environment.

## Features

* **Genesis and mined blocks** with complete transaction lists.
* **Adaptive mining difficulty** based on target block time.
* **Transaction management** with pending queues and multiple selection strategies:

  * FIFO (Altruistic)
  * Random
  * Highest-fee prioritisation (Greedy)
  * Receiver-focused selection (Addressed)
* **Wallet generation** with ECDSA key pairs.
* **Transaction signing** and **signature validation**.
* **Merkle root calculation** for block integrity verification.
* **Multithreaded mining** for efficient hash discovery.
* **Detailed blockchain metrics**:

  * Average block time
  * Average mining time
  * Average nonce rate
  * Predicted block time
* **GUI integration** via Windows Forms for user-friendly interaction.

## Usage

1. **Run the program** using Visual Studio or your preferred C# IDE.

2. The **GUI** allows you to:
  - Create wallets
  - View balances
  - Send transactions
  - Mine blocks
  - And more:
<img width="683" height="513" alt="image" src="https://github.com/user-attachments/assets/59fc29c7-f6e4-46bf-a5ae-ea605a08d3bb" />

3. Transactions are automatically **validated** and added to the **pending transactions pool**.

4. Mining occurs with **multithreaded hash computation**.
5. Use the GUI to monitor blockchain statistics, wallet balances, and transaction histories.

## Technical Highlights

- **Block Structure**: Each block contains an index, timestamp, nonce, hash, last hash, Merkle root, and a list of transactions, providing a full record of blockchain state.  
- **Merkle Root Calculation**: Ensures block integrity by recursively hashing transaction pairs, allowing tamper-evident verification of all transactions within a block.  
- **Mining & Difficulty Adjustment**: Implements Proof-of-Work style mining with nonce iteration; mining difficulty is dynamically adjusted based on block generation time to maintain a consistent target block time.  
- **Wallet & Signature Handling**: Generates cryptographic key pairs for wallets, signs transactions with private keys, and validates signatures for security and authenticity.  
- **Multithreaded Mining**: Utilises multiple processor threads to mine blocks concurrently, improving efficiency and simulating real-time mining processes.


## Project Structure

```
/Localised-Blockchain-Emulation-Software
│
└─ /BlockchainAssignment
    ├─ Block.cs
    ├─ Blockchain.cs
    ├─ BlockchainApp.cs
    ├─ Transaction.cs
    ├─ Program.cs
    ├─ App.config
    ├─ /Wallet
        └─ Wallet.cs
    └─ /HashCode
        └─ HashTools.cs
```

* **Block.cs** - Block creation, mining, Merkle root, and time tracking.
* **Blockchain.cs** - Blockchain ledger management, transaction selection, difficulty adjustment, and statistics.
* **BlockchainApp.cs** - Windows Forms graphical UI setup.
* **Transaction.cs** - Transaction creation, hashing, and printable summaries.
* **Wallet/Wallet.cs** - Public/private key generation, signing, and validation.
* **HashCode/HashTools.cs** - Utility methods for hashing and Merkle root calculation.
* **Program.cs & App.config** - Entry point and runtime configuration.

## Technical Achievements

* Emulates **decentralised consensus** through local multithreaded mining.
* Implements **transaction signing** using ECDSA and validates integrity in real time.
* Provides **adaptive difficulty adjustment**, ensuring consistent block timing.
* Offers a **visual interface** for blockchain interactions.
* Demonstrates **professional C# software engineering skills**:

  * Object-oriented design
  * Multithreading
  * Cryptography
  * GUI development
