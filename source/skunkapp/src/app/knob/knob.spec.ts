import { TestBed, ComponentFixture } from '@angular/core/testing';
import { KnobComponent, KnobModule } from './knob';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';

describe('Knob', () => {
    let knob: KnobComponent;
    let fixture: ComponentFixture<KnobComponent>;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [NoopAnimationsModule, KnobModule]
        });

        fixture = TestBed.createComponent(KnobComponent);
        knob = fixture.componentInstance;
    });

    it('should display by default', () => {
        fixture.detectChanges();

        const knobEl = fixture.debugElement.query(By.css('.sk-knob'));
        expect(knobEl.nativeElement).toBeTruthy();
    });
});
