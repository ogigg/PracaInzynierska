import { DynamicInterfaceComponent } from './dynamic-interface/dynamic-interface.component';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { Ng5SliderModule } from 'ng5-slider';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { SendC2DComponent } from './send-c2-d/send-c2-d.component';
import { IotService } from './services/iot.service';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import 'hammerjs';
import {MatSliderModule} from '@angular/material/slider';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    SendC2DComponent,
    DynamicInterfaceComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    Ng5SliderModule,
    FormsModule,
    BrowserAnimationsModule,
    MatSliderModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'counter', component: CounterComponent },
      { path: 'fetch-data', component: FetchDataComponent },
      { path: 'SendC2D', component: SendC2DComponent },
      { path: 'DyamicInterface', component: DynamicInterfaceComponent },
    ])
  ],
  //providers: [IotService],
  bootstrap: [AppComponent]
})
export class AppModule { }
