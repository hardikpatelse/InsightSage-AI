import { Component, Input } from '@angular/core'
import { UserService } from '../../services/user.service'
import { User } from '../../entities/user'
import { CommonModule } from '@angular/common'

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header {
  @Input() user: User | null = null;

  showUserMenu = false;

  constructor(private userService: UserService) {

  }

  logout(): void {
    this.userService.logout()
  }

  toggleUserMenu() {
    this.showUserMenu = !this.showUserMenu
  }
}
