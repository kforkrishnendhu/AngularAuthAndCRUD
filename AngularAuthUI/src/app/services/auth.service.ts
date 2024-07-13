import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenApiModel } from '../models/token-api.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl: string = "http://localhost:30829/api/User/";
  private userPayload:any;

  constructor(private http: HttpClient, private router:Router) {
    this.userPayload=this.decodedToken();
  }

  signUp(userObj: any) {
    return this.http.post<any>(`${this.baseUrl}register`, userObj)
  }

  login(loginObj: any) {
    return this.http.post<any>(`${this.baseUrl}authenticate`, loginObj)
  }

  signout(){
    localStorage.clear();
    this.router.navigate(['login']);
  }

  storeToken(tokenValue: string){
    localStorage.setItem('token',tokenValue);
  }

  storeRefreshToken(tokenValue: string){
    localStorage.setItem('refreshToken',tokenValue);
  }

  getToken(){
    return localStorage.getItem('token');
  }

  getRefreshToken(){
    return localStorage.getItem('refreshToken');
  }

  isLoggedIn():boolean{
    return !!localStorage.getItem('token');  //if there is token that means the user is logged in. So allow him to access
  }

  decodedToken(){
    const jwtHelper=new JwtHelperService();
    const token=this.getToken()!;
    return jwtHelper.decodeToken(token);
  }

  getfullNameFromToken(){
    if(this.userPayload)
      return this.userPayload.name;
  }


  getRoleFromToken(){
    if(this.userPayload)
      return this.userPayload.role;
  }

  renewToken(tokenApi: TokenApiModel){
    return this.http.post<any>(`${this.baseUrl}refresh`,tokenApi);
  }


}
