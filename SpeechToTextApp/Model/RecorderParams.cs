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
    public class RecorderParams : INotifyPropertyChanged
    {
        private int silenceTimeout;
        private float silenceThreshold;
        private int timeoutLimit;

        public RecorderParams(int silenceTimeout, float silenceThreshold, int timeoutLimit)
        {
            this.SilenceTimeout = silenceTimeout;
            this.SilenceThreshold = silenceThreshold;
            this.TimeoutLimit = timeoutLimit;
        }

        public int SilenceTimeout
        {
            get { return silenceTimeout; }
            set { silenceTimeout = value; }
        }

        public float SilenceThreshold
        {
            get { return silenceThreshold; }
            set { silenceThreshold = value; }
        }

        public int TimeoutLimit
        {
            get { return timeoutLimit; }
            set { timeoutLimit = value; }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
