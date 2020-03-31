using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace SpeechToTextApp.Model
{
    //public enum SupportedLanguage
    //{
    //    Spanish = 0,
    //    English = 1,
    //    French = 2,
    //    Mandarin = 3
    //}
    
    public class Speaker //: INotifyPropertyChanged
    {
        //private bool isReadOnly = true;

        public Speaker()
        {
            this.SpeakerId = Guid.Empty;
            this.SpeakerName = "N/A";
            this.Confidence = "N/A";

            var rnd = new Random();
            var rgba = new byte[4];
            rnd.NextBytes(rgba);
            DialogColor = Color.FromArgb(255, rgba[1], rgba[2], rgba[3]);

            EnrollmentSpeechTime = 0;
            EnrollmentStatus = "Enrolling";
            RemainingEnrollmentSpeechTime = 30;
        }

        public Speaker(string speakerName, string confidence) : this()
        {
            this.SpeakerName = speakerName;
            this.Confidence = confidence;
        }

        public Speaker(string speakerName, Guid speakerId) : this()
        {
            this.SpeakerName = speakerName;
            this.SpeakerId = speakerId;
        }

        public Guid SpeakerId { get; set; }
        
        public string SpeakerName { get; set; }

        [JsonIgnore]
        public string Confidence { get; set; }

        //private SupportedLanguage language;
        //public SupportedLanguage Language
        //{
        //    get { return language; }
        //    set 
        //    { 
        //        language = value; 
        //        switch(language)
        //            {
        //            case SupportedLanguage.Spanish:
        //                LanguageStr = "es-ES";
        //                break;
        //            case SupportedLanguage.English:
        //                LanguageStr = "en-US";
        //                break;
        //            case SupportedLanguage.French:
        //                LanguageStr = "fr-FR";
        //                break;
        //            case SupportedLanguage.Mandarin:
        //                LanguageStr = "zn-CN";
        //                break;
        //        }
        //    }
        //}

        [JsonIgnore]
        public string Locale { get; set; }

        public Color DialogColor { get; set; }

        //[JsonIgnore]
        //public bool IsReadOnly
        //{
        //    get
        //    {
        //        return isReadOnly;
        //    }

        //    set
        //    {
        //        isReadOnly = value;
        //        OnPropertyChanged();
        //    }
        //}

        [JsonIgnore]
        public double EnrollmentSpeechTime { get; set; }

        [JsonIgnore]
        public string EnrollmentStatus { get; set; }

        private double remainingEnrollmentSpeechTime;
        [JsonIgnore]
        public double RemainingEnrollmentSpeechTime
        {
            get
            {
                return remainingEnrollmentSpeechTime;
            }
            set
            {
                remainingEnrollmentSpeechTime = value;

                EnrollmentProgress = remainingEnrollmentSpeechTime == 0
                    ? "100%" : ((int)(100 * EnrollmentSpeechTime / 30)).ToString() + "%";
            }
        }

        [JsonIgnore]
        public string EnrollmentProgress { get; set; }
        
        //public event PropertyChangedEventHandler PropertyChanged;

        //private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
