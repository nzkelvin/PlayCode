import { Component } from '@angular/core';

// Meta data
@Component({
    selector: 'pm-app',
    template: `
        <div>
            <h1>{{pageTitle}}</h1>
            <div>My first component</div>
        </div>
    `
})
export class AppComponent {
    pageTitle: string = 'Acme Product Management'; 
 }
