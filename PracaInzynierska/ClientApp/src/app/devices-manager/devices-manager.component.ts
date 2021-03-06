import { Component, OnInit } from '@angular/core';
import { IotService } from '../services/iot.service';

@Component({
  selector: 'app-devices-manager',
  templateUrl: './devices-manager.component.html',
  styleUrls: ['./devices-manager.component.css']
})
export class DevicesManagerComponent implements OnInit {
  devices;
  currentDevice;
  primaryKey;
  connectionString;
  AddDeviceLayout=false;
  secondaryKey;
  deviceId:any;
  constructor(private iotService: IotService) {
    iotService.getDevicesNames().subscribe(response=>{this.devices=response;})
   }

  ngOnInit() {
  }

  onSelectChange(){
    console.log(this.currentDevice);
    if(this.currentDevice!=0){
    this.iotService.getDeviceConnectionString(this.currentDevice).subscribe((response:any)=>{
      this.primaryKey=response.primaryKey;
      this.secondaryKey=response.secondaryKey;
      this.connectionString="HostName=PracaInzynierkska.azure-devices.net;DeviceId="+this.currentDevice+";SharedAccessKey="+this.primaryKey;
    });
    }
  }
  onAddDeviceLayout(){
    this.AddDeviceLayout=true;
  }
  onAddDevice(){
    this.AddDeviceLayout=false;
    console.log(this.deviceId)
    if(this.deviceId){
      this.iotService.CreateNewDevice(this.deviceId).subscribe(res=>console.log(res))
    }

  }

}
