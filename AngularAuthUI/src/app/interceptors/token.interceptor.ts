import { HttpErrorResponse, HttpHandler, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { NgToastService } from 'ng-angular-popup';

export const tokenInterceptor: HttpInterceptorFn = (req, next) => {
  const auth=inject(AuthService);
  const myToken=auth.getToken();
  const toast=inject(NgToastService);
  
  const router=inject(Router);
    const authreq=req.clone({
      headers:req.headers.set('Authorization',`Bearer ${myToken}`)
    })
    return next(authreq).pipe(
      catchError((error: any) => {
        if (error.status === 401) { 
          toast.warning({detail:"WARNING",summary:"Token expired, login again"});
          router.navigate(['login']);
        
        } 
          return throwError(()=>{
            new Error("Some other error occured");
          }); // Re-throw other errors for handling elsewhere
        
      })
    );
 
}


// function handleUnAuthorizedError(req:HttpRequest<any>,next: HttpHandlerFn, auth: AuthService,router:Router){
   
//   let tokenApiModel=new TokenApiModel();
//   tokenApiModel.accessToken=auth.getToken()!;
//   tokenApiModel.refreshToken=auth.getRefreshToken()!;
//   return  auth.renewToken(tokenApiModel)
//   .pipe(
//     switchMap((data:TokenApiModel)=>{
//     auth.storeRefreshToken(data.refreshToken);
//     auth.storeToken(data.accessToken);
//     const authreq=req.clone({
//       headers:req.headers.set('Authorization',`Bearer ${data.accessToken}`)
//     })
//     return next(authreq);
//   }),
//     catchError((err)=>{
//       console.log(err);
      
// return throwError(()=>{
// alert("Token Expired")
// router.navigate(['login']);
// })
//     })

//   )
// }