import { NgModule, Optional, SkipSelf  } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
//import { AuthService } from '../../svc/auth.service';
import { AuthComponent } from './auth.component';
//import { AuthGuard } from '../../svc/auth-guard.service';
import { AuthPendingComponent } from './auth-pending.component';
import { AuthFailedComponent } from './auth-failed.component';
import { AuthTestComponent } from './auth-test.component';
import { LoginComponent } from './login.component';
//import { SettingsService } from './settings.service';

@NgModule({
    declarations: [
        AuthComponent,
        AuthPendingComponent,
        AuthFailedComponent,
        AuthTestComponent,
        LoginComponent
    ],
    providers: [
        // AuthService,
        // AuthGuard,
        // SettingsService
    ],
    imports: [
        SharedModule,
        RouterModule.forChild([
            {
                path: 'auth',
                component: AuthComponent,
                children: [
                    { path: 'nope', component: AuthFailedComponent },
                    { path: 'test', component: AuthTestComponent },
                    { path: 'login', component: LoginComponent },
                    { path: 'oidc', component: AuthPendingComponent },
                    { path: '', redirectTo: 'login', pathMatch: 'full'}
                ]
            }
        ])
    ]
})
export class AuthModule {

}