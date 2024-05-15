import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { UserStoreService } from '../../services/user-store.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit{
public users:any=[];
public fullName:string="";

constructor(
  private auth:AuthService,
  private api:ApiService,
  private userStore:UserStoreService
  ){}

 ngOnInit(){
  this.api.getUsers().subscribe(res=>{
    this.users=res;
  });

  this.userStore.getfullNameFromStore()
  .subscribe(val=>{
    let fullnameFronToken=this.auth.getfullNameFromToken();
    this.fullName=val || fullnameFronToken;
  });
 }

logout(){
  this.auth.signout();
}

}
