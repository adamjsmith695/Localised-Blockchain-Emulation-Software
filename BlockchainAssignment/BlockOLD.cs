using BlockchainAssignment.HashCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockchainAssignment
{
    class BlockOLD
    {
        public DateTime timestamp;              //saves the current time.
        public int index, nonce;                //index of Block position in the Blockchain, nonce is a variable utilised in mining.
        public static int difficulty = 4;       //how many zeroes are required for a block to be mined.
        public string hash, lastHash, merkleRoot;               //hash variable of the current block and for the last block.
        public List<Transaction> TransactionList = new List<Transaction>();     //List of transactions in the block.
        public static float standardReward = 1;
        public TimeSpan blockTime, mineTime;
        public bool isMineComplete = false;





        //PRIMARY CONSTRUCTOR this constructor is the primary constructor, but is only used to generate the initial genesis block.
        //Instantiates the block timestamp, sets the index to 0, specifies an empty value for the previous hash, and generates
        //and mines a hash for the genesis block.
        public BlockOLD()
        {
            timestamp = DateTime.Now;
            index = 0;                      //sets index to 0.
            lastHash = "";          //the previous hash doesn't exist so is set to an empty string.
            hash = CreateHash(nonce);            //Creates the hash for the genesis block. Note the genesis block does not need to be and isn't mined.
        }





        //SECONDARY CONSTRUCTOR instantiates a new block with Block data, including the timestamp, blockchain index, the previous hash, nonce, list of
        //transactions, and the hash that is mined upon block generation.
        public BlockOLD(BlockOLD lastBlock, List<Transaction> Transactions, String MinerPublicID)
            : this()
        {
            timestamp = DateTime.Now;
            index = lastBlock.index + 1;        //increments the block index from the previous block, which must be passed
            lastHash = lastBlock.hash;          //into this object. The previous hash is also taken and used to create the 
            nonce = 0;                          //current hash.

            TransactionList = Transactions;     //the list of transaction objects that are generated with the block are passed into this constructor, here storing it into the block data.

            TransactionList.Add(GenerateReward(MinerPublicID));    //adding the reward value and fee sum of transactions to the transactions

            List<String> TransactionHashes = new List<String>();    //creates a list of transaction hashes for merkle root v
            for(int i = 0; i < TransactionList.Count(); i++)        //
            {                                                       //
                TransactionHashes.Add(TransactionList[i].hash);     //
            }

            merkleRoot = MerkleRoot(TransactionHashes);         //produces the merkle root of the transaction hashes
            
            Thread th = new Thread(startMineCompletionCheck);

            Thread th1 = new Thread(startMineThread1);
            Thread th2 = new Thread(startMineThread2);
            //Thread th3 = new Thread(startMineThread3);
            //Thread th4 = new Thread(startMineThread4);

            th.Start();

            th1.Start();
            th2.Start();
            //th3.Start();
            //th4.Start();

            th.Join();

            th1.Abort();
            th2.Abort();
            //th3.Abort();
            //th4.Abort();
            

            //hash = Mine();          //mines the hash using the above data against the global difficulty level

            mineTime = DateTime.Now - timestamp;
            blockTime = SetBlockTime();
        }


       





        //METHOD creates a hash value based on index value, the timestamp, the previous hash value and the nonce -
        //the nonce is an interchangable integer and enables mining by allowing one block of data to take many hash forms. with only different nonce values
        public String CreateHash(int currNonce)
        {
            SHA256 hasher = SHA256Managed.Create();
            String input = index.ToString() + timestamp.ToString() + lastHash + merkleRoot + currNonce.ToString();
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            String hash = string.Empty;
            foreach (byte x in hashByte)
                hash += String.Format("{0:x2}", x);
            return hash;
        }




        
        //METHOD returns a string of formatted printable and readable data about the block 
        public String ReturnBlock()
        {
            String printTransactions = "";

            for(int i = 0; i < TransactionList.Count(); i++)    //              ---->                                   for each Transaction that the block contains:
            {
                printTransactions += ("\n\n\nTRANSACTION " + (i+1) + " -\n" + TransactionList[i].PrintTransaction());   //concatenates each transaction in the block and
            }                                                                                                           //it's formatted print data

            return (string.Concat(
                "------------------------------------------------------------------------------------------------------------------------------------------------------------" +
                "\n\nindex: " + index.ToString() +
                "\nprevious hash: " + lastHash +
                "\ncurrent hash: " + hash +
                "\nmerkle root: " + merkleRoot +
                "\ndifficulty: " + difficulty.ToString() +              //Block data formatting and string concatenation into
                "\nnonce: " + nonce.ToString() +                        //one big formatted string that is returned by the function.
                "\ntimestamp: " + timestamp.ToString() +
                "\nmine time: " + mineTime.ToString() +
                "\nblock time: " + blockTime.ToString() +
                printTransactions
                ));
        }



        
        /*
        //METHOD finds a rehash (using the nonce incrementally to alter the hash with the same core data) of the block to fit the 
        //difficulty level for mining. If difficulty = 5, a hash that begins with 00000 (5 zeroes) is required, so for each nonce incrementation
        //the hash will change (but still linked to the unique core block data that remains the same) until a hash starting with 00000 is found.
        //The method then returns the the valid rehash string. 
        public String Mine()
        {
            bool mined = false;
            String rehash = hash;   //setting the rehash variable to the original hash - may already (highly unlikely) be valid.

            while(mined == false)       //loops until mined = true, which can only occur when the difficulty level has been reached.
            {
                //Console.WriteLine(nonce);
                for(int i = 0; i < difficulty; i++)     //loops through the first [difficulty value] letters of the current hash to check
                {                                       //if they're all zero.
                    if(rehash[i] == '0')
                    {
                        mined = true;       //mined becomes true if a zero is detected. However, if any other character is detected at any point,
                    } else {                //
                        mined = false;      //mined becomes false and the loop is broken, as there is no need to continue searching for 0's.
                        break;
                    }
                }
                if(mined == true)       //if the hash has been found (hash exited the previous loop with mined = true), the hash value is returned.
                {                       //Note that the value of the nonce is unchanged once the hash is found, this ensures that the nonce value 
                    return rehash;      //can be saved in the block data.
                } else {
                    nonce++;                    //if the hash was not found this iteration, nonce is incremented by 1, and the new hash is generated
                    rehash = CreateHash(nonce);      //for the next iteration.
                }
            }
            return null;
        }     
        */





        //METHOD finds a rehash (using the nonce incrementally to alter the hash with the same core data) of the block to fit the 
        //difficulty level for mining. If difficulty = 5, a hash that begins with 00000 (5 zeroes) is required, so for each nonce incrementation
        //the hash will change (but still linked to the unique core block data that remains the same) until a hash starting with 00000 is found.
        //The method then returns the the valid rehash string. 
        public String Mine1()
        {
            int nonce1 = 0;
            bool mined = false;
            String rehash = hash;   //setting the rehash variable to the original hash - may already (highly unlikely) be valid.

            while (mined == false)       //loops until mined = true, which can only occur when the difficulty level has been reached.
            {
                //Console.WriteLine(nonce1);
                for (int i = 0; i < difficulty; i++)     //loops through the first [difficulty value] letters of the current hash to check
                {                                       //if they're all zero.
                    if (rehash[i] == '0')
                    {
                        mined = true;       //mined becomes true if a zero is detected. However, if any other character is detected at any point,
                    }
                    else
                    {                       //
                        mined = false;      //mined becomes false and the loop is broken, as there is no need to continue searching for 0's.
                        break;
                    }
                }
                if (mined == true)       //if the hash has been found (hash exited the previous loop with mined = true), the hash value is returned.
                {                       //Note that the value of the nonce is unchanged once the hash is found, this ensures that the nonce value 
                    nonce = nonce1;
                    return rehash;      //can be saved in the block data.
                }
                else
                {
                    nonce1 += 3;                 //if the hash was not found this iteration, nonce is incremented by 1, and the new hash is generated
                    rehash = CreateHash(nonce1);      //for the next iteration.
                }
            }
            return null;
        }





        //METHOD finds a rehash (using the nonce incrementally to alter the hash with the same core data) of the block to fit the 
        //difficulty level for mining. If difficulty = 5, a hash that begins with 00000 (5 zeroes) is required, so for each nonce incrementation
        //the hash will change (but still linked to the unique core block data that remains the same) until a hash starting with 00000 is found.
        //The method then returns the the valid rehash string. 
        public String Mine2()
        {
            int nonce2 = 1;
            bool mined = false;
            String rehash = hash;   //setting the rehash variable to the original hash - may already (highly unlikely) be valid.

            while (mined == false)       //loops until mined = true, which can only occur when the difficulty level has been reached.
            {
                //Console.WriteLine(nonce2);
                for (int i = 0; i < difficulty; i++)     //loops through the first [difficulty value] letters of the current hash to check
                {                                       //if they're all zero.
                    if (rehash[i] == '0')
                    {
                        mined = true;       //mined becomes true if a zero is detected. However, if any other character is detected at any point,
                    }
                    else
                    {                       //
                        mined = false;      //mined becomes false and the loop is broken, as there is no need to continue searching for 0's.
                        break;
                    }
                }
                if (mined == true)       //if the hash has been found (hash exited the previous loop with mined = true), the hash value is returned.
                {                       //Note that the value of the nonce is unchanged once the hash is found, this ensures that the nonce value 
                    nonce = nonce2;
                    return rehash;      //can be saved in the block data.
                }
                else
                {
                    nonce2 += 3;                 //if the hash was not found this iteration, nonce is incremented by 1, and the new hash is generated
                    rehash = CreateHash(nonce2);      //for the next iteration.
                }
            }
            return null;
        }




        /*
        //METHOD finds a rehash (using the nonce incrementally to alter the hash with the same core data) of the block to fit the 
        //difficulty level for mining. If difficulty = 5, a hash that begins with 00000 (5 zeroes) is required, so for each nonce incrementation
        //the hash will change (but still linked to the unique core block data that remains the same) until a hash starting with 00000 is found.
        //The method then returns the the valid rehash string. 
        public String Mine3()
        {
            int nonce3 = 2;
            bool mined = false;
            String rehash = hash;   //setting the rehash variable to the original hash - may already (highly unlikely) be valid.

            while (mined == false)       //loops until mined = true, which can only occur when the difficulty level has been reached.
            {
                //Console.WriteLine(nonce3);
                for (int i = 0; i < difficulty; i++)     //loops through the first [difficulty value] letters of the current hash to check
                {                                       //if they're all zero.
                    if (rehash[i] == '0')
                    {
                        mined = true;       //mined becomes true if a zero is detected. However, if any other character is detected at any point,
                    }
                    else
                    {                       //
                        mined = false;      //mined becomes false and the loop is broken, as there is no need to continue searching for 0's.
                        break;
                    }
                }
                if (mined == true)       //if the hash has been found (hash exited the previous loop with mined = true), the hash value is returned.
                {                       //Note that the value of the nonce is unchanged once the hash is found, this ensures that the nonce value 
                    nonce = nonce3;
                    return rehash;      //can be saved in the block data.
                }
                else
                {
                    nonce3 += 3;                 //if the hash was not found this iteration, nonce is incremented by 1, and the new hash is generated
                    rehash = CreateHash(nonce3);      //for the next iteration.
                }
            }
            return null;
        }




        
        //METHOD finds a rehash (using the nonce incrementally to alter the hash with the same core data) of the block to fit the 
        //difficulty level for mining. If difficulty = 5, a hash that begins with 00000 (5 zeroes) is required, so for each nonce incrementation
        //the hash will change (but still linked to the unique core block data that remains the same) until a hash starting with 00000 is found.
        //The method then returns the the valid rehash string. 
        public String Mine4()
        {
            int nonce4 = 3;
            bool mined = false;
            String rehash = hash;   //setting the rehash variable to the original hash - may already (highly unlikely) be valid.

            while (mined == false)       //loops until mined = true, which can only occur when the difficulty level has been reached.
            {
                //Console.WriteLine(nonce4);
                for (int i = 0; i < difficulty; i++)     //loops through the first [difficulty value] letters of the current hash to check
                {                                       //if they're all zero.
                    if (rehash[i] == '0')
                    {
                        mined = true;       //mined becomes true if a zero is detected. However, if any other character is detected at any point,
                    }
                    else
                    {                       //
                        mined = false;      //mined becomes false and the loop is broken, as there is no need to continue searching for 0's.
                        break;
                    }
                }
                if (mined == true)       //if the hash has been found (hash exited the previous loop with mined = true), the hash value is returned.
                {                       //Note that the value of the nonce is unchanged once the hash is found, this ensures that the nonce value 
                    nonce = nonce4;
                    return rehash;      //can be saved in the block data.
                }
                else
                {
                    nonce4 += 4;                 //if the hash was not found this iteration, nonce is incremented by 1, and the new hash is generated
                    rehash = CreateHash(nonce4);      //for the next iteration.
                }
            }
            return null;
        }
        */




        //METHOD generates the merkle root of all transactions
        public String MerkleRoot(List<String> TransactionHashes)
        {
            List<String> merkleLeaves = new List<String>();

            if(TransactionHashes.Count() % 2 != 0)    //if the number of transaction hashes is odd:
            {
                for(int i = 0; i < TransactionHashes.Count(); i++)
                {
                    //Console.WriteLine("i = " + i);
                    if(i < TransactionHashes.Count()-1)       //combines hashes with the next until the last hash remains - in which the else branch is reached
                    {
                        merkleLeaves.Add(HashTools.CombineHash(TransactionHashes[i], TransactionHashes[i+1]));
                        i++;        //skips an iteration as the current iteration and next iteration value have been combined, so next iteration is now already used.
                    } else {
                        merkleLeaves.Add(TransactionHashes[i]);     //adds the last transaction untouched to the list of combined hashes - because this if statement branch is 
                    }                                               //reached when the number of hashes is odd, there will be this spare hash that has no partner to combine with.
                }
            }
            else if(TransactionHashes.Count() != 0)     //if the number of transactions hashes is not odd (so, even):
            {
                for(int i = 0; i < TransactionHashes.Count(); i++)
                {
                    //Console.WriteLine("i = " + i);
                    merkleLeaves.Add(HashTools.CombineHash(TransactionHashes[i], TransactionHashes[i+1]));  //combines hashes with the next hash.
                    i++;        //skips an iteration as the current iteration and next iteration value have been combined, so next iteration is now already used.
                }
            } else {
                return null;  //logically should never be reached, although parse will fail if there is no return branch here.
            }


            if(merkleLeaves.Count() != 1)
            {
                return MerkleRoot(merkleLeaves);    //if there is not a single hash value (complete merkle root), recurses this method with the new merkle leaves -
            } else {                                //
                return merkleLeaves[0];             //until there is only one value, in which the merkle root has been found and is returned here.
            }
        }





        //METHOD creates a new transaction object with the sum of the block's transaction fees and standard reward that
        //is addressed to the miner.
        public Transaction GenerateReward(String MinerPublicID)
        {
            float mineReward = 0;   

            for(int i = 0; i < TransactionList.Count(); i++)
            {
                mineReward += TransactionList[i].fee;       //summating the fee totals
            }
             
            mineReward += standardReward;   //adding the standard reward value to the reward total
            return new Transaction("Mining_Reward", MinerPublicID, mineReward, 0, "");      //returns a transaction that pays the miner the full reward.
        }





        //METHOD
        public TimeSpan SetBlockTime()
        {
            DateTime blockMinedTime = timestamp + mineTime;
            DateTime prevBlockMinedTime = Blockchain.Blocks[index - 1].timestamp + Blockchain.Blocks[index-1].mineTime;
            return blockMinedTime - prevBlockMinedTime;

            //return timestamp - Blockchain.Blocks[index - 1].timestamp;
        }





        //METHOD
        void startMineCompletionCheck()
        {
            Console.WriteLine("MINE STARTED");
            while (isMineComplete == false) { }
            Console.WriteLine("MINE COMPLETE");
        }




        /*
        //METHOD
        void startMineThread()
        {
            isMineComplete = false;
            hash = Mine();
            isMineComplete = true;
        }
        */




        //METHOD
        void startMineThread1()
        {
            isMineComplete = false;
            hash = Mine1();
            isMineComplete = true;
        }



        

        //METHOD
        void startMineThread2()
        {
            isMineComplete = false;
            hash = Mine2();
            isMineComplete = true;
        }




        /*
        //METHOD
        void startMineThread3()
        {
            isMineComplete = false;
            hash = Mine3();
            isMineComplete = true;
        }




        
        //METHOD
        void startMineThread4()
        {
            isMineComplete = false;
            hash = Mine4();
            isMineComplete = true;
        }*/
    }
}
