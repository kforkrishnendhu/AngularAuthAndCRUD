import { Component, Input, OnInit } from '@angular/core';
import { DashboardComponent } from '../dashboard/dashboard.component';
import { ApiService } from '../../services/api.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-edit-user',
  standalone: true,
  imports: [DashboardComponent, CommonModule, FormsModule],
  templateUrl: './edit-user.component.html',
  styleUrl: './edit-user.component.scss'
})
export class EditUserComponent implements OnInit {
  constructor(private api: ApiService) { }
  @Input() emp: any;
  Id = 0;
  FirstName = "";
  LastName = "";
  UserName = "";
  Email = "";
  Password = "";


  ngOnInit(): void {
    this.Id = this.emp.id;
    this.FirstName = this.emp.firstName;
    this.LastName = this.emp.lastName;
    this.UserName = this.emp.userName;
    this.Email = this.emp.email;
    this.Password = this.emp.password;
  }
  updateUser() {
    var val = {
      FirstName: this.FirstName,
      LastName: this.LastName,
      UserName: this.UserName,
      Email: this.Email,
      Password: this.Password
    };

    this.api.updateUser(this.Id,val).subscribe({
      next:(res:any)=>
      alert(res.message.toString())
    });
  }

}
