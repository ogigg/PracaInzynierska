import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable()
export class IotService {

  constructor(private http: HttpClient) { }

  httpOptions = {
    headers: new HttpHeaders({
      'Content-Type':  'application/json',
      'responseType' : 'xhr'
    })
  };

  sendC2DMessage(message: string,deviceName){
   var body = ('"message": '+message)
   //var body = JSON.stringify(body)
   console.log(body)
    return this.http.post("api/iot/SendC2DOld/?message="+message+"&deviceName="+deviceName, this.httpOptions);
  }

  SendC2DCommand(deviceName: string, functionName: string, parameterName: string, parameterValue: string){
    console.log("api/iot/SendC2DCommand/?deviceName="+deviceName+"&functionName="+functionName+
    "&parameterName="+parameterName+"&parameterValue="+parameterValue);
    console.log("Wyslano!");
    return this.http.post("api/iot/SendC2DCommand/?deviceName="+deviceName+"&functionName="+functionName+
    "&parameterName="+parameterName+"&parameterValue="+parameterValue, this.httpOptions);
  }
  sendC2DBool(message: string){
   var body = ('"status": '+message)
   console.log(body)
    return this.http.post("api/iot/SendC2DtoNodeMCU/?LED="+message, this.httpOptions);
  }
  getDevicesNames(){
    return this.http.get("/api/iot/GetDevicesNames", this.httpOptions);
  }
  getDeviceInfo(){ //Tymczasowe
    return this.http.get("/api/device/IotConfig", this.httpOptions);
  }
  getDeviceConnectionString(deviceId: string){ //Tymczasowe
    return this.http.get("/api/iot/GetDeviceConnectionString?deviceId="+deviceId, this.httpOptions);
  }
  CreateNewDevice(deviceId: string){ //Tymczasowe
    return this.http.get("/api/iot/CreateNewDevice?deviceId="+deviceId, this.httpOptions);
  }
  getDeviceMessage(deviceId: string){
    return this.http.get("/api/device/GetLastDataMessage?deviceId="+deviceId, this.httpOptions);
  }
}
