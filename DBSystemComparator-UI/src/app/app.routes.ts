import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { DatabasesComponent, } from './components/databases/databases';

export const routes: Routes = [
    {
        path: '',
        component: HomeComponent,
        pathMatch: 'full'
    },
    {
        path: 'databases',
        component: DatabasesComponent
    },
    {
        path: '**',
        redirectTo: ''
    }
];
