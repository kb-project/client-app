using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace kbdwrfunctions
{
    public static class ProcessPhotoQueue
    {
        [FunctionName("ProcessPhotoQueue")]
        public static void Run([QueueTrigger("photo-test", Connection = "StorageConnection")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            var jsonData = myQueueItem;

            Photo photo = new Photo();
            photo = JsonConvert.DeserializeObject<Photo>(jsonData);

            // Add Face 
            var photoResult = AddPhoto(photo);
            log.Info(photoResult);

            // Training PersonGroup
            var trainResult = TrainPersonGroup(photo);
            log.Info(trainResult);

        }

        private static string AddPhoto(Photo photo)
        {
            string baseUrl = "https://eastasia.api.cognitive.microsoft.com/face/v1.0/";
            string addPhotoUrl = baseUrl + "persongroups/" + photo.personGroupId + "/persons/" + photo.personId + "/persistedFaces";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "d34788525010436ba92a2fdea1463ec4");

                string httpBody = "{\"url\":\"" +
                    photo.blobUrl + "\"}";
                var httpContent = new StringContent(httpBody, Encoding.UTF8, "application/json");

                var response = client.PostAsync(addPhotoUrl, httpContent).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return "Http ��û�� �����߽��ϴ�.";
                }

                response.EnsureSuccessStatusCode();
                client.Dispose();

                // JSON ���� �� ���� 
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        private static string TrainPersonGroup(Photo photo)
        {
            string baseUrl = "https://eastasia.api.cognitive.microsoft.com/face/v1.0/";
            string trainGroupUrl = baseUrl + "persongroups/" + photo.personGroupId + "/train";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "d34788525010436ba92a2fdea1463ec4");

                var response = client.PostAsync(trainGroupUrl, null).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return "Http ��û�� �����߽��ϴ�.";
                }

                response.EnsureSuccessStatusCode();
                client.Dispose();

                // JSON ���� �� ���� 
                return response.Content.ReadAsStringAsync().Result;
            }

        }
    }

    public class Photo
    {
        public string personGroupId { get; set; }
        public string personId { get; set; }
        public string blobUrl { get; set; }
    }
}
