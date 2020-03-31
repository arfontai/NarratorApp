using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using SpeechToTextApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToTextApp.Helpers
{
    class SpeechToTextHelper
    {
        static string speechConfig_SubscriptionID = "6f38b2dfc79c4a838aeac5745c8d9e86";
        static string speechConfig_AzureZone = "westeurope";

        //return the Message identified 
        public static async Task<Model.Message> RunSpeechToTextAsync(string filePath, string speakerLanguage)
        {
            var result = new Model.Message();

            using (var audioInput = AudioConfig.FromWavFileInput(filePath))
            {
                var speechConfig = SpeechConfig.FromSubscription(speechConfig_SubscriptionID, speechConfig_AzureZone);
                speechConfig.SpeechRecognitionLanguage = speakerLanguage;

                using (var recognizer = new SpeechRecognizer(speechConfig, audioInput))
                {
                    var recognizerResult = await recognizer.RecognizeOnceAsync();

                    if (recognizerResult.Reason == ResultReason.RecognizedSpeech)
                    {
                        result.MessageText = recognizerResult.Text;
                    }
                    else if (recognizerResult.Reason == ResultReason.NoMatch)
                    {
                        //TODO: ajouter un niveau de détail verbose

                        //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                        //    Messages.Insert(0, new Message("Error", "Speech could not be recognized."));
                        //});
                    }
                    else if (recognizerResult.Reason == ResultReason.Canceled)
                    {
                        var cancellation = CancellationDetails.FromResult(recognizerResult);
                        result = new Message("Error", $"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            sb.AppendLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            sb.AppendLine($"CANCELED: Did you update the subscription info?");

                            result = new Message("Error", sb.ToString());
                        }
                    }
                }
            }
            return result;
        }
    }
}
