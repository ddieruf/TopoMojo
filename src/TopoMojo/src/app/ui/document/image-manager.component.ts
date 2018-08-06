import { Component, OnInit, Input, Inject } from '@angular/core';
import { HttpEvent, HttpEventType, HttpResponse } from '@angular/common/http';
import { DOCUMENT } from '@angular/platform-browser';
import { DocumentService } from '../../api/document.service';
import { ImageFile } from '../../api/gen/models';

@Component({
    selector: 'app-image-manager',
    templateUrl: 'image-manager.component.html',
    styleUrls: ['image-manager.component.css']
})
export class ImageManagerComponent implements OnInit {

    @Input() id: string;
    images: ImageFile[] = [];
    queuedFiles: any[] = [];
    clipboardText = '';

    constructor(
        private service: DocumentService,
        @Inject(DOCUMENT) private dom: Document
    ) { }

    ngOnInit() {
        this.list();
    }

    fileSelectorChanged(e) {
        // console.log(e.srcElement.files);
        this.queuedFiles = [];
        for (let i = 0; i < e.srcElement.files.length; i++) {
            const file = e.srcElement.files[i];
            this.queuedFiles.push({
                key: this.id + '-' + file.name,
                name: file.name,
                file: file,
                progress: -1
            });
        }
        // this.queuedFiles = [ e.srcElement.files[0] ];
    }

    filesQueued() {
        return this.queuedFiles.length > 0;
    }

    dequeueFile(qf) {
        this.queuedFiles.splice(this.queuedFiles.indexOf(qf), 1);
    }

    upload() {
        for (let i = 0; i < this.queuedFiles.length; i++) {
            this.uploadFile(this.queuedFiles[i]);
        }
        // this.queuedFiles = [];
    }

    uploadFile(qf) {
        this.service.uploadImage(this.id, qf.file)
        .subscribe(
            (event) => {
                if (event.type === HttpEventType.UploadProgress) {
                    qf.progress = Math.round(100 * event.loaded / event.total);
                } else if (event instanceof HttpResponse) {
                    const imagefile = event.body;
                    const found = this.images.filter(
                        (e) => {
                            return e.filename === imagefile.filename;
                        }
                    );
                    if (found.length === 0) {
                        this.images.push(imagefile);
                    }
                }
            },
            (err) => { },
            () => this.queuedFiles.splice(this.queuedFiles.indexOf(qf), 1)
        );
    }



    list() {
        this.service.getImages(this.id)
        .subscribe((result: ImageFile[]) => {
            this.images = result;
        });
    }

    copyToClipboard(text: string): void {
        const el = this.dom.getElementById('image-clipboard') as HTMLTextAreaElement;
        // console.log(el);
        el.value = text;
        el.select();
        this.dom.execCommand('copy');
    }

    genMd(img: ImageFile): void {
        this.copyToClipboard(`![${img.filename}](${this.imagePath(img)} =480x*)`);
    }

    delete(img: ImageFile) {
        this.service.deleteImage(this.id, img.filename)
        .subscribe((result: ImageFile) => {
            this.removeImage(result.filename);
        });
    }

    private removeImage(fn) {
        for (let i = 0; i < this.images.length; i++) {
            if (this.images[i].filename === fn) {
                this.images.splice(i, 1);
                break;
            }
        }
    }
}
