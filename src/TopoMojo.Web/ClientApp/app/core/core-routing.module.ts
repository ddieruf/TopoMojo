import { NgModule } from '@angular/core';
import { RouterModule, Routes, ActivatedRouteSnapshot } from '@angular/router';
import { CoreComponent } from './core.component';
import { HomeComponent } from './home.component';
import { NavbarComponent } from './navbar.component';
import { NotFoundComponent } from './notfound.component';
import { AboutPanelComponent } from './about-panel.component';
import { HelpPanelComponent } from './help-panel.component';

const routes: Routes = [
    {
        path: 'home',
        component: CoreComponent,
        children: [
            { path: 'about', component: AboutPanelComponent },
            { path: 'help' , component: HelpPanelComponent },
            { path: 'notfound', component: NotFoundComponent },
            { path: '', component: HomeComponent },
        ]
    }
];

@NgModule({
    imports: [ RouterModule.forChild(routes) ],
    exports: [ RouterModule ]
})
export class CoreRoutingModule {
    static components = [
        CoreComponent,
        HomeComponent,
        NavbarComponent,
        NotFoundComponent,
        AboutPanelComponent,
        HelpPanelComponent
    ]
}