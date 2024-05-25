import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { UserStoreService } from '../../services/user-store.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  public users: any = [];
  public fullName: string = "";
  public role: string = "";
  public user:any;

  constructor(
    private auth: AuthService,
    private api: ApiService,
    private userStore: UserStoreService
  ) { }

  ngOnInit() {
    this.api.getUsers().subscribe(res => {
      this.users = res;
    });

    this.userStore.getfullNameFromStore()
      .subscribe(val => {
        let fullnameFronToken = this.auth.getfullNameFromToken();
        this.fullName = val || fullnameFronToken;
      });

    this.userStore.getRoleFromStore()
      .subscribe(val => {
        let roleFromToken = this.auth.getRoleFromToken();
        this.role = val || roleFromToken;
      });

      this.api.getUserDetails(this.fullName)
      .subscribe((val: any)=>{
        this.user=val;
        console.log(this.user);
      });

  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      const formData = new FormData();
      formData.append('file', file);

      // Upload the file to the server
      this.api.uploadImage(formData).subscribe({
        next: (response: { imageUrl: string }) => {
          // Update the user's profile image URL
          if (this.user) {
            this.user.profileImageUrl = response.imageUrl; // Assuming the response contains the image URL
          }
        },
        error: (error: any) => {
          console.error('Error uploading image', error);
        }
      });
      
    }
  }

  logout() {
    this.auth.signout();
  }

}
