import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { TemplateService } from '../../api/template.service';
import { NotificationService } from '../../svc/notification.service';
import { Topology, Template, ChangedTemplate } from '../../api/gen/models';

@Component({
    selector: 'template-editor',
    templateUrl: 'template-editor.component.html',
    styleUrls: [ 'template-editor.component.css' ]
})
export class TemplateEditorComponent implements OnInit {
    @Input() template: Template;
    @Input() topo: Topology;
    @Output() onRemoved: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('cloneHelp') cloneHelp: any;
    editing: boolean;
    cloneVisible: boolean;
    vm: any;
    isosVisible: boolean;
    uploadVisible: boolean;
    errors: any[] = [];

    constructor(
        private service : TemplateService,
        private notifier: NotificationService
    ) { }

    ngOnInit() {
    }


    edit() {
        this.editing = !this.editing;
    }

    remove() {
        if ((this.vm) && (this.vm.id))
            return; //don't remove if vm created

        this.service.deleteTemplate(this.template.id)
        .subscribe(
            (result) => {
                if (result) {
                    this.notifier.sendTemplateEvent("TEMPLATE.REMOVED", this.template);
                    this.topo.templates.splice(this.topo.templates.indexOf(this.template), 1);
                }
            },
            (err) => { this.onError(err);}
        );
    }

    save() {
        this.service.putTemplate(this.template as ChangedTemplate).subscribe(
            (result : Template) => {
                this.notifier.sendTemplateEvent("TEMPLATE.UPDATED", result);
            },
            (err) => { this.onError(err);}
        );
    }

    clone() {
        if ((!this.vm) || (this.vm.id))
            return; //don't clone unless empty vm loaded

        this.cloneHelp.toggle();
        this.service.unlinkTemplate(this.template.id)
        .subscribe(
            (result) => {
                let i = this.topo.templates.indexOf(this.template);
                this.topo.templates.splice(i, 1, result);
            },
            (err) => { this.onError(err);}
        );
    }

    toggleClone() {
        this.cloneVisible = !this.cloneVisible;
    }

    vmLoaded(vm) {
        this.vm = vm;
    }

    isoChanged(iso : string) {
        this.template.iso = iso;
        this.save();
        this.isosVisible = false;
        // todo: if vm, change iso
    }

    hasParent() : boolean {
        return !!this.template.parentId;
    }

    onError(err) {
        let text = JSON.parse(err.text());
        this.errors.push(text);
        console.debug(text);
    }

}