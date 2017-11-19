using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//using System.Windows.MessageBox;
using System.Text.RegularExpressions;
using System.IO;
using MessageFilter;

namespace MessageFilter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        string TextWordsPath = @"C:\Users\TheOddSheep\Desktop\3rd Year\Software Engineering\CWRepo\SE-CW\MessageFilter\FilterWords.csv";
        public static Dictionary<string, string> TextWordsDictionary;
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {


        }

        private void BTN_SetPath_Click(object sender, RoutedEventArgs e)
        {
            bool Exists = File.Exists(TB_PathToTextWords.Text);
            if (Exists)
            {
                TextWordsPath = TB_PathToTextWords.Text;
                TextWordsDictionary = MyFileIO.PopulateTextWordsFromCsv(TextWordsPath);
            }
            else
            {
                MessageBox.Show("Invalid Path.");
            }
            //foreach (KeyValuePair<string, string> kv in FilterWordsDictionary)
             //  Console.WriteLine("{0}, {1}", kv.Key.ToString(), kv.Value.ToString());
        }

        private void BTN_Submit_Message_Click(object sender, RoutedEventArgs e)
        {
           // Message temp = new Message();
            string HeaderInput = TB_MessageHeader.Text;
            string MessageBody = TB_MessageBody.Text;
            if (Regex.IsMatch(HeaderInput, @"^[S][0-9]{9}$", RegexOptions.IgnoreCase))
            {

                SMS temp = new SMS(HeaderInput, MessageBody);
                temp.Process(TextWordsDictionary);
                temp.printSMS();
            }
            else if (Regex.IsMatch(HeaderInput, @"^E[0-9]{9}$", RegexOptions.IgnoreCase))
            {
                Email temp = new Email();
                //Seperate message Text frombody, means getting email from start first
                if()//If Subject is SIR Code then make SIR
                {
                    temp.SIR = true;
                }
                else//Normal email message
                {
                    temp.SIR = false;
                }
            }
            else if (Regex.IsMatch(HeaderInput, @"^T[0-9]{9}$", RegexOptions.IgnoreCase))
            {
                Tweet temp = new Tweet();
                temp.Process();
                
            }
            else
            {
                MessageBox.Show("Please ensure the Message ID has been entered in the proper format and that the message body is not empty.\n");
            }
        } 
    }
}
