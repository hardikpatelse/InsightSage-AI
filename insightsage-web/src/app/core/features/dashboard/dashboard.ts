import { Component } from '@angular/core'

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard {
  tokenCost: number = 0
  userCount: number = 0
  chatCount: number = 0
}
