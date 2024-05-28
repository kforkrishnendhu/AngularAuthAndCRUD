import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { UserStoreService } from '../../services/user-store.service';
import { Observable } from 'rxjs';
import { EditUserComponent } from "../edit-user/edit-user.component";

@Component({
    selector: 'app-dashboard',
    standalone: true,
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss',
    imports: [CommonModule, EditUserComponent]
})
export class DashboardComponent implements OnInit {

  public users: any = [];
  public fullName: string = "";
  public role: string = "";
  public user:any;
  public ModalTitle = "";
  public emp: any;
  ActivateAddEditComp: boolean = false;

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

  editClick(item: any) {
    this.emp = item;
    this.ModalTitle = "Edit Employee";
    this.ActivateAddEditComp = true;
  }

  deleteClick(item: any) {
    if (confirm('Are you sure? you want to delete?')) {
      this.api.deleteUser(item.id).subscribe({
        next:(res:any)=>
        alert(res.message.toString())
      });
    }
    this.refreshList();
  }
  closeClick() {
    this.ActivateAddEditComp = false;
    this.refreshList();
  }

  refreshList() {
    this.api.getUsers().subscribe(res => {
      this.users = res;
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
