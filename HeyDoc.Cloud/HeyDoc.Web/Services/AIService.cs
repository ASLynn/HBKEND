using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    /// <summary>
    /// AI Service
    /// </summary>
    public class AIService
    {
        /// <summary>
        /// Upload medical picture
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="classifier">Classifier</param>
        /// <param name="fileName">File name</param>
        /// <returns>AI Result</returns>
        public static async Task<AIImageRequest> Medical(Stream fileStream, string classifier, string fileName)
        {
            try
            {
                var client = new RestClient("http://ai.medicmind.tech:5000/");
                var request = new RestRequest();
                request.Method = Method.POST;
                request.AddParameter("classifier", classifier);
                request.AddFile("file", ReadToEnd(fileStream), fileName);
                var response = await client.ExecuteAsync(request);
                var data = JsonConvert.DeserializeObject<AIImageRequest>(response.Content);
                return data;
            }
            catch (Exception e)
            {
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.BadRequest, "The software can only detect mole or skin lump. Please try again."));
            }
        }

        public static string GetRiskLevel(double score)
        {
            if (score > 0.7)
            {
                return "High";
            }
            else if (score < 0.5)
            {
                return "Low";
            }
            return "Moderate";
        } 

        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}