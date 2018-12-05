import { IotService } from './../services/iot.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dynamic-interface',
  templateUrl: './dynamic-interface.component.html',
  styleUrls: ['./dynamic-interface.component.css'],
  providers: [ IotService ]
})
export class DynamicInterfaceComponent implements OnInit {
  deviceInfo;

  constructor(private iotService: IotService) {
    iotService.getDeviceInfo().subscribe(response=>{
      this.deviceInfo=response;
      console.log(this.deviceInfo);
    })
    
   }

  ngOnInit() {
  }

}
