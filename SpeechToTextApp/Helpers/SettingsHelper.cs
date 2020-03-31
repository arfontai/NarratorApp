using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

using SpeakerRecognition = Universal.Microsoft.CognitiveServices.SpeakerRecognition.V1;

using Plugin.AudioRecorder;
using System.Collections.ObjectModel;

using SpeechToTextApp.Model;
using System.Text;
using Universal.Microsoft.CognitiveServices.SpeakerRecognition.V1;
using Newtonsoft.Json;

namespace SpeechToTextApp.Helpers
{
    class SettingsHelper
    {
        static StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
        static string fileName = "settings.json";

        //private static Collection<Speaker> registeredSpeakers;
        //public static List<Speaker> RegisteredSpeakers
        //{
        //    get
        //    {
        //        return registeredSpeakers;
        //    }
        //    set
        //    {
        //        registeredSpeakers = value;
        //    }
        //}

        public static async Task RemoveFilesAsync()
        {
            var files = await storageFolder.GetFilesAsync();
            var filteredFiles = from f in files
                                where f.FileType == ".wav"
                                select f;
            foreach (var file in filteredFiles)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

            public static async Task<Collection<Speaker>> LoadSettingsAsync()
        {
            Collection<Speaker> speakers = new Collection<Speaker>();
            var settingsFile = await storageFolder.TryGetItemAsync(fileName);

            if (settingsFile != null)
            {
                string content = await Windows.Storage.FileIO.ReadTextAsync((StorageFile)settingsFile);
                speakers = JsonConvert.DeserializeObject<Collection<Speaker>>(content);
            }
            else
            {
                settingsFile = await storageFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);

                //registeredSpeakers = new List<Speaker>();
                
                //var speaker = new Speaker("Arnaud", Guid.Parse("f5d2c9bd-d5c2-4f88-b9be-d466d5d89351"));
                //speaker.Language = SupportedLanguage.French;
                //registeredSpeakers.Add(speaker);

                string content = JsonConvert.SerializeObject(speakers);
                await Windows.Storage.FileIO.WriteTextAsync((StorageFile)settingsFile, content);
            }
            return speakers;
        }

        public static async Task SaveSettingsAsync(Collection<Speaker> speakers)
        {
            var settingsFile = await storageFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            string content = JsonConvert.SerializeObject(speakers);
            await Windows.Storage.FileIO.WriteTextAsync((StorageFile)settingsFile, content);
        }
    }
}
