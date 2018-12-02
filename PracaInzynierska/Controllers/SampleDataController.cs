using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PracaInzynierska.Models;
using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Container;


namespace PracaInzynierska.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private static string StorageConnectionString =
            "DefaultEndpointsProtocol=https;AccountName=inzstorage;AccountKey=uA6F6BMRoHGP3RpfL2KVIF2shhawP3drGdRsZkPVJXYCfSTCq9VoL6A0uMKNKwpLLfke4UsQ2mUYFHJwGaQ/Ag==;EndpointSuffix=core.windows.net";
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
        }
        [HttpGet("[action]")]
        public async Task<ActionResult> Iot()
        {
            string blobString = await GetAllBlobsAsString();

            //IEnumerable<Data> messages = DeserializeMessages(blobString).ToList();

            return Ok(blobString);
            //return Ok(blobString);
        }

        private async Task<string> GetAllBlobsAsString()
        {
            //Dzia³a, zwraca wszystko
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            MemoryStream stream = new MemoryStream();
            List<string> blobs = new List<string>();
            List<Data> data = new List<Data>();
            
            BlobResultSegment resultSegment = blobContainer.ListBlobsSegmentedAsync("PracaInzynierkska/",true,BlobListingDetails.All,null,null,null,null).Result;
            foreach (IListBlobItem item in resultSegment.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    List<AvroRecord> avroRecords = await GetAvroRecordsAsync((CloudBlockBlob)item);
                    foreach (var avroRecord in avroRecords)
                    {
                        data.Add(GetMyObject(avroRecord));
                    }
                }
            }
            var dataJson = JsonConvert.SerializeObject(data);
            return dataJson;
        }

        private async Task<string> GetBlobAsString()
        {
            //Dzia³a pieknie i zwraca JSONa
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference("PracaInzynierkska/00/2018/11/03/20/15");
            List<Data> data = new List<Data>();
            List<AvroRecord> avroRecords = await GetAvroRecordsAsync(blob);
            foreach (var avroRecord in avroRecords)
            {
                data.Add(GetMyObject(avroRecord));
            }
            var dataJson = JsonConvert.SerializeObject(data);
            return dataJson;

        }

        private async Task<List<AvroRecord>> GetAvroRecordsAsync(CloudBlockBlob cloudBlockBlob)
        {
            var memoryStream = new MemoryStream();
            await cloudBlockBlob.DownloadToStreamAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            List<AvroRecord> avroRecords;
            using (var reader = AvroContainer.CreateGenericReader(memoryStream))
            {
                using (var sequentialReader = new SequentialReader<object>(reader))
                {
                    avroRecords = sequentialReader.Objects.OfType<AvroRecord>().ToList();
                }
            }

            return avroRecords;

        }

        private Data GetMyObject(AvroRecord avroRecord)
        {
            var body = avroRecord.GetField<byte[]>("Body");
            var dataBody = Encoding.UTF8.GetString(body);
            var myObj = JsonConvert.DeserializeObject<Data>(dataBody);
            return myObj;
        }

        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF
            {
                get
                {
                    return 32 + (int)(TemperatureC / 0.5556);
                }
            }
        }
    }
}
