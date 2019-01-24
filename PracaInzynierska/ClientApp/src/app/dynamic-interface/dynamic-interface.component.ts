import { IotService } from './../services/iot.service';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
@Component({
  selector: 'app-dynamic-interface',
  templateUrl: './dynamic-interface.component.html',
  styleUrls: ['./dynamic-interface.component.css'],
})
export class DynamicInterfaceComponent implements OnInit {
  deviceInfoArray;
  deviceInfo;
  devicesNames;
  currentDevice;
  deviceData;
  private hubConnection: HubConnection;
  nick = '';
  message = '';
  messages: string[] = [];
  constructor(private iotService: IotService) {
    iotService.getDeviceInfo().subscribe(response=>{
      this.deviceInfoArray=response;
      console.log(this.deviceInfoArray);
    });
    iotService.getDevicesNames().subscribe(response=>{
      this.devicesNames=response;
      console.log(this.devicesNames);
    });
    
   }
   convertToBoolean(input: string): boolean | undefined {
    try {
        return JSON.parse(input);
    }
    catch (e) {
        return undefined;
    }
}
  ngOnInit() {
  this.hubConnection =     new HubConnectionBuilder().withUrl("http://localhost:5000/iotsignalrchat").build();
  this.hubConnection
    .start()
    .then(() => console.log('Connection started!'))
    .catch(err => console.log('Error while establishing connection :('));
    
    this.hubConnection.on('sendToAll', (nick: string, receivedMessage: string) => {
      const text = `${nick}: ${receivedMessage}`;
      if(nick=="EventHub"){
        var receivedJSON=JSON.parse(receivedMessage)
        console.log(receivedJSON);
        if(receivedJSON.deviceId==this.currentDevice && receivedJSON.messageType=="data" ){
          console.log("OK!")
          this.deviceData=receivedJSON;
          console.log(this.deviceData);
          console.log(this.deviceData.portStatus);
          this.changeValues()
        }
      }

    });
}
  onSelectDeviceChange(){
    for(var i = 0; i < this.deviceInfoArray.length; i++)
    {
      if(this.deviceInfoArray[i].deviceId === this.currentDevice)
      {
        this.deviceInfo = this.deviceInfoArray[i];
      }
    }
    console.log(this.currentDevice);
    console.log(this.deviceInfo);
  }
  onGetMessages(){
    this.iotService.getDeviceMessage(this.currentDevice).subscribe(resp=>{
      console.log(resp)
      this.deviceData=resp;
      this.changeValues()
    })
  }
  changeValues()
  {
    console.log("Zmiana parametrów...")
    this.deviceData.portStatus.forEach(port => {
      let portInfo = this.deviceInfo.portAttributes.find(i => i.id === port.id);
      portInfo.value=port.value;
      if(portInfo.valueType=="bool"){
        (document.getElementById(String(port.id)) as HTMLInputElement).checked=this.convertToBoolean(port.value);
      }
      else{
      (document.getElementById(port.id) as HTMLInputElement).value=port.value;
    }
    });
  }
  onTest(){
    (document.getElementById('0') as HTMLInputElement).checked=this.convertToBoolean("false")
//     this.deviceInfo.portAttributes[0].value=7;
//     (document.getElementById('1') as HTMLInputElement).value = "7";
// console.log(this.deviceInfo.portAttributes);
// console.log(this.deviceInfo.portAttributes[0]);
// console.log(this.deviceInfo.portAttributes[0].value);

  }
  onSend(){
    //console.log("onSend()");
    let portAttibutesCount=this.deviceInfo.portAttributes.length;
    console.log(portAttibutesCount);
    this.deviceInfo.portAttributes.forEach(port => {
      var portValue: any;
      if(port.valueType=='bool'){
        portValue = ((document.getElementById(port.id) as HTMLInputElement).checked);
      }
      else{
        portValue = ((document.getElementById(port.id) as HTMLInputElement).value);
      }
      this.iotService.SendC2DCommand(this.currentDevice,port.name,"Value",portValue).subscribe(resp=>console.log(resp));
      console.log(portValue);
      var C2DMessage: any={
        "Name": port.name,
        "Parameters":{"Value":portValue}
      }
      console.log(C2DMessage);
    });
    //var id = this.deviceInfo.portAttributes[0].id
    //console.log(id);
    //var   num1= ((document.getElementById(id) as HTMLInputElement).value);
    //var   num2= ((document.getElementById("num2") as HTMLInputElement).value);
    //console.log(num1);
    //var C2DMessage: any={
    //  "Name": id,
    //  "Parameters":{"Value":num1}
    //}
    //console.log(C2DMessage)
    //this.iotService.SendC2DCommand(this.currentDevice,this.deviceInfo.portAttributes[0].name,"test","2.3");

  }

}
