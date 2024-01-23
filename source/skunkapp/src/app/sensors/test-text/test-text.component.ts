import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Observable } from 'rxjs';

@Component({
  selector: 'sk-test-text',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './test-text.component.html',
  styleUrl: './test-text.component.scss'
})
export class TestTextComponent {
  @Input() value: number = 0;
}
