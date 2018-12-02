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

  sendC2DMessage(message: string){
   var body = ('"message": '+message)
   //var body = JSON.stringify(body)
   console.log(body)
    return this.http.post("api/iot/SendC2D/?message="+message, this.httpOptions);
  }
  sendC2DBool(message: string){
   var body = ('"status": '+message)
   console.log(body)
    return this.http.post("api/iot/SendC2DtoNodeMCU/?LED="+message, this.httpOptions);
  }
  getDevicesNames(){
    return this.http.get("https://localhost:44315/api/iot/GetDevicesNames", this.httpOptions);
  }
}
