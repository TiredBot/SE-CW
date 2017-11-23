using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MessageFilter
{
    /*EMAIL REGEX*/
        public class Message
        {
            [JsonProperty]
            protected string MessageText { get; set; }
            [JsonProperty]
            protected string MessageHeader { get; set; }
            [JsonProperty]
            protected string MessageBody { get; set; } //message body is raw input before processing

            protected static Regex URLregex = new Regex(@"(www\.)(.*)(.com)", RegexOptions.IgnoreCase);//https://msdn.microsoft.com/en-us/library/ff650303.aspx?f=255&MSPPError=-2147217396#paght000001_commonregularexpressions
            public static string jsonPath = @"C:\Users\TheOddSheep\Desktop\JsonObjects\";

            public Message(){}
            public Message(string messageHeader, string messageBody)
            {
                MessageHeader = messageHeader;
                MessageBody = messageBody;
            }

            public string getMessageHeader()
            {
                return MessageHeader;
            }
            public string getMessageBody()
            {
                return MessageBody;
            }
            public string getMessageText()
            {
                return MessageText;
            }
            protected string ReplaceTextWords(Dictionary<string,string> TextWords)//Searches Messagebody for matches to Disctionary.Keys
            {
                var outputString = new StringBuilder(this.MessageText);
                foreach (var KeyValuePair in TextWords)
                    outputString.Replace(KeyValuePair.Key, KeyValuePair.Key + "<" + KeyValuePair.Value + "> ");
                return outputString.ToString();
            }
        }

   


       public class SMS: Message
       {
           public static List<SMS> SMSsReceivedList = new List<SMS>();
           [JsonProperty]
           private string PhoneNumber;

           private static Regex SMSProcess = new Regex(@"^(\+\d{11})[ ](.{1,140})", RegexOptions.IgnoreCase);
           //private static Regex InternationalNumber = new Regex(@"^(+[0-9]{11})");

           public SMS() { }
           public SMS(string messageHeader, string messageBody) : base(messageHeader, messageBody)
           {
               MessageHeader = messageHeader;
               MessageBody = messageBody;
           }

           public void ProcessSMS(Dictionary<string, string> TextWordsDict)
           {
               Match SMSMessageTemp = SMSProcess.Match(this.MessageBody);
               this.PhoneNumber = SMSMessageTemp.Groups[1].Value.ToString();//start on 1 not 0, 0 contains whole string
               this.MessageText = SMSMessageTemp.Groups[2].Value;
               this.MessageText = this.ReplaceTextWords(TextWordsDict);//returns string with expanded txt talk
               SMSsReceivedList.Add(this);
               string SerializedObj = JsonConvert.SerializeObject(this) + "\n";
               File.AppendAllText(Message.jsonPath + "SMS.json", SerializedObj);
           }

           public void printSMS()
           {
               Console.WriteLine("MessageID = {0}, PhoneNumber = {1}, \n MessageText = {2} ", this.MessageHeader, this.PhoneNumber, this.MessageText);
           }

       }



       public class Email: Message
       {
           public static List<Email> EmailsReceivedList= new List<Email>();
           public static List<string> URLsQuarantined = new List<String>();
           public static List<Tuple<string, string>> SIRsReceivedList = new List<Tuple<string, string>>();
           [JsonProperty]
           private string Sender { get; set; }
           [JsonProperty]
           private string Subject { get; set; }
           [JsonProperty]
           private bool SIR { get; set; }
           

           
           //public static Regex EmailProcess = new Regex(@"^(("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))", RegexOptions.IgnoreCase);
           private static Regex EmailProcess = new Regex(@"^(("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6})) (.{20}) (.{1,1028})");
           private static Regex ContainsSIR = new Regex(@"\bSIR [0-9]{2}\/[0-9]{2}\/[0-9]{4}?\r\n");
           private static Regex SIRProcess = new Regex(@"^(("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))( SIR [0-9]{2}\/[0-9]{2}\/[0-9]{4})\r\n(\d{2}\-\d{3}\-\d{2})?\r\n(.*)\r\n(.{1,1028})");
           
           
           //SIR REGEX for subject incedient report and body(\d{2}\-\d{3}\-\d{2})\n(.*)\n(.{1,1028})
           public bool SIRcheck()
           {
               if(Email.ContainsSIR.IsMatch(this.MessageBody) == true)
               {
                   this.SIR=true;
                   return true;
               }
               else
               {
                   this.SIR = false;
                   return false;
               } 
           }

           private string quarantineURLs()//replaces URLs found, returns list of URLs found to be written
           {
               foreach(Match match in URLregex.Matches(this.MessageText))
               {
                   string tempUrl = match.Value.ToString();
                   URLsQuarantined.Add(tempUrl);
               }
               this.MessageText = URLregex.Replace(this.MessageText, "<URL QUARINTINED> ");

               return this.MessageText;
           }

           public void ProcessEmail()
           {//Fix Regex to split the message into its parts as Groups
               Match emailTemp = Email.EmailProcess.Match(this.MessageBody);
               this.Sender = emailTemp.Groups[4].Value + emailTemp.Groups[8].Value;//start on 1 not 0, 0 contains whole string
               this.Subject = emailTemp.Groups[14].Value;
               this.MessageText = emailTemp.Groups[15].Value;
               this.MessageText = this.quarantineURLs();
               Email.EmailsReceivedList.Add(this);
               string SerializedObj = JsonConvert.SerializeObject(this) + "\n";
               File.AppendAllText(Message.jsonPath + "Email.json", SerializedObj);
           }

           public void ProcessSIR()
           {//Fix Regex to split the message into its parts as Groups
               Match SIRTemp = Email.SIRProcess.Match(this.MessageBody);
               this.Sender = SIRTemp.Groups[4].Value + SIRTemp.Groups[8].Value;//start on 1 not 0, 0 contains whole string
               this.Subject = SIRTemp.Groups[14].Value;
               this.MessageText = SIRTemp.Groups[15].Value + SIRTemp.Groups[16].Value + SIRTemp.Groups[17];//Whole message text
               Email.SIRsReceivedList.Add(Tuple.Create(SIRTemp.Groups[15].Value, SIRTemp.Groups[16].Value));//adds centre code and nature of incident list
               this.MessageText = this.quarantineURLs();
               Email.EmailsReceivedList.Add(this);
               string SerializedObj = JsonConvert.SerializeObject(this) + "\n";
               File.AppendAllText(Message.jsonPath + "Email.json", SerializedObj);
               
           }

           public Email() { }
           public Email(string messageHeader, string messageBody) : base(messageHeader, messageBody)
           {
               MessageHeader = messageHeader;
               MessageBody = MessageBody;
           }

           public void printEmail()
           {
               Console.WriteLine("MessageID = {0}, Sender = {1},  Subject = {2}\n MessageText = {3} ", this.MessageHeader, this.Sender,this.Subject, this.MessageText);
           }
           public static List<string> SIRListDisplay()
          {
              List<string> SIRstrings = new List<string>();
              for(int i = 0; i < SIRsReceivedList.Count();++i)
              {
                  string temp = SIRsReceivedList[i].Item1 + " " + SIRsReceivedList[i].Item2;
                  SIRstrings.Add(temp);
              }
              return SIRstrings;
          }
       }



      public class Tweet: Message
       {
          private string Sender;

          public static List<Tweet> TweetsRecievedList = new List<Tweet>();
          public static HashSet<string> MentionsList = new HashSet<string>();
          public static List<string> HashtagsList = new List<string>();
         

          private static Regex Handles = new Regex(@"(\@[0-9a-zA-Z]{1,20})(\s|\n|$)",RegexOptions.IgnoreCase);
          private static Regex Hashtags = new Regex(@"(\#[0-9a-zA-Z]{1,20})(\s|\n|$)", RegexOptions.IgnoreCase);
          private static Regex TweetProcess = new Regex(@"(^\@[0-9a-zA-Z]{1,15})(\s|\n| )(.{1,140})",RegexOptions.IgnoreCase);

          public Tweet() { }
          public Tweet(string messageHeader, string messageBody) : base(messageHeader, messageBody)
           {
               MessageHeader = messageHeader;
               MessageBody = messageBody;
           }
          private void getHashTags()
          {
              foreach (Match match in Tweet.Hashtags.Matches(this.MessageText))
              {
                  HashtagsList.Add(match.ToString());
              }
          }

          private void getMentions()
          {
              foreach (Match match in Tweet.Handles.Matches(this.MessageText))
              {
                  TwitterTracker.MentionsList.Add(match.ToString());
              }
          }

          public void ProcessTweet(Dictionary<string,string> TextWords)//BUG IN HERE MEMEORY ISSUE, try add a tweet to reproduce
           {
               Match tempTweet = Tweet.TweetProcess.Match(this.MessageBody);
               this.Sender = tempTweet.Groups[1].Value;
               this.MessageText = tempTweet.Groups[3].Value;
               this.getHashTags();//gets hashtags and adds them to static list
               this.getMentions();
               this.ReplaceTextWords(TextWords);
               Tweet.TweetsRecievedList.Add(this);
               string SerializedObj = JsonConvert.SerializeObject(this) + "\n";
               File.AppendAllText(Message.jsonPath + "Tweet.json", SerializedObj);
           }

          public static List<string> TrendingListDisplay() //Returns a string to show to the user in UI Listbox
          {
              List<string> TrendingStrings = new List<string>();
              foreach (KeyValuePair<string, int> kvp in TwitterTracker.TrendingList)
              {
                  string temp = kvp.Key.ToString() + " " + kvp.Value.ToString();
                  TrendingStrings.Add(temp);
              }
              return TrendingStrings;
          }
 
      }
}

