import { IotService } from './../services/iot.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dynamic-interface',
  templateUrl: './dynamic-interface.component.html',
  styleUrls: ['./dynamic-interface.component.css'],
  providers: [ IotService ]
})
export class DynamicInterfaceComponent implements OnInit {
  deviceInfoArray;
  deviceInfo;
  devicesNames;
  currentDevice;
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

  ngOnInit() {
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

}
