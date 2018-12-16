import { Component, OnInit } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
@Component({
  selector: 'app-signal-rtest',
  templateUrl: './signal-rtest.component.html',
  styleUrls: ['./signal-rtest.component.css']
})
export class SignalRtestComponent implements OnInit {
  private hubConnection: HubConnection;
  nick = '';
  message = '';
  messages: string[] = [];
  constructor() { }

  ngOnInit() {
    this.nick = window.prompt('Your name:', 'John');
    this.hubConnection =     new HubConnectionBuilder().withUrl("http://localhost:5000/iotsignalrchat").build();


    this.hubConnection
      .start()
      .then(() => console.log('Connection started!'))
      .catch(err => console.log('Error while establishing connection :('));
      
      this.hubConnection.on('sendToAll', (nick: string, receivedMessage: string) => {
        const text = `${nick}: ${receivedMessage}`;
        this.messages.push(text);
      });
  }
  public sendMessage(): void {
    this.hubConnection
      .invoke('sendToAll', this.nick, this.message)
      .catch(err => console.error(err));
  }

}
