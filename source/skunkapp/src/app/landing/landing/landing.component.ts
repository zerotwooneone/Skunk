import { AfterViewInit, Component } from '@angular/core';
import { BackendService } from '../../backend/backend.service';
import { Router } from '@angular/router';

@Component({
  selector: 'sk-landing',
  standalone: true,
  imports: [],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss'
})
export class LandingComponent implements AfterViewInit {
  constructor(
    private readonly backend: BackendService,
    private readonly router: Router) { }
  async ngAfterViewInit(): Promise<undefined> {
    await this.backend.connect();

    await this.backend.ping();

    this.router.navigate(['/home']);
  }
}
