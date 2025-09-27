using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockchainAssignment
{
    public partial class BlockchainApp : Form
    {
        Blockchain blockchain;





        //CONSTRUCTOR instantiates a Blockchain and the User Interface.
        public BlockchainApp()
        {
            InitializeComponent();                      //instantiates the UI
            blockchain = new Blockchain();                      //instantiates a Blockchain
            richTextBox1.Text = "New Blockchain Initialised";               //outputs to confirm Blockchain instantiation
        }







        
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        
               

        //METHOD called when the PRINT BLOCK button is pressed.
        private void Button1_Click(object sender, EventArgs e)    
        {
            if (int.TryParse(textBox1.Text, out _) == true)     //checks whether or not the input string in textBox1 is valid as an integer 
            {
                PrintOutput(Blockchain.PrintBlock(Convert.ToInt32(textBox1.Text)));     //if the value is an integer value, fetches and outputs the block from the given index integer. If out of range function returns "NULL" as output
            } else {                                                                    
                PrintOutput("ERROR: Input contains non-integer values, thus no matches can be located.");       //if the input contains non-integer values an error message is displayed.
            }
            textBox1.Text = "";
        }





        //METHOD called when GENERATE WALLET button is pressed. Instantiates a Wallet object, generating a mathematically related public 
        //and private key to identify and restrict the wallet access.
        private void Button2_Click(object sender, EventArgs e)
        {
            String privKey;                                                 
            Wallet.Wallet newWallet = new Wallet.Wallet(out privKey);       //Generates a new wallet, returning the private key to variable privKey.
            String publicKey = newWallet.publicID;                  //variable publicKey is assigned to the value of the Wallet's publicID (public key).

            PrintPublicKey(publicKey);      //prints the public and private keys into the PUBLIC KEY and PRIVATE KEY text boxes respectively. 
            PrintPrivKey(privKey);      
        }





        //METHOD used to print an output to the large OUTPUT box, richTextBox1.
        private void PrintOutput(String text)
        {
            richTextBox1.Text = text;       //alters the displayed text value of the output box to the passed argument value.
        }

        



        //METHOD used to print an output to the PUBLIC KEY text box, TextBox2.
        private void PrintPublicKey(String text)
        {
            textBox2.Text = text;           //alters the displayed text value of the public key box to the passed argument value.
        }





        //METHOD used to print an output to the PRIVATE KEY text box, TextBox3.
        private void PrintPrivKey(String text)
        {
            textBox3.Text = text;           //alters the displayed text value of the private key box to the passed argument value.
        }


        


        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }
        


        //METHOD used to validate that the public and private key inputs are mathemawhen the VALIDATE KEYS button is pressed.
        private void Button3_Click(object sender, EventArgs e)
        {
            String publicID = textBox2.Text;        //public key variable publicID is the input of the public key text box
            String privKey = textBox3.Text;          //private key variable privKey is the input of the private key text box

            if (Wallet.Wallet.ValidatePrivateKey(privKey, publicID) == true)
            {
                PrintOutput("Key Pair is Valid");       //if the public and private key are mathematically related, outputs
            } else {                                    //a valid response.
                PrintOutput("Key Pair is Invalid");     //Otherwise outputs an invalid response.
            }
        }






        private void Label4_Click(object sender, EventArgs e)
        {

        }



        //METHOD creates a transaction object and adds it to the pending transactions listwith the public and private key address inputs,
        //the receiver public ID key input, and the amount and fee input box inputs when the SEND TRANSACTION button is pressed.
        private void Button4_Click(object sender, EventArgs e)
        {
            if ((float.TryParse(textBox4.Text, out _) == true) && (float.TryParse(textBox5.Text, out _) == true))
            {
                float amount = float.Parse(textBox4.Text);
                float fee = float.Parse(textBox5.Text);
                string publicID = textBox2.Text;

                Transaction Transact = new Transaction(textBox2.Text, textBox6.Text, amount, fee, textBox3.Text);

                if (Blockchain.PendTransaction(Transact))
                {
                    PrintOutput(Transact.PrintTransaction());
                }
                else
                {
                    PrintOutput("TRANSACTION DECLINED - Invalid input or insufficient available balance.");
                    MessageBox.Show("Transaction was rejected.\n\nEnsure amount/fee are positive and that your effective balance covers all pending transactions.");
                }
            }
        }





        //METHOD creates and mines a new block into the blockchain containing up to 5 (or value of 'n') pending transactions when the GENERATE BLOCK button is pressed.
            private void Button5_Click(object sender, EventArgs e)
            {
            List<Transaction> selectedTransactions = new List<Transaction>();       //creates an empty list for pending transactions to be added
            selectedTransactions.Clear();                                           //to the block.

            selectedTransactions = Blockchain.SelectTransactions(5, textBox7.Text);                                                      //sets the maximum number of transactions per block to 5 in the arguments.

            
            Block newBlock = new Block(Blockchain.Blocks.Last(),selectedTransactions, textBox2.Text);  //instantiates and mines a new block, given the previous block and the selected transactions.
            PrintOutput(newBlock.ReturnBlock());        //outputs the new block data.
            Blockchain.Blocks.Add(newBlock);        //adds the block to the blockchain.
            Blockchain.UpdateDifficulty();
            }





        //METHOD 
        private void Button6_Click(object sender, EventArgs e)
        {
            string allBlocks = "";      //creates an empty string for output data

            for(int i = 0; i < Blockchain.Blocks.Count; i++)
            {
                allBlocks += ("<BLOCK " + i.ToString() + ">\n" + Blockchain.Blocks[i].ReturnBlock() + "\n\n\n\n");       //adds and formats all block data sequentially into one 
            }                                                                       //large output string of all block data.
            PrintOutput(allBlocks);     //outputs all block data.
        }





        //METHOD validates that each block in the blockchain proceeds the next block by checking the previous hash values 
        private void Button7_Click(object sender, EventArgs e)
        {
            bool blockchainValid = true;

            for (int i = Blockchain.Blocks.Count() - 1; i > 0; i--)
            {
                if (Blockchain.Blocks[i-1].hash != Blockchain.Blocks[i].lastHash)
                {
                    blockchainValid = false;
                    break;
                }
                else if (Blockchain.ValidateHash(Blockchain.Blocks[i]) == false)
                {
                    blockchainValid = false;
                    break;
                }
                else if (Blockchain.ValidateMerkleRoot(Blockchain.Blocks[i]) == false)
                {
                    blockchainValid = false;
                    break;
                }
            }
            



            if(blockchainValid == true)
            {
                PrintOutput("BLOCKCHAIN IS VALID");
            } else {
                PrintOutput("BLOCKCHAIN IS INVALID");
            }
        }





        //METHOD returns the wallet transaction info and account balance.
        private void Button8_Click(object sender, EventArgs e)
        {
            string publicID = textBox2.Text;
            PrintOutput(Blockchain.getBalance(publicID,true));
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            String printPendingTransactions = "";
            for(int i = 0; i < Blockchain.pendingTransactions.Count(); i++)
            {
                printPendingTransactions += "TRANSACTION "+ ((i+1).ToString()) + " -\n" + Blockchain.pendingTransactions[i].PrintTransaction() +"\n\n\n";
            }
            PrintOutput(printPendingTransactions);
        }

        private void Label7_Click(object sender, EventArgs e)
        {

        }

        private void AltruisticSwitch_Click(object sender, EventArgs e)
        {
            AltruisticSwitch.BackColor = System.Drawing.Color.Silver;
            RandomSwitch.BackColor = System.Drawing.Color.LightGray;
            GreedySwitch.BackColor = System.Drawing.Color.LightGray;
            AddressedSwitch.BackColor = System.Drawing.Color.LightGray;

            Blockchain.transactionSelectionType = 0;
        }

        private void RandomSwitch_Click(object sender, EventArgs e)
        {
            AltruisticSwitch.BackColor = System.Drawing.Color.LightGray;
            RandomSwitch.BackColor = System.Drawing.Color.Silver;
            GreedySwitch.BackColor = System.Drawing.Color.LightGray;
            AddressedSwitch.BackColor = System.Drawing.Color.LightGray;

            Blockchain.transactionSelectionType = 1;
        }

        private void GreedySwitch_Click(object sender, EventArgs e)
        {
            AltruisticSwitch.BackColor = System.Drawing.Color.LightGray;
            RandomSwitch.BackColor = System.Drawing.Color.LightGray;
            GreedySwitch.BackColor = System.Drawing.Color.Silver;
            AddressedSwitch.BackColor = System.Drawing.Color.LightGray;

            Blockchain.transactionSelectionType = 2;
        }

        private void AddressedSwitch_Click(object sender, EventArgs e)
        {
            AltruisticSwitch.BackColor = System.Drawing.Color.LightGray;
            RandomSwitch.BackColor = System.Drawing.Color.LightGray;
            GreedySwitch.BackColor = System.Drawing.Color.LightGray;
            AddressedSwitch.BackColor = System.Drawing.Color.Silver;

            Blockchain.transactionSelectionType = 3;
        }

        private void Button10_Click_1(object sender, EventArgs e)
        {

            PrintOutput("average block time: " + (Blockchain.GetAverageBlockTime()).ToString() +
                "\n\naverage mine time: " + (Blockchain.GetAverageMineTime()).ToString() +
                "\naverage mine rate (hashes/s): " + (Blockchain.GetAverageMineRate()).ToString());
        }
    }
}
