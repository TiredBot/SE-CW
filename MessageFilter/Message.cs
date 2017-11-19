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
        protected string MessageText {get; set;}
        protected string MessageHeader {get; set;}
        protected string MessageBody { get; set; } //message body is raw input before processing

        
     /* public static Regex Email = new Regex("^E[0-9]{9}$", RegexOptions.IgnoreCase);
        public static Regex Tweet = new Regex("^T[0-9]{9}$", RegexOptions.IgnoreCase);
        public static Regex SMS = new Regex("^S[0-9]{9}$", RegexOptions.IgnoreCase);*/

        public static Regex EmailProcess = new Regex(@"^(("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+\/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))((\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))", RegexOptions.IgnoreCase);


        public static Regex SMSProcess = new Regex(@"^(\+\d{11})[ ](.{140})", RegexOptions.IgnoreCase);
        //SMSProcess could be @^(\+\d{11})[ ](.{140})$
        public static Regex InternationalNumber = new Regex(@"^(+[0-9]{11}");

        public Message(){}


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
           private string PhoneNumber;
          // private string MessageHeader;
          // private string MessageBody;
           public SMS() { }
           public SMS(string messageHeader, string messageBody)
           {
               MessageHeader = messageHeader;
               MessageBody = messageBody;
               //messageBody.Substring(0, Math.Min(140, messageBody.Length));
           }

           public void Process(Dictionary<string,string> TextWordsDict)
           {
               Match SMSMessageTemp = Message.SMSProcess.Match(this.MessageBody);
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
           public bool SIR;
           private string Sender;
           private string Subject;

           public static Regex URLregex = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$",RegexOptions.IgnoreCase);//https://msdn.microsoft.com/en-us/library/ff650303.aspx?f=255&MSPPError=-2147217396#paght000001_commonregularexpressions

           public List<string> quarantineURLs()//replaces URLs found, returns list of URLs found to be written
           {
               List<string> URLsFound = new List<string>();
               Match match = URLregex.Match(this.MessageText);
               while(match.Success)
               {
                   string tempUrl = match.Value.ToString();
                   URLsFound.Add(tempUrl);
               }
               URLregex.Replace(this.MessageText, "<URL QUARINTINED> ");

              return URLsFound;
           }

           public void Process()
           {//Fix Regex to split the message into its parts as Groups
               Match emailaddressTemp = Message.EmailProcess.Match(this.MessageBody);
               this.Sender = emailaddressTemp.Groups[1].Value;//start on 1 not 0, 0 contains whole string
               this.Subject = emailaddressTemp.Groups[2].Value;
               this.MessageText = emailaddressTemp.Groups[3].Value;
               
           }

       }

      public class Tweet: Message
       {
           
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

