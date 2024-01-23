import { Routes } from '@angular/router';
import { LandingComponent } from './landing/landing/landing.component';
import { HomeComponent } from './home/home.component';
import { NotFoundComponent } from './not-found/not-found.component';

export const routes: Routes = [
    { path: 'home', component: HomeComponent, },
    { path: '', component: LandingComponent },
    { path: '**', component: NotFoundComponent }
];
