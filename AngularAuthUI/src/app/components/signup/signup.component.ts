import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import ValidateForm from '../../helpers/validateform';
import { AuthService } from '../../services/auth.service';
import { HttpClientModule } from '@angular/common/http';
import { NgToastService } from 'ng-angular-popup';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [RouterModule,ReactiveFormsModule,CommonModule,HttpClientModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.scss'
})
export class SignupComponent implements OnInit {
  type: string = "password";
  isText: boolean = false;
  eyeIcon: string = "fa-eye-slash";
  signUpForm!: FormGroup;

  constructor(
    private fb:FormBuilder,
     private auth :AuthService,
      private router:Router,
    private toast:NgToastService
    ) {}

  ngOnInit(): void {

    this.signUpForm = this.fb.group({
      firstname: ['', Validators.required],
      lastname: ['', Validators.required],
      email: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required]
    })
  }

  hideShowPass() {
    this.isText = !this.isText;
    this.isText ? this.eyeIcon = "fa-eye" : this.eyeIcon = "fa-eye-slash";
    this.isText ? this.type = "text" : this.type = "password";
  }


  onSubmit() {
    if (this.signUpForm.valid) {
      // console.log(this.signUpForm.value);
      //send object to database or any operation
      this.auth.signUp(this.signUpForm.value)
        .subscribe({
          next:(res=>{
            this.toast.success({detail:"SUCCESS",summary:res.message, duration:5000});
            this.signUpForm.reset();
            this.router.navigate(['login']);
          }),
          error:(err=>{
            this.toast.error({detail:"ERROR",summary:err.message, duration:5000});
          })
        })
    }
    else {
      //throw som eerror using toaster and with required fileds
      ValidateForm.validateAllFormFields(this.signUpForm);
      this.toast.error({detail:"ERROR",summary:"Your form is invalid", duration:5000});
    }
  }

}
