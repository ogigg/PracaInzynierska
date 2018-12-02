import { Component, OnInit } from '@angular/core';
import { IotService } from '../services/iot.service';

@Component({
  selector: 'app-send-c2-d',
  templateUrl: './send-c2-d.component.html',
  styleUrls: ['./send-c2-d.component.css']
})
export class SendC2DComponent implements OnInit {
  message="";
  devices;
  currentDevice;
  constructor(private iotService: IotService) {
    iotService.getDevicesNames().subscribe(response=>{this.devices=response;})
   }

  ngOnInit() {
  }

  onClick(message:string){
    console.log(message)
    this.iotService.sendC2DMessage(message).subscribe(response=>console.log(response))
  }

  onSwitchClick(event){
    console.log(event.target.checked)
    let message=event.target.checked;
    this.iotService.sendC2DBool(message).subscribe(response=>console.log(response))
  }
  onDeviceChange()
  {
    console.log(this.currentDevice);
    //let message=event.target.checked;
    //this.iotService.sendC2DBool(message).subscribe(response=>console.log(response))
  }
}
