import { AfterViewInit, Component } from '@angular/core';
import { BackendService } from '../../backend/backend.service';

@Component({
  selector: 'sk-landing',
  standalone: true,
  imports: [],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss'
})
export class LandingComponent implements AfterViewInit {
  constructor(private readonly backend: BackendService) { }
  async ngAfterViewInit(): Promise<undefined> {
    await this.backend.connect();

    await this.backend.ping();
  }
}
