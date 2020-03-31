using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToTextApp.Model
{
    public class Message
    {
        //private string speakerName;
        //private string speakerStyle;
        //private string messageText;

        public Message()
        {
            this.SpeakerName = "N/A";
            this.SpeakerStyle = "#FFCCCCCC";
            this.SpeakerColor = "#FFCCCCCC";
            this.MessageText = "N/A";
        }

        public Message(string speakerName, string messageText) : this()
        {
            this.SpeakerName = speakerName;
            this.MessageText = messageText;
        }

        public string FilePath { get; set; }

        public string SpeakerName { get; set; }
        //{
        //    get { return speakerName; }
        //    set { speakerName = value; }
        //}

        public string SpeakerStyle { get; set; }
        //{
        //    get { return speakerStyle; }
        //    set { speakerStyle = value; }
        //}

        public string SpeakerColor { get; set; }

        public string MessageText { get; set; }
        //{
        //    get { return messageText; }
        //    set { messageText = value; }
        //}
    }
}
