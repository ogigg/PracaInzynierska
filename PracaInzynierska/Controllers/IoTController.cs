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


namespace PracaInzynierska.ControllersB
{
    [Route("api/[controller]")]
    [ApiController]
    public class IoTController : ControllerBase
    {
        private static ServiceClient s_serviceClient;
        private readonly static string s_connectionString = "HostName=PracaInzynierkska.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ib4HWDWL7wqBkzkGlDxhhfQ5n7ujqe3AVrWAYMQdPzA=";


        [HttpPost("[action]")]
        public IActionResult SendC2D(string message)
        {
            s_serviceClient = ServiceClient.CreateFromConnectionString(s_connectionString);
            InvokeMethod(message).GetAwaiter().GetResult();
            return Ok("Send!");
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
            return Ok("Send!");
        }

        private static async Task InvokeMethod(string message)
        {
            var methodInvocation = new CloudToDeviceMethod("SendMessage") { ResponseTimeout = TimeSpan.FromSeconds(30) };
            
            message = JsonConvert.SerializeObject(new { message = message });
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
            var registryManager = RegistryManager.CreateFromConnectionString(s_connectionString);

            var devices = await registryManager.GetDevicesAsync(100);

            List<string> devicesNames = new List<string>();

            foreach (var device in devices)
            {
                devicesNames.Add(device.Id); 
            }
            

            return devicesNames;
        }

    }
}