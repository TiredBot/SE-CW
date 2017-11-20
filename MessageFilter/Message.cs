using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;

namespace MessageFilter
{

    /*
     EMAIL REGEX*/
        public class Message
        {
            protected string MessageText { get; set;}
            protected string MessageHeader {get; set;}
            protected string MessageBody { get; set; } //message body is raw input before processing

            
        

            public Message(){}
            public Message(string messageHeader, string messageBody)
            {
                MessageHeader = messageHeader;
                MessageBody = messageBody;
            }

            public string ReplaceTextWords(Dictionary<string,string> TextWords)//Searches Messagebody for matches to Disctionary.Keys
            {
                var outputString = new StringBuilder(this.MessageText);
                foreach (var KeyValuePair in TextWords)
                    outputString.Replace(KeyValuePair.Key, KeyValuePair.Key + "<" + KeyValuePair.Value + "> ");
                return outputString.ToString();
            }
        }

   


       public class SMS: Message
       {
           public static List<Message> SMSsReceivedList;
           private string PhoneNumber;

           public static Regex SMSProcess = new Regex(@"^(\+\d{11})[ ](.{1,140})", RegexOptions.IgnoreCase);
           private static Regex InternationalNumber = new Regex(@"^(+[0-9]{11}");

           public SMS() { }
           public SMS(string messageHeader, string messageBody) : base(messageHeader, messageBody)
           {
               MessageHeader = messageHeader;
               MessageBody = messageBody;
               //messageBody.Substring(0, Math.Min(140, messageBody.Length));
           }

           public void Process(Dictionary<string, string> TextWordsDict)
           {
               Match SMSMessageTemp = SMSProcess.Match(this.MessageBody);
               this.PhoneNumber = SMSMessageTemp.Groups[1].Value.ToString();//start on 1 not 0, 0 contains whole string
               this.MessageText = SMSMessageTemp.Groups[2].Value;
               this.ReplaceTextWords(TextWordsDict);
           }

           public void printSMS()
           {
               Console.WriteLine("MessageID = {0}, PhoneNumber = {1}, \n MessageText = {2} ", this.MessageHeader, this.PhoneNumber, this.MessageText);
           }

       }



       public class Email: Message
       {
           public static List<Email> EmailsReceivedList;
           public static List<string> URLsQuarantined;
           public static List<Tuple<string, string>> SIRsReceivedList;

           private string Sender { get; set; }
           private string Subject { get; set; }
           private bool SIR { get; set; }
           

           
           //public static Regex EmailProcess = new Regex(@"^(("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))", RegexOptions.IgnoreCase);
           private static Regex EmailProcess = new Regex(@"^(("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6})) (.{20}) (.{1,1028})");
           private static Regex ContainsSIR = new Regex(@"\bSIR [0-9]{2}\/[0-9]{2}\/[0-9]{4}\b");
           private static Regex SIRProcess = new Regex(@"^(("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))( SIR [0-9]{2}\/[0-9]{2}\/[0-9]{4})\n(\d{2}\-\d{3}\-\d{2})\n(.*)\n(.{1,1028})");
           private static Regex URLregex = new Regex(@"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?", RegexOptions.IgnoreCase);//https://msdn.microsoft.com/en-us/library/ff650303.aspx?f=255&MSPPError=-2147217396#paght000001_commonregularexpressions
           
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

           private void quarantineURLs()//replaces URLs found, returns list of URLs found to be written
           {
               Match match = URLregex.Match(this.MessageText);
               while(match.Success)
               {
                   string tempUrl = this.MessageHeader + ": "+ match.Value.ToString();
                   URLsQuarantined.Add(tempUrl);
               }
               URLregex.Replace(this.MessageText, "<URL QUARINTINED> ");

           }

           public void ProcessEmail()
           {//Fix Regex to split the message into its parts as Groups
               Match emailTemp = Email.EmailProcess.Match(this.MessageBody);
               this.Sender = emailTemp.Groups[4].Value + emailTemp.Groups[8].Value;//start on 1 not 0, 0 contains whole string
               this.Subject = emailTemp.Groups[14].Value;
               this.MessageText = emailTemp.Groups[15].Value;
               this.quarantineURLs();
               
           }
           public void ProcessSIR()
           {//Fix Regex to split the message into its parts as Groups
               Match SIRTemp = Email.SIRProcess.Match(this.MessageBody);
               this.Sender = SIRTemp.Groups[4].Value + SIRTemp.Groups[8].Value;//start on 1 not 0, 0 contains whole string
               this.Subject = SIRTemp.Groups[14].Value;
               this.MessageText = SIRTemp.Groups[15].Value + SIRTemp.Groups[16].Value + SIRTemp.Groups[17];//Whole message text
               Email.SIRsReceivedList.Add(Tuple.Create(SIRTemp.Groups[15].Value, SIRTemp.Groups[16].Value));//adds centre code and nature of incident list
               this.quarantineURLs();
               
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
       }



      public class Tweet: Message
       {
          public static List<Tweet> TweetsRecievedList;
          public List<string> getHashTags()
          {
              List<string> Hashtags = new List<string>();
              foreach (Match match in Regex.Matches(this.MessageBody, @"(?<!\w)#\w+"))
              {
                  Hashtags.Add(match.ToString());
              }
              return Hashtags;
          }

          public List<string> getMentions()
          {
              List<string> Mentions = new List<string>();
              foreach (Match match in Regex.Matches(this.MessageBody, @"(?<!\w)@\w+"))
              {
                  Mentions.Add(match.ToString());
              }
              return Mentions;
          }

          public void Process()
           {
               
           }
      }
}

