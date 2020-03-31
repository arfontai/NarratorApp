using Plugin.AudioRecorder;
using SpeechToTextApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SpeechToTextApp.Helpers
{
    public class RecorderHelper
    {
        AudioRecorderService recorder;
        StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
        IRecordAction _actionHandler;

        public RecorderParams Params { get; set; }

        //public bool IsRecording
        //{
        //    get
        //    {
        //        if (recorder == null)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return recorder.IsRecording;
        //        }
        //    }
        //}

        public bool IsInRecordingMode { get; set; } = false;

        public RecorderHelper(RecorderParams parameters, IRecordAction actionHandler)
        {
            Params = parameters;
            _actionHandler = actionHandler;
        }

        public async Task StartRecordAsync()
        {
            IsInRecordingMode = true;
            
            var filePath = storageFolder.Path + @"\" +
                DateTime.Now.Year + "-" +
                DateTime.Now.Month + "-" +
                DateTime.Now.Day + "_" +
                DateTime.Now.Hour + "." +
                DateTime.Now.Minute + "." +
                DateTime.Now.Second + "." +
                DateTime.Now.Millisecond + ".wav";

            recorder = new AudioRecorderService
            {
                StopRecordingOnSilence = true, //will stop recording after 2 seconds (default)
                StopRecordingAfterTimeout = true,  //stop recording after a max timeout (defined below)
                AudioSilenceTimeout = TimeSpan.FromMilliseconds(Params.SilenceTimeout * 1000),
                SilenceThreshold = Params.SilenceThreshold, // float between 0 and 1
                TotalAudioTimeout = TimeSpan.FromSeconds(Params.TimeoutLimit),
                FilePath = filePath,
                PreferredSampleRate = 16000
            };

            recorder.AudioInputReceived += Recorder_AudioInputReceived;
            //await RecordAudio();
            await recorder.StartRecording();
        }

        //private async Task RecordAudio()
        //{
        //    try
        //    {
        //        if (!recorder.IsRecording)
        //        {
        //            await recorder.StartRecording();
        //        }
        //        else
        //        {
        //            await recorder.StopRecording();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

        private async void Recorder_AudioInputReceived(object sender, string audioFile)
        {
            // We relanch the recorder, to follow the discussion
            if (IsInRecordingMode)
            {
                await StartRecordAsync();
            }

            var file = ((AudioRecorderService)sender);
            // 2s = approximativally 75kb
            if (file.GetAudioFileStream().Length > Params.SilenceTimeout * 37500)
            {
                _actionHandler.RunActionOnRecordAsync(file.FilePath);
            }
            else
            {
                //File.Delete(file.FilePath);
            }
        }
    }
}
