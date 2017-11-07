import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { TopoRoutingModule } from './topo-routing.module';
import { TopologyService } from '../../api/topology.service';
import { VmModule } from '../vm/vm.module';
import { DocumentModule } from '../document/document.module';
import { ApiModule } from '../../api/api.module';
import { ChatModule } from '../chat/chat.module';
import { TemplateEditorComponent } from './template-editor.component';

@NgModule({
    imports: [
        SharedModule,
        ApiModule,
        VmModule,
        TopoRoutingModule,
        DocumentModule,
        ChatModule
    ],
    declarations: [ TopoRoutingModule.components ],
    exports: [ TemplateEditorComponent ],
    providers: [  ]
})
export class WorkspaceModule { }