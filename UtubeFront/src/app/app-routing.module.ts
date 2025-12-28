import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
//import { AppComponent } from './app.component';
import { FileQualityPickerComponent } from './file-quality-picker/file-quality-picker.component';
import { VideoDisplayComponent } from './video-display/video-display.component';

const routes: Routes = [
  //{ path: '', component: AppComponent },
  { path: 'file-quality-picker', component: FileQualityPickerComponent },
  { path: 'video-display', component: VideoDisplayComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
