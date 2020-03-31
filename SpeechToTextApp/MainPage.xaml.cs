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



using Plugin.AudioRecorder;
using System.Collections.ObjectModel;

using SpeechToTextApp.Model;
using System.Text;
using Universal.Microsoft.CognitiveServices.SpeakerRecognition.V1;
using Newtonsoft.Json;

using SpeechToTextApp.Helpers;
using Windows.UI;
using System.Net.Http;

// https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/conversation-transcription

namespace SpeechToTextApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IRecordAction
    {
        private bool isInitialized = false;
        //private bool isRecording = false;
        private IdentificationHelper identificationHelper;
        private RecorderHelper recorderHelper;
        public RecorderParams RecorderParams { get; set; }

        public string SpeakerLanguage { get; set; }

        ObservableCollection<Message> Messages { get; }
        ObservableCollection<Speaker> Speakers { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            identificationHelper = new IdentificationHelper();

            RecorderParams = new RecorderParams(2, 0.15f, 10);
            recorderHelper = new RecorderHelper(RecorderParams, this);

            Messages = new ObservableCollection<Message>();
            Speakers = new ObservableCollection<Speaker>();
        }

        private async Task InitializeAsync()
        {
            isInitialized = true;
            await SettingsHelper.RemoveFilesAsync();

            var savedProfiles = await SettingsHelper.LoadSettingsAsync();
            var profiles = await identificationHelper.LoadProfilesAsync();

            foreach (var profile in profiles)
            {
                if (Speakers.FirstOrDefault(s => s.SpeakerId == profile.SpeakerId) == null)
                {
                    var savedProfile = savedProfiles.FirstOrDefault(p => p.SpeakerId == profile.SpeakerId);
                    if (savedProfile != null)
                    {
                        profile.SpeakerName = savedProfile.SpeakerName;
                        profile.DialogColor = savedProfile.DialogColor;
                    }
                    else
                    {
                        profile.SpeakerName = profile.SpeakerId.ToString();
                    }
                    Speakers.Add(profile);
                }
            }

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Messages.Insert(0, new Message("App", "Profiles loaded."));
            });

            //Speakers.Add(new Speaker("Arnaud", Guid.NewGuid()));

            //await SettingsHelper.LoadSettingsAsync();
            //var test = SettingsHelper.RegisteredSpeakers;


            //string key = "136f7f3c7c32459f802db2ad0b911bb7";
            //var client = new SpeakerRecognition.SpeakerRecognitionClient(key);

            //var profiles = await client.GetAllIdentificationProfilesAsync();

            //string profileId = profiles[0].IdentificationProfileId;
            //string filePath = storageFolder.Path + @"\" + "2020-2-2_11.46.8.251.wav";

            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            //    Messages.Insert(0, new Message("Profile", profileId));
            //});

            //var audioInput = await File.ReadAllBytesAsync(filePath);
            ////var response = await client.CreateIdentificationProfileEnrollmentAsync(profileId, audioInput, true);

            ////await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            ////{
            ////    Messages.Insert(0, new Message("Enrollement", response.OperationId));
            ////});

            //var ids = new List<string>() { profiles[0].IdentificationProfileId };
            //var ir = await client.IdentifyAsync(ids, audioInput, true);

            //var ope = await client.GetOperationStatusAsync(ir.OperationId);
        }

                     
        private async void StartRecordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!recorderHelper.IsInRecordingMode)
                {
                    if (!isInitialized)
                    {
                        await InitializeAsync();
                    }

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Messages.Insert(0, new Message("App", "start recording...."));
                    });

                    await recorderHelper.StartRecordAsync();

                    StopRecordButton.IsEnabled = true;
                    CreateProfileButton.IsEnabled = true;
                    AddEnrollmentButton.IsEnabled = true;
                }
            }
            catch(HttpRequestException)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Insert(0, new Message("Error", "An error occured when we tried to reach the cloud. Check your Internet connection."));
                });
            }
            catch (Exception)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Insert(0, new Message("Error", "An unkown error occured."));
                });
            }
        }

        private void StopRecordButton_Click(object sender, RoutedEventArgs e)
        {
            recorderHelper.IsInRecordingMode = false;
        }

        private async void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isInitialized) { await InitializeAsync(); }

                string locale = LanguageComboBox.SelectionBoxItem.ToString();
                var profileId = await identificationHelper.CreateProfileAsync(locale);
                Speakers.Add(new Speaker() { Locale = locale, SpeakerId = profileId, SpeakerName = profileId.ToString() });

                await SettingsHelper.SaveSettingsAsync(Speakers);
            }
            catch (HttpRequestException)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Insert(0, new Message("Error", "An error occured when we tried to reach the cloud. Check your Internet connection."));
                });
            }
            catch (Exception)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Insert(0, new Message("Error", "An unkown error occured."));
                });
            }
        }

        private async void AddEnrollmentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var messages = MessageList.SelectedItems;
                var speakerId = ((Speaker)SpeakerList.SelectedItem).SpeakerId;

                foreach (Message message in messages)
                {
                    if (message.FilePath != null && message.FilePath.Length > 0)
                    {
                        await identificationHelper.AddEnrollmentAsync(speakerId, message.FilePath);
                    }
                }

                var speaker = await identificationHelper.GetProfileAsync(speakerId);

                Speakers[SpeakerList.SelectedIndex] = speaker;
            }
            catch (HttpRequestException)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Insert(0, new Message("Error", "An error occured when we tried to reach the cloud. Check your Internet connection."));
                });
            }
            catch (Exception)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Insert(0, new Message("Error", "An unkown error occured."));
                });
            }
        }
                     
        private void TimeoutSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (RecorderParams != null)
            {
                Slider slider = (Slider)sender;
                RecorderParams.TimeoutLimit = (int)slider.Value;
            }
        }

        private void SilenceTimeoutSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (RecorderParams != null)
            {
                Slider slider = (Slider)sender;
                RecorderParams.SilenceTimeout = (int)slider.Value;
            }
        }

        private void SilenceThresholdSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (RecorderParams != null)
            {
                Slider slider = (Slider)sender;
                RecorderParams.SilenceThreshold = (float)slider.Value;
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            SpeakerLanguage = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
        }

        //private void SpeakerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Speaker removed = e.RemovedItems.FirstOrDefault() as Speaker;
        //    if (removed != null)
        //    {
        //        removed.IsReadOnly = true;
        //    }

        //    Speaker added = e.AddedItems.FirstOrDefault() as Speaker;
        //    if (added != null)
        //    {
        //        added.IsReadOnly = false;
        //    }
        //}

        private async void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;

            var speaker = Speakers.FirstOrDefault(s => s.SpeakerId.ToString().Equals(box.Tag.ToString()));
            var index = Speakers.IndexOf(speaker);
            speaker.SpeakerName = box.Text;
            Speakers[index] = speaker;

            await SettingsHelper.SaveSettingsAsync(Speakers);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Delete profile permanently?",
                Content = "If you delete this profile, you won't be able to identify this speaker anymore.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            // we delete the Profile.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                var button = (Button)sender;
                var speaker = Speakers.FirstOrDefault(s => s.SpeakerId.ToString().Equals(button.Tag.ToString()));
                
                Speakers.Remove(speaker);
                await identificationHelper.DeleteProfileAsync(speaker.SpeakerId);

                await SettingsHelper.SaveSettingsAsync(Speakers);
            }
            else
            {
                // The user clicked the CloseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }

        private async void myColorPicker_LostFocus(object sender, RoutedEventArgs e)
        {
            var colorPicker = (ColorPicker)sender;
            var speaker = Speakers.FirstOrDefault(s => s.SpeakerId.ToString().Equals(colorPicker.Tag.ToString()));
            var index = Speakers.IndexOf(speaker);
            speaker.DialogColor = colorPicker.Color;
            Speakers[index] = speaker;

            await SettingsHelper.SaveSettingsAsync(Speakers);
        }


        #region Implementation of interface IRecordAction
        public async void RunActionOnRecordAsync(string filePath)
        {
            // We run a speech to Text recognition
            var message = await SpeechToTextHelper.RunSpeechToTextAsync(filePath, SpeakerLanguage);

            if (!message.MessageText.Equals("N/A"))
            {
                var speakerLabel = "Not yet identified";

                // we insert the message without the speaker identification
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Insert(0, new Message(speakerLabel, message.MessageText) { FilePath = filePath });
                });

                try
                {
                    Color backgroundColor = Color.FromArgb(128, 128, 128, 128);
                    Color borderColor = Color.FromArgb(255, 68, 68, 68);

                    IEnumerable<string> speakerIds =
                        from sp in Speakers
                        where !sp.EnrollmentStatus.Equals("Enrolling")
                        select sp.SpeakerId.ToString();

                    // We run a speaker recognition
                    var identificationToken = await identificationHelper.IdentifySpeakerAsync(filePath, speakerIds);

                    Speaker speaker = null;
                    if (!identificationToken.Status.Equals("failed"))
                    {
                        Guid identifiedId = Guid.Parse(identificationToken.ProcessingResult.IdentificationProfileId);
                        if (identifiedId != Guid.Empty)
                        {
                            speaker = Speakers.FirstOrDefault(s => s.SpeakerId == identifiedId);
                            speakerLabel = speaker.SpeakerName + " (" + identificationToken.ProcessingResult.Confidence + ")";

                            backgroundColor = speaker.DialogColor;
                            backgroundColor.A = 128;
                            borderColor = speaker.DialogColor;
                        }
                        else
                        {
                            speakerLabel = "Speaker not identified";
                        }
                    }
                    else
                    {
                        speakerLabel = "Speaker not identified";
                    }

                    // we update the message with the speaker identified 
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        var item = Messages.FirstOrDefault(m => m.MessageText.Equals(message.MessageText));
                        if (item != null)
                        {
                            int i = Messages.IndexOf(item);
                            var msg = new Message()
                            {
                                FilePath = item.FilePath,
                                SpeakerName = speakerLabel,
                                SpeakerStyle = borderColor.ToString(),
                                SpeakerColor = backgroundColor.ToString(),
                                MessageText = item.MessageText
                            };
                            Messages[i] = msg;
                        }
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Messages.Insert(0, new Message("Error", ex.Message));
                    });
                }

                //File.Delete(filePath);
            }
        }
        #endregion
    }
}
