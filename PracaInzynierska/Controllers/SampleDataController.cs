using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PracaInzynierska.Models;
using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Container;
using PracaInzynierska.ControllersB;


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

        [HttpGet("[action]")]
        public async Task<List<DeviceInfo>> IotConfig()
        {
            List<DeviceInfo> blobDeviceInfo = await GetAllBlobsAsDeviceInfo();
            List<DeviceInfo> DeviceConfigInfo = new List<DeviceInfo>();
            List<DeviceInfo> FinalDeviceList = new List<DeviceInfo>();
            foreach (DeviceInfo device in blobDeviceInfo)
            {
                if (device.MessageType == "configuration")
                {
                    if (device.Time != null)
                    {
                        DeviceConfigInfo.Add(device);
                    }
                    
                }
                
            }
            string s_connectionString =
            "HostName=PracaInzynierkska.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ib4HWDWL7wqBkzkGlDxhhfQ5n7ujqe3AVrWAYMQdPzA=";

            RegistryManager _registryManager = RegistryManager.CreateFromConnectionString(s_connectionString);
            var devices = await _registryManager.GetDevicesAsync(100);

            List<string> devicesNames = new List<string>();
            foreach (var device in devices)
            {
                devicesNames.Add(device.Id);
            }

            foreach (string name in devicesNames)
            {
                var tempDeviceInfoList = new List<DeviceInfo>();
                foreach (DeviceInfo device in DeviceConfigInfo)
                {
                    if (device.DeviceId == name) 
                        tempDeviceInfoList.Add(device);
                }
                if (tempDeviceInfoList.Count > 0) {
                    DeviceInfo item = tempDeviceInfoList[tempDeviceInfoList.Count - 1];
                    FinalDeviceList.Add(item);
                }
                    
            }
            //IoTController iotcontroler = new IoTController();
            //List<string> names = await iotcontroler.GetDevicesNames();
            return FinalDeviceList;
        }

        private async Task<List<DeviceInfo>> GetAllBlobsAsDeviceInfo()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            MemoryStream stream = new MemoryStream();
            List<string> blobs = new List<string>();
            List<DeviceInfo> data = new List<DeviceInfo>();

            BlobResultSegment resultSegment = blobContainer.ListBlobsSegmentedAsync("PracaInzynierkska/", true, BlobListingDetails.All, null, null, null, null).Result;
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
            //var dataJson = JsonConvert.SerializeObject(data);
            return data;
        }
        private async Task<string> GetAllBlobsAsString()
        {
            //Dzia³a, zwraca wszystko
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            MemoryStream stream = new MemoryStream();
            List<string> blobs = new List<string>();
            List<DeviceInfo> data = new List<DeviceInfo>();
            
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
            List<DeviceInfo> data = new List<DeviceInfo>();
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

        private DeviceInfo GetMyObject(AvroRecord avroRecord)
        {
            var body = avroRecord.GetField<byte[]>("Body");
            var dataBody = Encoding.UTF8.GetString(body);
            var myObj = JsonConvert.DeserializeObject<DeviceInfo>(dataBody);
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
