using BlockchainAssignment.HashCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BlockchainAssignment
{
    class Block
    {
        public DateTime timestamp;              //saves the current time.
        public int index, nonce;                //index of Block position in the Blockchain, nonce is a variable utilised in mining.
        public string hash, lastHash, merkleRoot;               //hash variable of the current block and for the last block.
        public List<Transaction> TransactionList = new List<Transaction>();     //List of transactions in the block.
        public static float standardReward = 1;
        public TimeSpan blockTime, mineTime;

        private static object hashLock = new object();
        private static bool hashFound = false;

        public static TimeSpan averageBlockTime = TimeSpan.FromSeconds(10);
        public static TimeSpan targetBlockTime = TimeSpan.FromSeconds(10);




        //PRIMARY CONSTRUCTOR this constructor is the primary constructor, but is only used to generate the initial genesis block.
        public Block()
        {
            timestamp = DateTime.Now;
            index = 0;
            lastHash = "";
            hash = CreateHash(nonce);
        }



        //SECONDARY CONSTRUCTOR used to create new mined blocks with transactions and mining reward.
        public Block(Block lastBlock, List<Transaction> Transactions, string MinerPublicID)
            : this()
        {
            timestamp = DateTime.Now;
            index = lastBlock.index + 1;
            lastHash = lastBlock.hash;
            nonce = 0;

            TransactionList = Transactions;
            TransactionList.Add(GenerateReward(MinerPublicID));

            List<string> TransactionHashes = new List<string>();
            for (int i = 0; i < TransactionList.Count; i++)
            {
                TransactionHashes.Add(TransactionList[i].hash);
            }

            merkleRoot = MerkleRoot(TransactionHashes);

            Stopwatch stopwatch = Stopwatch.StartNew();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            int threads = Environment.ProcessorCount;
            Task[] miners = new Task[threads];

            hashFound = false;

            for (int i = 0; i < threads; i++)
            {
                int threadStart = i;
                miners[i] = Task.Run(() =>
                {
                    int localNonce = threadStart;
                    while (!token.IsCancellationRequested)
                    {
                        string trialHash = CreateHash(localNonce);
                        if (trialHash.StartsWith(new string('0', Blockchain.difficulty)))
                        {
                            lock (hashLock)
                            {
                                if (!hashFound)
                                {
                                    hashFound = true;
                                    nonce = localNonce;
                                    hash = trialHash;
                                    tokenSource.Cancel();
                                }
                            }
                            break;
                        }
                        localNonce += threads;
                    }
                });
            }

            Task.WaitAll(miners);
            stopwatch.Stop();

            mineTime = stopwatch.Elapsed;
            blockTime = SetBlockTime();   // calculates the time difference between this block and the previous one (includes mining delay)
            averageBlockTime = TimeSpan.FromSeconds(((averageBlockTime.TotalSeconds * (index - 1)) + blockTime.TotalSeconds) / index);

            AdjustDifficulty();  // adjusts the mining difficulty for the next block depending on the block time
        }



        //METHOD adjusts the mining difficulty for the next block depending on the block time
        public void AdjustDifficulty()
        {
            if (blockTime < targetBlockTime)
            {
                Blockchain.difficulty += 1;
            }
            else if (blockTime > targetBlockTime && Blockchain.difficulty > 1)
            {
                Blockchain.difficulty -= 1;
            }
        }



        //METHOD creates a SHA256 hash using block fields and current nonce
        public string CreateHash(int currNonce)
        {
            SHA256 hasher = SHA256.Create();
            string input = index.ToString() + timestamp.ToString() + lastHash + merkleRoot + currNonce.ToString();
            byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            string hash = string.Empty;
            foreach (byte x in hashByte)
                hash += string.Format("{0:x2}", x);
            return hash;
        }



        //METHOD returns a formatted string summarising the block's data and transactions
        public string ReturnBlock()
        {
            string printTransactions = "";

            for (int i = 0; i < TransactionList.Count; i++)
            {
                printTransactions += ("\n\n\nTRANSACTION " + (i + 1) + " -\n" + TransactionList[i].PrintTransaction());
            }

            return (string.Concat(
                "------------------------------------------------------------------------------------------------------------------------------------------------------------" +
                "\n\nindex: " + index.ToString() +
                "\nprevious hash: " + lastHash +
                "\ncurrent hash: " + hash +
                "\nmerkle root: " + merkleRoot +
                "\ndifficulty: " + Blockchain.difficulty.ToString() +
                "\nnonce: " + nonce.ToString() +
                "\ntimestamp: " + timestamp.ToString() +
                "\nmine time: " + mineTime.ToString() +
                "\nblock time: " + blockTime.ToString() +
                "\naverage block time: " + averageBlockTime.ToString() +
                printTransactions
            ));
        }



        //METHOD recursively calculates the Merkle root from transaction hashes
        public string MerkleRoot(List<string> TransactionHashes)
        {
            List<string> merkleLeaves = new List<string>();

            if (TransactionHashes.Count % 2 != 0)
            {
                for (int i = 0; i < TransactionHashes.Count; i++)
                {
                    if (i < TransactionHashes.Count - 1)
                    {
                        merkleLeaves.Add(HashTools.CombineHash(TransactionHashes[i], TransactionHashes[i + 1]));
                        i++;
                    }
                    else
                    {
                        merkleLeaves.Add(TransactionHashes[i]);
                    }
                }
            }
            else if (TransactionHashes.Count != 0)
            {
                for (int i = 0; i < TransactionHashes.Count; i++)
                {
                    merkleLeaves.Add(HashTools.CombineHash(TransactionHashes[i], TransactionHashes[i + 1]));
                    i++;
                }
            }
            else
            {
                return null;
            }

            if (merkleLeaves.Count != 1)
            {
                return MerkleRoot(merkleLeaves);  //recursive call
            }
            else
            {
                return merkleLeaves[0];
            }
        }



        //METHOD creates a reward transaction for the miner by summing transaction fees and adding base reward
        public Transaction GenerateReward(string MinerPublicID)
        {
            float mineReward = 0;

            for (int i = 0; i < TransactionList.Count; i++)
            {
                mineReward += TransactionList[i].fee;
            }

            mineReward += standardReward;
            return new Transaction("Mining_Reward", MinerPublicID, mineReward, 0, "");
        }



        //METHOD calculates the time difference between this block and the previous one (includes mining delay)
        public TimeSpan SetBlockTime()
        {
            DateTime blockMinedTime = timestamp + mineTime;
            DateTime prevBlockMinedTime = Blockchain.Blocks[index - 1].timestamp + Blockchain.Blocks[index - 1].mineTime;
            return blockMinedTime - prevBlockMinedTime;
        }
    }
}
