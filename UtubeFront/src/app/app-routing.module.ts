import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { FileQualityPickerComponent } from './file-quality-picker/file-quality-picker.component';
import { SearchStreamComponent } from './search-stream/search-stream.component';
import { VideoDisplayComponent } from './video-display/video-display.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'search', component: SearchStreamComponent },
  { path: 'manifest/:videoId', component: FileQualityPickerComponent },
  { path: 'file-quality-picker', component: FileQualityPickerComponent },
  { path: 'video-display', component: VideoDisplayComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
