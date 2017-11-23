using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MessageFilter;
using System.Collections.Generic;

namespace MessageFilter_Tests
{
    [TestClass]
    public class SMSTests
    {
  
        
        private string messageHeader = "S123123123";
        private string messageBody = "+12345678901 This is a test text messsage. This is a test text messsage.This is a test text messsage. This is a test text messsage.This is a test text messsage. This is a test text messsage. This is a test text messsage. This is a test text messsage.";
        private Dictionary<string, string> TextWordsDictionary = MyFileIO.PopulateTextWordsFromCsv(@"C:\Users\TheOddSheep\Desktop\3rd Year\Software Engineering\CWRepo\textwords.csv");
        [TestMethod]
        

        public void smsMessageHeaderFormattedCorrectly()
        {
            SMS smsBeingAdded = new SMS(messageHeader, messageBody);
        }
    }
}
