import { Router, Routes, UrlTree } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { NgToastService } from 'ng-angular-popup';

export const routes: Routes = [
    {
        path:'login',
        component:LoginComponent
    },
    {
        path:'signup',
        component:SignupComponent
    },
    {
        path:'dashboard',
        component:DashboardComponent,
        canActivate: [isAuthenticated]
    }
];

function isAuthenticated(): boolean | UrlTree {
    const authService = inject(AuthService);
    const toast =inject(NgToastService);
  
    if (authService.isLoggedIn()) {

        return true;
    }
    toast.error({detail:"ERROR",summary:"Please login first!"});
    return inject(Router).createUrlTree(['/login']);
  };
