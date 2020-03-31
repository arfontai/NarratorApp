using SpeechToTextApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Universal.Microsoft.CognitiveServices.SpeakerRecognition.V1;
using SpeakerRecognition = Universal.Microsoft.CognitiveServices.SpeakerRecognition.V1;

namespace SpeechToTextApp.Helpers
{
    class IdentificationHelper
    {
        private string speakerIdentificationKey = "136f7f3c7c32459f802db2ad0b911bb7";
        SpeakerRecognitionClient client;
        //private List<string> profileIds = new List<string>(); 

        public IdentificationHelper()
        {
            client = new SpeakerRecognition.SpeakerRecognitionClient(speakerIdentificationKey);
        }
        
        public async Task<Collection<Speaker>> LoadProfilesAsync()
        {
            var speakers = new Collection<Speaker>();
            
            var profiles = await client.GetAllIdentificationProfilesAsync();
            foreach(var profile in profiles)
            {
                speakers.Add(new Speaker()
                {
                    SpeakerId = Guid.Parse(profile.IdentificationProfileId),
                    SpeakerName = profile.IdentificationProfileId,
                    Locale = profile.Locale,
                    EnrollmentSpeechTime = profile.EnrollmentSpeechTime,
                    EnrollmentStatus = profile.EnrollmentStatus,
                    RemainingEnrollmentSpeechTime = profile.RemainingEnrollmentSpeechTime
                });

                //if (!profile.EnrollmentStatus.Equals("Enrolling"))
                //{
                //    profileIds.Add(profile.IdentificationProfileId);
                //}
            }
            return speakers;
        }

        public async Task<Guid> CreateProfileAsync(string locale)
        {
            CreateIdentificationProfileRequest request = new CreateIdentificationProfileRequest();
            request.Locale = locale;
         
            var response = await client.CreateIdentificationProfileAsync(request);
            return Guid.Parse(response.IdentificationProfileId);
        }

        public async Task DeleteProfileAsync(Guid profileId)
        {
            await client.DeleteIdentificationProfileAsync(profileId.ToString());

            //profileIds.Remove(profileId.ToString());
        }

        public async Task<CreateIdentificationProfileEnrollmentResponse> AddEnrollmentAsync(Guid profileId, string filePath)
        {
            var audioInput = await File.ReadAllBytesAsync(filePath);
            return await client.CreateIdentificationProfileEnrollmentAsync(profileId.ToString(), audioInput, false);
        }

        public async Task<Speaker> GetProfileAsync(Guid profileId)
        {
            var profile = await client.GetIdentificationProfileAsync(profileId.ToString());

            var speaker = new Speaker()
            {
                SpeakerId = profileId,
                EnrollmentSpeechTime = profile.EnrollmentSpeechTime,
                EnrollmentStatus = profile.EnrollmentStatus,
                RemainingEnrollmentSpeechTime = profile.RemainingEnrollmentSpeechTime
            };

            //if (!profile.EnrollmentStatus.Equals("Enrolling"))
            //{
            //    profileIds.Add(profile.IdentificationProfileId);
            //}
            return speaker;
        }

        public async Task<GetOperationStatusResponse> IdentifySpeakerAsync(string filePath, IEnumerable<string> profileIds)
        {
            var audioInput = await File.ReadAllBytesAsync(filePath);
            var ir = await client.IdentifyAsync(profileIds, audioInput, true);

            SpeakerRecognition.GetOperationStatusResponse ope = null;
            while (ope == null || ope.Status.Equals("notstarted") || ope.Status.Equals("running"))
            {
                ope = await client.GetOperationStatusAsync(ir.OperationId);
                await Task.Delay(100);
            }

            return ope;
        }
    }
}
