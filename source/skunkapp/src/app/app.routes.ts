import { Routes } from '@angular/router';
import { LandingComponent } from './landing/landing/landing.component';
import { AppComponent } from './app.component';

export const routes: Routes = [
    { path: 'home', component: AppComponent },
    { path: '', component: LandingComponent }
];
