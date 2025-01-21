import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { FormsModule } from '@angular/forms'; 

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FileQualityPickerComponent } from './file-quality-picker/file-quality-picker.component';
import { CheckboxListComponent } from './file-quality-picker/checkbox-list/checkbox-list.component';
import { HttpClientModule, provideHttpClient } from '@angular/common/http';



@NgModule({
  declarations: [
    AppComponent,
    FileQualityPickerComponent,
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
