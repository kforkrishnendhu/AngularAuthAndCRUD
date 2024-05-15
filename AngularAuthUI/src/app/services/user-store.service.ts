import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserStoreService {
private fullName$=new BehaviorSubject<string>(""); 
private role$=new BehaviorSubject<string>("");

  constructor() { }

  public getRoleFromStore(){
    return this.role$.asObservable();
  }

  public setRoleForStore(role:string){
    this.role$.next(role);
  }

  public getfullNameFromStore(): Observable<string> {
    return this.fullName$.asObservable();
  }

  public setfullNameForStore(fullname:string){
    this.fullName$.next(fullname);
  }

}
