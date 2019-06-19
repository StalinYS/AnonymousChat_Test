import { AuthGuard } from './auth/auth.guard';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UserComponent } from './user/user.component';

import { RegistrationComponent } from './user/registration/registration.component';
import { LoginComponent } from './user/login/login.component';
import { HomeComponent } from './home/home.component';
import { ProfileComponent } from './profile/profile.component';
import { SearchComponent } from './home/search/search.component';
import { ChatsComponent } from './home/chats/chats.component';
import { DialogComponent } from './home/chats/dialog/dialog.component';

const routes: Routes = [
  {path:'',redirectTo:'/user/login',pathMatch:'full'},
  {
    path: 'user', component: UserComponent,
    children: [
      { path: 'registration', component: RegistrationComponent },
      { path: 'login', component: LoginComponent }
    ]
  },
  {
    path:'home',component: HomeComponent ,canActivate:[AuthGuard],
    children: [
      {path: 'search', component: SearchComponent},
      {path: 'chats', component: ChatsComponent,
        children: [
          {path: 'dialog', component: DialogComponent}
        ]
      }
    ]
  },
  {path:'profile',component:ProfileComponent,canActivate:[AuthGuard]}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
