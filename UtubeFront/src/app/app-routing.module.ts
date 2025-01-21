import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
//import { AppComponent } from './app.component';
import { FileQualityPickerComponent } from './file-quality-picker/file-quality-picker.component';

const routes: Routes = [
  //{ path: '', component: AppComponent },
  { path: 'file-quality-picker', component: FileQualityPickerComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
