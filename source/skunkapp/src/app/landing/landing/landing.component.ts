import { AfterViewInit, Component } from '@angular/core';
import { BackendService } from '../../backend/backend.service';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { filter } from 'rxjs';

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
    if (!await this.backend.connect()) {
      if (environment.isDevelopment) {
        this.backend.connected$.pipe(
          filter(c=>!!c)
        ).subscribe(c=>{
          this.router.navigate(['/home']);
        })
      }
      return;
    }
  
    if (!await this.backend.ping()) {
      return;
    }

    this.router.navigate(['/home']);
  }
}
