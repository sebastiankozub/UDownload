import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { FormsModule } from '@angular/forms'; 

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FileQualityPickerComponent } from './file-quality-picker/file-quality-picker.component';
import { CheckboxListComponent } from './file-quality-picker/checkbox-list/checkbox-list.component';
import { HttpClientModule, provideHttpClient } from '@angular/common/http';
import { HomeComponent } from './home/home.component';
import { SearchStreamComponent } from './search-stream/search-stream.component';
import { VideoDisplayComponent } from './video-display/video-display.component';
import { DownloadsListComponent } from './downloads-list/downloads-list.component';



@NgModule({
  declarations: [
    AppComponent,
    FileQualityPickerComponent,
    HomeComponent,
    SearchStreamComponent,
    VideoDisplayComponent,
    DownloadsListComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    CheckboxListComponent
],
  providers: [
  provideHttpClient()

  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
