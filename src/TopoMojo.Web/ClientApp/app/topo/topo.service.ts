import { AuthHttp } from '../auth/auth-http';
import { Injectable } from '@angular/core';

@Injectable()
export class TopoService {

    constructor(
        private http: AuthHttp
        ) {}

    // public loadTopo(id: number) {
    //     return this.http.get('/api/topology/load/'+id);
    // }

    public ipCheck() {
        return this.http.get("/api/topology/ipcheck");
    }

    public createTopo(Topo) {
        return this.http.post('/api/topology/create', Topo);
    }

    public loadTopo(id) {
        return this.http.get('/api/topology/load/' + id);
    }

    public listtopo(search) {
        return this.http.post('/api/topology/list', search);
    }

    public listmine(search) {
        return this.http.post('/api/topology/mine', search);
    }

    public updateTopo(topo) {
        return this.http.post('/api/topology/update', topo);
    }

    public deleteTopo(topo) {
        return this.http.delete('/api/topology/delete/' + topo.id);
    }

    public listMembers(id: number) {
        return this.http.get('/api/topology/members/' + id);
    }

    public addMembers(id: number, emails: string) {
        return this.http.post('/api/account/addtopouser', { topoId: id, emails: emails });
    }

    public enlist(code: string) {
        return this.http.get('/api/topology/enlist/' + code);
    }

    public delist(id: number, mid: number) {
        return this.http.delete('/api/topology/delist/' + id + "/" + mid);
    }

    public share(id: number) {
        return this.http.get('/api/topology/share/' + id);
    }

    public unshare(id: number) {
        return this.http.get('/api/topology/unshare/' + id);
    }

    public publish(id: number) {
        return this.http.put('/api/topology/publish/' + id, null);
    }

    public unpublish(id: number) {
        return this.http.put('/api/topology/unpublish/' + id, null);
    }

    public removeMember(id: number, personId: number) {
        return this.http.post('/api/account/removetopouser', { topoId: id, personId: personId});
    }

    public createTemplate(template) {
        return this.http.post('/api/template/create', template);
    }

    public listTopoTemplates(id) {
        return this.http.get("/api/topology/templates/"+id);
    }

    public listTemplates(search) {
        return this.http.post('/api/template/list', search);
    }

    public loadTemplate(id) {
        return this.http.get('/api/template/load/'+id);
    }

    public saveTemplate(template) {
        return this.http.post('/api/template/save', template);
    }

    public deleteTemplate(id) {
        return this.http.delete('/api/template/delete/' + id);
    }

    public addTemplate(tref) {
        return this.http.post('/api/topology/addtemplate', tref);
    }

    public updateTemplate(tref) {
        return this.http.post('/api/topology/updatetemplate', tref);
    }

    public removeTemplate(tref) {
        return this.http.delete('/api/template/remove/'+tref.id);
    }

    public cloneTemplate(tref) {
        return this.http.post('/api/topology/clonetemplate/'+tref.id, {});
    }

    public launchInstance(id) {
        return this.http.get('/api/instance/launch/' + id);
    }

    public checkInstance(id) {
        return this.http.get('/api/instance/check/' + id);
    }

    public destroyInstance(id) {
        return this.http.delete("/api/instance/destroy/" + id);
    }

    public activeInstances() {
        return this.http.get("/api/instance/active");
    }

    public saveDoc(id, text) {
        return this.http.post('/api/topology/savedocument/' + id, text);
    }

    public loadDoc(id) {
        return this.http.gettext('/docs/' + id + '.md');
    }

    public loadUrl(url) {
        return this.http.gettext(url);
    }

    public onError(err) {
        this.http.onError(err);
    }
}