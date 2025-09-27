using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockchainAssignment
{
    class Transaction
    {
        public string hash, signature, sender, receiver;    
        public float amount, fee;
        public DateTime timestamp;



        //CONSTRUCTOR includes appropriate assignments of data for a transaction object, including hash and signature generation
        public Transaction(string senderAddress, string recipientAddress, float cost, float feeCost, string senderPrivateKey)
        {
            sender = senderAddress;
            receiver = recipientAddress;
            amount = cost;
            fee = feeCost;
            timestamp = DateTime.Now;               //saves the current time

            hash = CreateHash();            //creates the transaction hash derived from variables above
            signature = Wallet.Wallet.CreateSignature(sender, senderPrivateKey, hash);      //creates a signature signed by sender's private key
        }             
        


        //METHOD generates and returns a hash value from the sender and receiver addresses, transaction amount and fee, and timestamp
        String CreateHash()
        {
            SHA256 hasher = SHA256Managed.Create();
            string input = sender.ToString() + receiver.ToString() + amount.ToString() + fee.ToString() + timestamp.ToString();     //unique variable concatenation - never the same.
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            String hash = string.Empty;
            foreach (byte x in hashByte)
                hash += String.Format("{0:x2}", x);
            return hash;
        }



        //METHOD returns formatted transaction data, formatted to be printable and readable.
        public String PrintTransaction()
        {
            return ("Transaction Hash: " + hash + "\nDigital Signature: " +
            signature + "\n\nTransaction Amount: " + amount +
            "\nTransaction Fee: " + fee + "\n\nSender Address: " +
            sender + "\nReceiver Address: " + receiver +
            "\n\nTimestamp: " + timestamp);
        }
    }
}
