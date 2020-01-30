using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

using Plugin.AudioRecorder;
using System.Collections.ObjectModel;

namespace SpeechToTextApp
{
    public class Message
    {
        private string speakerName;
        private string messageText;

        public Message(string speakerName, string messageText)
        {
            this.speakerName = speakerName;
            this.messageText = messageText;
        }

        public string SpeakerName
        {
            get { return speakerName; }
            set { speakerName = value; }
        }

        public string MessageText
        {
            get { return messageText; }
            set { messageText = value; }
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SpeechConfig config = SpeechConfig.FromSubscription("6f38b2dfc79c4a838aeac5745c8d9e86", "westeurope");
        AudioRecorderService recorder;
        bool isInRecordingMode = false;
        ObservableCollection<Message> Messages;

        //SpeechRecognizer recognizer;
        //private object threadLocker = new object();
        //private bool recognitionStarted = false;
        //private string message;

        public MainPage()
        {
            this.InitializeComponent();

            Messages = new ObservableCollection<Message>();
        }

        private void StopRecordButton_Click(object sender, RoutedEventArgs e)
        {
            isInRecordingMode = false;
        }

        private async void StartRecordButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                Messages.Insert(0, new Message("App", "start recording...."));
            });

            await StartRecordAsync();
        }

        private async Task StartRecordAsync()
        {
            isInRecordingMode = true;

            recorder = new AudioRecorderService
            {
                StopRecordingOnSilence = true,
                StopRecordingAfterTimeout = false,
                FilePath = $@"C:\Users\arfontai\AppData\Local\Packages\4845d745-bd85-48c3-8103-dac1116b8293_7bx13berc2gct\TempState\{Guid.NewGuid().ToString()}.wav"
                //FilePath = $@"C:\Users\arfontai\AppData\Local\Packages\4845d745-bd85-48c3-8103-dac1116b8293_7bx13berc2gct\TempState\Temp.wav"
            };

            recorder.AudioInputReceived += Recorder_AudioInputReceived;

            await RecordAudio();
        }

        async Task RecordAudio()
        {
            try
            {
                if (!recorder.IsRecording)
                {
                    await recorder.StartRecording();
                }
                else
                {
                    await recorder.StopRecording();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void Recorder_AudioInputReceived(object sender, string audioFile)
        {
            AudioRecorderService service = (AudioRecorderService)sender;
            config = SpeechConfig.FromSubscription("6f38b2dfc79c4a838aeac5745c8d9e86", "westeurope");

            using (var audioInput = AudioConfig.FromWavFileInput(service.FilePath))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    Console.WriteLine("Recognizing first result...");
                    var result = await recognizer.RecognizeOnceAsync();

                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        //Console.WriteLine($"We recognized: {result.Text}");

                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                            Messages.Insert(0, new Message("Arnaud", result.Text));
                        });

                    }
                    else if (result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = CancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }
                }
            }
            File.Delete(service.FilePath);
        }
    }
}
