using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using PracaInzynierska.Hubs;
using PracaInzynierska.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Devices.Client.Exceptions;
using PracaInzynierska.Controllers;
using PracaInzynierska.Others;


namespace PracaInzynierska.ControllersB
{
    [Route("api/[controller]")]
    [ApiController]
    public class IoTController : ControllerBase
    {
        private static ServiceClient s_serviceClient;
        private readonly static string s_connectionString =
            "HostName=PracaInzynierkska.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ib4HWDWL7wqBkzkGlDxhhfQ5n7ujqe3AVrWAYMQdPzA=";

        private static IHubContext<IoTSignalRHub> _hubContext;
        private static RegistryManager _registryManager = RegistryManager.CreateFromConnectionString(s_connectionString);
        private readonly ISignalR _signalRController;

        public IoTController(IHubContext<IoTSignalRHub> hubContext, ISignalR signalRController)
        {
            _hubContext = hubContext;
            _signalRController = signalRController;
        }


        [HttpGet("SignalR")]
        public IActionResult SignalRActionResult(string name, string message)
        {
            _signalRController.SendMessage(name, message);
            return Ok("send!");
        }

        [HttpGet("[action]")]
        public async Task<ConnectionString> GetDeviceConnectionString (string deviceId)
        {
            Device device = await _registryManager.GetDeviceAsync(deviceId);//wyszukaj urządzenie na liście

            if (device != null)//jeśli urządzenie jest na liście
            {
                return new ConnectionString() //zwróć dane
                {
                    primaryKey = device.Authentication.SymmetricKey.PrimaryKey,
                    secondaryKey = device.Authentication.SymmetricKey.SecondaryKey
                };
            }
            return null; //zwróć pustą wartość jeśli urządzenia nie ma na liście
        }

        [HttpGet("[action]")]
        public async Task<Device> GetAllDeviceData (string deviceId)
        {
            Device device;
            device = await _registryManager.GetDeviceAsync(deviceId);//wyszukaj urządzenie na liście
            return device; //zwróć wszystkie dane urządzenia
        }

        [HttpGet("[action]")]
        public async Task<Device> CreateNewDevice (string deviceId)
        {
            Device device;
            device = await _registryManager.GetDeviceAsync(deviceId);//wyszukaj urządzenie na liście

            if (device == null)//jeśli nie ma urządzenia na liście
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceId)); //utwórz nowe urządzenie
                return device;//zwróć dane urządznia
            }

            return null; //nie zwracaj nic jeśli urządzenie istnieje



        }

        [HttpPost("[action]")]
        public IActionResult SendC2DOld(string message, string deviceName)
        {
            s_serviceClient = ServiceClient.CreateFromConnectionString(s_connectionString);
            dynamic C2DMessage = new
            {
                Name = "WriteToLCD",
                Parameters = new
                {
                    Text = message
                }
            };

            var messageJson = JsonConvert.SerializeObject(C2DMessage);

            var messageArray = Encoding.UTF8.GetBytes(messageJson);
            var commandMessage = new Message(messageArray);
            s_serviceClient.SendAsync(deviceName, commandMessage);
            return Ok(messageJson);
        }

        [HttpPost("[action]")]
        public IActionResult SendC2DCommand(string deviceName, string functionName, string parameterName, string parameterValue)
        {
            s_serviceClient = ServiceClient.CreateFromConnectionString(s_connectionString);
            dynamic C2DMessage = new
            {
                Name = functionName,
                Parameters = new
                {
                    Text = parameterValue
                }
            };


            var messageJson = JsonConvert.SerializeObject(C2DMessage);
            var messageArray = Encoding.UTF8.GetBytes(messageJson);
            var commandMessage = new Message(messageArray);
            s_serviceClient.SendAsync(deviceName, commandMessage);
            return Ok(messageJson);
        }

        [HttpPost("[action]")]
        public IActionResult SendC2DtoNodeMCU(bool LED)
        {
            s_serviceClient = ServiceClient.CreateFromConnectionString(s_connectionString);


            dynamic TurnLED = new
            {
                Name = "TurnLED",
                Parameters = new
                {
                    IsTurnedON = LED
                }
            };

            var dynJson = JsonConvert.SerializeObject(TurnLED);
            var dynArray = Encoding.UTF8.GetBytes(dynJson);
            var commandMessage = new Message(dynArray);
            s_serviceClient.SendAsync("nodeMCU", commandMessage);
            return Ok(dynJson);
        }

        private static async Task InvokeMethod(string message)
        {
            var methodInvocation = new CloudToDeviceMethod("SendMessage") {ResponseTimeout = TimeSpan.FromSeconds(30)};

            message = JsonConvert.SerializeObject(new {message = message});
            methodInvocation.SetPayloadJson(message);

            // Invoke the direct method asynchronously and get the response from the simulated device.
            var response = await s_serviceClient.InvokeDeviceMethodAsync("SymulowaneUrzadzenie", methodInvocation);

            //Console.WriteLine("Response status: {0}, payload:", response.Status);
            //Console.WriteLine(response.GetPayloadAsJson());
            ////var methodInvocation = new CloudToDeviceMethod("SetTelemetryInterval") { ResponseTimeout = TimeSpan.FromSeconds(30) };
            //methodInvocation.SetPayloadJson("10");

            //// Invoke the direct method asynchronously and get the response from the simulated device.
            //var response = await s_serviceClient.InvokeDeviceMethodAsync("SymulowaneUrzadzenie", methodInvocation);

            //Console.WriteLine("Response status: {0}, payload:", response.Status);
            //Console.WriteLine(response.GetPayloadAsJson());
        }

        [HttpGet("[action]")]
        public async Task<List<string>> GetDevicesNames() //Zwraca listę nazw wszystkich połączonych urządzeń 
        {
            var devices = await _registryManager.GetDevicesAsync(100);

            List<string> devicesNames = new List<string>();
            foreach (var device in devices)
            {
                devicesNames.Add(device.Id);
            }

            return devicesNames;
        }

        [HttpGet("[action]")]
        public DeviceConfig GetDeviceInfo()
        {

            DeviceConfig deviceConfig = new DeviceConfig()
            {
                DeviceId = "b555-b34c59e051a9",
                DeviceName = "Testoweurzadzenie1",
                Message = "To jest test dynamicznego generowania interfejsu",
            };
            deviceConfig.PortAttributes.Add(new PortAttributes()
            {
                Id = 1,
                Name = "WriteToLCD",
                Label = "Wiadomosc na ekranie LCD:",
                GPIOType = "input",
                ValueType = "string",
                MinValue = 0,
                MaxValue = 16
            });
            deviceConfig.PortAttributes.Add(new PortAttributes()
            {
                Id = 2,
                Name = "LED1",
                Label = "Sterowanie Dioda 1: ",
                GPIOType = "input",
                ValueType = "bool"
            });
            deviceConfig.PortAttributes.Add(new PortAttributes()
            {
                Id = 3,
                Name = "MoveServo",
                Label = "Wychylenie serwa (%): ",
                GPIOType = "input",
                ValueType = "float",
                Unit = "%",
                MaxValue = 100,
                MinValue = 0
            });
            return deviceConfig;
        }


    }
}
