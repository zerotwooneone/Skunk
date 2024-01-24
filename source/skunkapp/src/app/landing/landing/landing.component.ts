import { AfterViewInit, Component } from '@angular/core';
import { BackendService } from '../../backend/backend.service';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'sk-landing',
  standalone: true,
  imports: [],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss'
})
export class LandingComponent implements AfterViewInit {
  private _failedToConnect?: boolean;
  get failedToConnect(): boolean {
    return !!this._failedToConnect;
  }
  constructor(
    private readonly backend: BackendService,
    private readonly router: Router) { }
  async ngAfterViewInit(): Promise<undefined> {
    if (!await this.backend.connect()) {
      this._failedToConnect = true;
      if (environment.isDevelopment) {
        //todo: show dev simulation menu
      }
      return;
    }
    this._failedToConnect = false;

    if (!await this.backend.ping()) {
      return;
    }

    this.router.navigate(['/home']);
  }
}
