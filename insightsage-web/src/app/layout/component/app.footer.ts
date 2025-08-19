import { Component } from '@angular/core'

@Component({
    standalone: true,
    selector: 'app-footer',
    template: `<div class="layout-footer">
        InsightSage AI
        <span class="font-semibold">© {{currentYear}}</span>
    </div>`
})
export class AppFooter {
    currentYear: number = new Date().getFullYear();
}
