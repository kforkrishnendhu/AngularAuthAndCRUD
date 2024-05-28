import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  

  private baseUrl:string='https://localhost:7080/api/User/';

  constructor(private http: HttpClient) { }

  getUsers(){
    return this.http.get<any>(this.baseUrl);
  }

  getUserDetails(fullName: string){
    // return this.http.get<any>(`${this.baseUrl}user`,fullName);
    const params = new HttpParams().set('username', fullName);
    console.log(params);
    return this.http.get<any>(`${this.baseUrl}user`, { params });
  }

  updateUser(id: number,userObj: any) {
    return this.http.put(`${this.baseUrl}UpdateUser/${id}`, userObj)
  }

  deleteUser(id: any) {
    console.log(id+"check id");
    return this.http.delete(`${this.baseUrl}DeleteUser/${id}`)
  }

  uploadImage(formData:FormData){
    return this.http.post<any>(`${this.baseUrl}upload`, formData)
  }

}
