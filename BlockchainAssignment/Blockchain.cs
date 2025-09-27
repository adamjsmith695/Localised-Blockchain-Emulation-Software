using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace BlockchainAssignment
{
    class Blockchain
    {
        //creating a list for storing Blocks that are in the Blockchain 
        public static List<Block> Blocks = new List<Block>();

        //creating a list for transactions that are pending to be processed into the Blockchain
        public static List<Transaction> pendingTransactions = new List<Transaction>();

        //variable for determining the strategy used to select transactions when mining
        public static int transactionSelectionType = 0;

        //difficulty variable (begins at 1)
        public static int difficulty = 1;

        //CONSTRUCTOR instantiates the genesis block
        public Blockchain()
        { Blocks.Add(new Block()); }      //instantiates a new genesis Block into the Blockchain

        //METHOD to return a Block's information in a readable and printable format from the Blockchain with a given index input -
        public static String PrintBlock(int index)
        {
            if (index > Blocks.Count - 1)
            {
                return ("NULL");
            }
            else
            {
                return Blocks[index].ReturnBlock();
            }
        }



        //METHOD adds the given transaction to the pending transactions list
        public static bool PendTransaction(Transaction transaction)
        {
            if (transaction.amount <= 0 || transaction.fee <= 0)
            {
                return false;
            }

            float balance = float.Parse(getBalance(transaction.sender, false));
            float pendingOut = 0;

            foreach (Transaction t in pendingTransactions)
            {
                if (t.sender == transaction.sender)
                {
                    pendingOut += t.amount + t.fee;
                }
            }

            float effectiveBalance = balance - pendingOut;

            if (effectiveBalance >= (transaction.amount + transaction.fee))
            {
                pendingTransactions.Add(transaction);
                return true;
            }
            else
            {
                return false;
            }
        }



        //METHOD returns the balance of a given publicID's wallet by checking each block for transactions involving said publicID.
        public static string getBalance(string publicID, bool print)
        {
            float walletBalance = 0;
            string transactions = "";

            // Calculate confirmed blockchain balance
            for (int i = 1; i < Blocks.Count; i++)
            {
                for (int j = 0; j < Blocks[i].TransactionList.Count; j++)
                {
                    if (Blocks[i].TransactionList[j].sender == publicID)
                    {
                        walletBalance -= Blocks[i].TransactionList[j].amount + Blocks[i].TransactionList[j].fee;
                        transactions += ("\n\n\n< SENT >\n\n" + Blocks[i].TransactionList[j].PrintTransaction());
                    }
                    if (Blocks[i].TransactionList[j].receiver == publicID)
                    {
                        walletBalance += Blocks[i].TransactionList[j].amount;
                        transactions += ("\n\n\n< RECEIVED >\n\n" + Blocks[i].TransactionList[j].PrintTransaction());
                    }
                }
            }

            // Calculate pending outgoing total
            float pendingOut = 0;
            foreach (Transaction t in pendingTransactions)
            {
                if (t.sender == publicID)
                {
                    pendingOut += t.amount + t.fee;
                }
            }

            float effectiveBalance = walletBalance - pendingOut;

            if (print == true)
            {
                return ("Public ID: " + publicID +
                        "\nConfirmed Balance: " + walletBalance.ToString("0.00") +
                        "\nPending Outgoing: " + pendingOut.ToString("0.00") +
                        "\n-------------------------------------------------------" +
                        "\n\nEffective Balance: " + effectiveBalance.ToString("0.00") +
                        "\n\n" + transactions);
            }
            else
            {
                return walletBalance.ToString();
            }
        }



        //METHOD recalculates a given blocks' merkle root to verify integrity
        public static bool ValidateMerkleRoot(Block block)
        {
            List<String> TransactionHashes = new List<String>();

            for (int i = 0; i < block.TransactionList.Count(); i++)
            {
                TransactionHashes.Add(block.TransactionList[i].hash);
            }
            String reMerkle = block.MerkleRoot(TransactionHashes);

            if (reMerkle == block.merkleRoot)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        //METHOD recalculates and verifies a block's hash for tamper detection
        public static bool ValidateHash(Block block)
        {
            String reHash = block.CreateHash(block.nonce);

            if (reHash == block.hash)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        //METHOD selects which transactions will be mined based on the chosen strategy
        public static List<Transaction> SelectTransactions(int n, [Optional] string address)
        {
            List<Transaction> selectedTransactions = new List<Transaction>();

            if (pendingTransactions.Count() < n)
            {
                n = pendingTransactions.Count();
            }

            if (transactionSelectionType == 0)
            {
                for (int i = 0; i < n; i++)
                {
                    selectedTransactions.Add(pendingTransactions[i]);
                }

                pendingTransactions = pendingTransactions.Except(selectedTransactions).ToList();
            }
            else if (transactionSelectionType == 1)
            {
                Random rand = new Random();

                for (int i = 0; i < n; i++)
                {
                    int randIndex = rand.Next(pendingTransactions.Count());
                    selectedTransactions.Add(pendingTransactions[randIndex]);
                    pendingTransactions.RemoveAt(randIndex);
                }
            }
            else if (transactionSelectionType == 2)
            {
                int max = 0;

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < pendingTransactions.Count() - 1; j++)
                    {
                        if (pendingTransactions[max].fee < pendingTransactions[j].fee)
                        {
                            max = j;
                        }
                    }
                    selectedTransactions.Add(pendingTransactions[max]);
                    pendingTransactions.RemoveAt(max);
                }
            }
            else if (transactionSelectionType == 3)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < pendingTransactions.Count() - 1; j++)
                    {
                        if (pendingTransactions[j].receiver == address)
                        {
                            selectedTransactions.Add(pendingTransactions[j]);
                            pendingTransactions.RemoveAt(j);
                        }
                    }
                }
            }

            return selectedTransactions;
        }



        //METHOD calculates the average time taken to generate blocks (excluding genesis block)
        public static TimeSpan GetAverageBlockTime()
        {
            double sum = 0;
            for (int i = 1; i < Blocks.Count(); i++)
            {
                sum += Blocks[i].blockTime.TotalSeconds;
            }
            if (sum != 0)
            {
                return TimeSpan.FromSeconds(sum / (Blocks.Count() - 1));
            }
            else
            {
                return TimeSpan.FromSeconds(0);
            }
        }



        //METHOD calculates the average actual mining duration across mined blocks
        public static TimeSpan GetAverageMineTime()
        {
            double sum = 0;
            for (int i = 1; i < Blocks.Count(); i++)
            {
                sum += Blocks[i].mineTime.TotalSeconds;
            }
            if (sum != 0)
            {
                return TimeSpan.FromSeconds(sum / (Blocks.Count() - 1));
            }
            else
            {
                return TimeSpan.FromSeconds(0);
            }
        }



        //METHOD calculates the average nonce increment rate during mining (nonces per second)
        public static double GetAverageMineRate()
        {
            double sum = 0;
            for (int i = 1; i < Blocks.Count(); i++)
            {
                sum += Blocks[i].nonce / Blocks[i].mineTime.TotalSeconds;
            }
            return (sum / (Blocks.Count() - 1));
        }



        //METHOD estimates expected block time based on difficulty and average mining rate
        public static TimeSpan GetPredictedBlockTime(int difficulty)
        {
            Console.WriteLine("16^" + difficulty.ToString() + ": " + Math.Pow(16, difficulty));
            Console.WriteLine("Average Mine Rate: " + GetAverageMineRate());
            Console.WriteLine("Predicted Block Time: " + Math.Pow(16, difficulty) / GetAverageMineRate());

            double avgMineRate = GetAverageMineRate();

            if (avgMineRate <= 0 || double.IsNaN(avgMineRate) || double.IsInfinity(avgMineRate))
            {
                return TimeSpan.FromSeconds(0);
            }

            return TimeSpan.FromSeconds(Math.Pow(16, difficulty) / avgMineRate);
        }



        //METHOD adjusts difficulty based on how current average block time compares to the target
        public static void UpdateDifficulty()
        {
            TimeSpan targetBlockTime = TimeSpan.FromSeconds(10);
            TimeSpan avgBlockTime = GetAverageBlockTime();

            Console.WriteLine($"Average Block Time: {avgBlockTime.TotalSeconds}s");
            Console.WriteLine($"Target Block Time: {targetBlockTime.TotalSeconds}s");

            if (avgBlockTime.TotalSeconds < targetBlockTime.TotalSeconds * 0.9)
            {
                difficulty++;
                Console.WriteLine("Increased difficulty to " + difficulty);
            }
            else if (avgBlockTime.TotalSeconds > targetBlockTime.TotalSeconds * 1.1 && difficulty > 1)
            {
                difficulty--;
                Console.WriteLine("Decreased difficulty to " + difficulty);
            }
            else
            {
                Console.WriteLine("Difficulty unchanged: " + difficulty);
            }
        }
    }
}
