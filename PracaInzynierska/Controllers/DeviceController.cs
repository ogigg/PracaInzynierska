using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class DeviceController : Controller
    {
        private static string StorageConnectionString =
            "DefaultEndpointsProtocol=https;AccountName=inzstorage;AccountKey=uA6F6BMRoHGP3RpfL2KVIF2shhawP3drGdRsZkPVJXYCfSTCq9VoL6A0uMKNKwpLLfke4UsQ2mUYFHJwGaQ/Ag==;EndpointSuffix=core.windows.net";


        [HttpGet("[action]")]
        public async Task<ActionResult> Iot()
        {
            string blobString = await GetAllBlobsAsString();

            //IEnumerable<Data> messages = DeserializeMessages(blobString).ToList();

            return Ok(blobString);
        }

        [HttpGet("[action]")]
        public async Task<DeviceDataMessage> ExampleDataMessage(string deviceName)
        {
            var mess =  new DeviceDataMessage()
            {
                DeviceId="NodeMCU_test",
                Message="elo",
                MessageType = "data",
                Time = DateTime.Now,
                PortStatus = new Collection<PortStatus>()
            };
            mess.PortStatus.Add(new PortStatus() {Id = 0, ValueType = "double"});
            return mess;
        }


        [HttpGet("[action]")]
        public async Task<DeviceConfig> IotLastDataMessage(string deviceName)
        {
            List<DeviceConfig> blobDeviceInfo = await GetAllDeviceInfoBlobs();
            List<DeviceConfig> messagesFromDevice = new List<DeviceConfig>();
            foreach (DeviceConfig device in blobDeviceInfo)
            {
                if (device.MessageType != "configuration" && device.DeviceId == deviceName && device.Time != null)
                {
                    messagesFromDevice.Add(device);
                }
            }
            DeviceConfig lastDataMessage =  messagesFromDevice[messagesFromDevice.Count - 1];
            return lastDataMessage;
        }

        [HttpGet("[action]")]
        public async Task<List<DeviceConfig>> IotConfig()
        {
            List<DeviceConfig> blobDeviceInfo = await GetAllDeviceInfoBlobs();
            List<DeviceConfig> DeviceConfigInfo = new List<DeviceConfig>();
            List<DeviceConfig> FinalDeviceList = new List<DeviceConfig>();
            foreach (DeviceConfig device in blobDeviceInfo)
            {
                if (device.MessageType == "configuration") //pobiera tylko te z konfiguracja
                {
                  DeviceConfigInfo.Add(device);
                }
            }

            string s_connectionString =
            "HostName=PracaInzynierkska.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ib4HWDWL7wqBkzkGlDxhhfQ5n7ujqe3AVrWAYMQdPzA=";

            RegistryManager _registryManager = RegistryManager.CreateFromConnectionString(s_connectionString);
            var devices = await _registryManager.GetDevicesAsync(100);

            List<string> devicesNames = new List<string>();
            foreach (var device in devices)//Tworzy listê wszystkich urz¹dzeñ
            {
                devicesNames.Add(device.Id);
            }
            //wybiera ze wszystkich danych tylko te pasujace do nazwy urzadzenia i bierze ostatni z nich (najnowszy)
            foreach (string name in devicesNames) 
            {
                var tempDeviceInfoList = new List<DeviceConfig>();
                foreach (DeviceConfig device in DeviceConfigInfo)
                {
                    if (device.DeviceId == name) 
                        tempDeviceInfoList.Add(device);
                }
                if (tempDeviceInfoList.Count > 0) {
                    DeviceConfig item = tempDeviceInfoList[tempDeviceInfoList.Count - 1];
                    FinalDeviceList.Add(item);
                }
            }
            return FinalDeviceList;
        }

        [HttpGet("[action]")]
        public async Task<DeviceDataMessage> GetLastDataMessage(string deviceId)
        {
            List<DeviceDataMessage> allDeviceDataMessageBlobs = await GetAllDeviceDataMessageBlobs();
            List<DeviceDataMessage> dataMessageBlobsFromSelectedDevice = new List<DeviceDataMessage>();
            foreach (var deviceDataMessageBlob in allDeviceDataMessageBlobs)
            {
                if (deviceDataMessageBlob.DeviceId == deviceId)
                {
                    dataMessageBlobsFromSelectedDevice.Add(deviceDataMessageBlob);
                }
            }
            return dataMessageBlobsFromSelectedDevice[dataMessageBlobsFromSelectedDevice.Count - 1];

        }

        [HttpGet("[action]")]
        public async Task<List<DeviceConfig>> GetAllDeviceInfoBlobs()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            MemoryStream stream = new MemoryStream();
            List<string> blobs = new List<string>();
            List<DeviceConfig> data = new List<DeviceConfig>();

            BlobResultSegment resultSegment = blobContainer.ListBlobsSegmentedAsync("PracaInzynierkska/", true, BlobListingDetails.All, null, null, null, null).Result;
            foreach (IListBlobItem item in resultSegment.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    List<AvroRecord> avroRecords = await GetAvroRecordsAsync((CloudBlockBlob)item);
                    foreach (var avroRecord in avroRecords)
                    {
                        try
                        {
                            data.Add(ConvertAvroToDeviceConfig(avroRecord));
                        }
                        catch
                        {
                            try
                            {
                                var unknownType = ConvertAvroToDeviceConfigWithSingleParam(avroRecord);
                                data.Add(SingleParamToDeviceConfig(unknownType));
                            }
                            catch { }
                        }
                    }
                }
            }
            //var dataJson = JsonConvert.SerializeObject(data);
            return data;
        }
        [HttpGet("[action]")]
        public async Task<List<DeviceConfigSinglePort>> GetAllDeviceInfoWithSinglePortBlobs()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            MemoryStream stream = new MemoryStream();
            List<string> blobs = new List<string>();
            List<DeviceConfigSinglePort> data = new List<DeviceConfigSinglePort>();

            BlobResultSegment resultSegment = blobContainer.ListBlobsSegmentedAsync("PracaInzynierkska/", true, BlobListingDetails.All, null, null, null, null).Result;
            foreach (IListBlobItem item in resultSegment.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    List<AvroRecord> avroRecords = await GetAvroRecordsAsync((CloudBlockBlob)item);
                    foreach (var avroRecord in avroRecords)
                    {
                        try
                        {
                            data.Add(ConvertAvroToDeviceConfigWithSingleParam(avroRecord));
                        }
                        catch
                        {
                        }
                    }
                }
            }
            //var dataJson = JsonConvert.SerializeObject(data);
            return data;
        }


        [HttpGet("[action]")]
        public async Task<List<DeviceDataMessage>> GetAllDeviceDataMessageBlobs()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            MemoryStream stream = new MemoryStream();
            List<string> blobs = new List<string>();
            List<DeviceDataMessage> data = new List<DeviceDataMessage>();

            BlobResultSegment resultSegment = blobContainer.ListBlobsSegmentedAsync("PracaInzynierkska/", true, BlobListingDetails.All, null, null, null, null).Result;
            foreach (IListBlobItem item in resultSegment.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    List<AvroRecord> avroRecords = await GetAvroRecordsAsync((CloudBlockBlob)item);
                    foreach (var avroRecord in avroRecords)
                    {
                        try
                        {
                            DeviceDataMessage obj = GetDeviceDataMessageObject(avroRecord);
                            if(obj.MessageType == "data")
                                data.Add(obj);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return data;
        }

        [HttpGet("[action]")]
        public async Task<string> GetAllBlobsAsString()
        {
            //Dzia³a, zwraca wszystko
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            MemoryStream stream = new MemoryStream();
            List<string> blobs = new List<string>();
            List<DeviceConfig> data = new List<DeviceConfig>();
            
            BlobResultSegment resultSegment = blobContainer.ListBlobsSegmentedAsync("PracaInzynierkska/",true,BlobListingDetails.All,null,null,null,null).Result;
            foreach (IListBlobItem item in resultSegment.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    List<AvroRecord> avroRecords = await GetAvroRecordsAsync((CloudBlockBlob)item);
                    foreach (var avroRecord in avroRecords)
                    {
                        blobs.Add(GetBodyAsStringFromAvro(avroRecord));
                        //data.Add(ConvertAvroToDeviceConfig(avroRecord));
                    }
                }
            }
            string blobsAsString=  String.Join(", ", blobs.ToArray());
            return blobsAsString;
            //var dataJson = JsonConvert.SerializeObject(data);
            //return dataJson;
        }

        private async Task<string> GetBlobAsString()
        {
            //Dzia³a pieknie i zwraca JSONa
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("messages");
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference("PracaInzynierkska/00/2018/11/03/20/15");
            List<DeviceConfig> data = new List<DeviceConfig>();
            List<AvroRecord> avroRecords = await GetAvroRecordsAsync(blob);
            foreach (var avroRecord in avroRecords)
            {
                data.Add(ConvertAvroToDeviceConfig(avroRecord));
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

        private string GetBodyAsStringFromAvro(AvroRecord avroRecord)
        {
            var body = avroRecord.GetField<byte[]>("Body");
            var dataBody = Encoding.UTF8.GetString(body);
            return dataBody;
        }

        private DeviceConfig ConvertAvroToDeviceConfig(AvroRecord avroRecord)
        {
            var body = avroRecord.GetField<byte[]>("Body");
            var dataBody = Encoding.UTF8.GetString(body);
            var myObj = JsonConvert.DeserializeObject<DeviceConfig>(dataBody);
            return myObj;
        }

        private DeviceConfig SingleParamToDeviceConfig(DeviceConfigSinglePort configSinglePort)
        {
            var config = new DeviceConfig()
            {
                DeviceId = configSinglePort.DeviceId,
                Message = configSinglePort.Message,
                MessageType = configSinglePort.MessageType,
                Time = configSinglePort.Time,
                DeviceName= configSinglePort.DeviceName,
                PortAttributes = new Collection<PortAttributes>()
            };
            config.PortAttributes.Add(configSinglePort.PortAttributes);
            return config;
        }

        private DeviceConfigSinglePort ConvertAvroToDeviceConfigWithSingleParam(AvroRecord avroRecord)
        {
            var body = avroRecord.GetField<byte[]>("Body");
            var dataBody = Encoding.UTF8.GetString(body);
            var myObj = JsonConvert.DeserializeObject<DeviceConfigSinglePort>(dataBody);
            return myObj;
        }

        private DeviceDataMessage GetDeviceDataMessageObject(AvroRecord avroRecord)
        {
            var body = avroRecord.GetField<byte[]>("Body");
            var dataBody = Encoding.UTF8.GetString(body);
            var myObj = JsonConvert.DeserializeObject<DeviceDataMessage>(dataBody);
            return myObj;
        }

        
    }
}
