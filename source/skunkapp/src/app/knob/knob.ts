import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, Output, ViewEncapsulation, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

export const KNOB_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => KnobComponent),
    multi: true
};
/**
 * Knob is a form component to define number inputs with a dial.
 * @group Components
 */
@Component({
    selector: 'sk-knob',
    imports: [CommonModule],
    template: `
        <div [ngClass]="containerClass()" [class]="styleClass??''" [ngStyle]="style" [attr.data-pc-name]="'knob'" [attr.data-pc-section]="'root'">
            <svg
                viewBox="0 0 100 100"
                role="slider"
                [style.width]="size + 'px'"
                [style.height]="size + 'px'"
                [attr.aria-valuemin]="min"
                [attr.aria-valuemax]="max"
                [attr.aria-valuenow]="_value"
                [attr.aria-labelledby]="ariaLabelledBy"
                [attr.aria-label]="ariaLabel"
                [attr.tabindex]="-1"
                [attr.data-pc-section]="'svg'"
            >
                <circle [attr.cx]="marker1X()" [attr.cy]="marker1Y()" [attr.r]="marker1Radius()" [attr.fill]="marker1Color" class="sk-knob-marker1"></circle>
                <circle [attr.cx]="marker2X()" [attr.cy]="marker2Y()" [attr.r]="marker2Radius()" [attr.fill]="marker2Color" class="sk-knob-marker2"></circle>
                <path [attr.d]="rangePath()" [attr.stroke-width]="strokeWidth" [attr.stroke]="rangeColor" class="sk-knob-range"></path>
                <path [attr.d]="valuePath()" [attr.stroke-width]="strokeWidth" [attr.stroke]="valueColor" class="sk-knob-value"></path>
                <text *ngIf="showValue" [attr.x]="50" [attr.y]="57" text-anchor="middle" [attr.fill]="textColor" class="sk-knob-text" [attr.name]="name">{{ valueToDisplay() }}</text>
            </svg>
        </div>
    `,
    providers: [KNOB_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush,
    encapsulation: ViewEncapsulation.None,
    standalone: true,
    styleUrls: ['./knob.css'],
    host: {
        class: 'p-element'
    }
})
export class KnobComponent {
    /**
     * Style class of the component.
     * @group Props
     */
    @Input() styleClass: string | undefined;
    /**
     * Inline style of the component.
     * @group Props
     */
    @Input() style: { [klass: string]: any } | null | undefined;
    /**
     * Defines a string that labels the input for accessibility.
     * @group Props
     */
    @Input() ariaLabel: string | undefined;
    /**
     * Specifies one or more IDs in the DOM that labels the input field.
     * @group Props
     */
    @Input() ariaLabelledBy: string | undefined;
    /**
     * Index of the element in tabbing order.
     * @group Props
     */
    @Input() tabindex: number = 0;
    /**
     * Background of the value.
     * @group Props
     */
    @Input() valueColor: string = 'var(--primary-color, Black)';
    /**
     * Background color of the range.
     * @group Props
     */
    @Input() rangeColor: string = 'var(--surface-border, LightGray)';
    /**
     * Background color of the marker1.
     * @group Props
     */
    @Input() marker1Color: string = 'var(--marker1-color, Blue)';
    /**
     * Background color of the marker2.
     * @group Props
     */
    @Input() marker2Color: string = 'var(--marker2-color, Red)';
    /**
     * Color of the value text.
     * @group Props
     */
    @Input() textColor: string = 'var(--text-color-secondary, Black)';
    /**
     * Template string of the value.
     * @group Props
     */
    @Input() valueTemplate: string = '{value}';
    /**
     * Name of the input element.
     * @group Props
     */
    @Input() name: string | undefined;
    /**
     * Size of the component in pixels.
     * @group Props
     */
    @Input() size: number = 100;
    /**
     * Step factor to increment/decrement the value.
     * @group Props
     */
    @Input() step: number = 1;
    /**
     * Mininum boundary value.
     * @group Props
     */
    @Input() min: number = 0;
    /**
     * Maximum boundary value.
     * @group Props
     */
    @Input() max: number = 100;
    /**
     * Width of the knob stroke.
     * @group Props
     */
    @Input() strokeWidth: number = 14;
    /**
     * When present, it specifies that the component should be disabled.
     * @group Props
     */
    @Input() disabled: boolean | undefined;
    /**
     * Whether the show the value inside the knob.
     * @group Props
     */
    @Input() showValue: boolean = true;
    /**
     * Callback to invoke on value change.
     * @param {number} value - New value.
     * @group Emits
     */
    @Output() onChange: EventEmitter<number> = new EventEmitter<number>();

    radius: number = 40;

    midX: number = 50;

    midY: number = 50;

    minRadians: number = (4 * Math.PI) / 3;

    maxRadians: number = -Math.PI / 3;

    value: number = 0;

    /**
     * The location of marker1 between min and max
     * @group Props
     */
    @Input() marker1: number|undefined|null = undefined;

    /**
     * The location of marker2 between min and max
     * @group Props
     */
    @Input() marker2: number | undefined|null = undefined;

    onModelChange: Function = () => {};

    onModelTouched: Function = () => {};

    constructor(private cd: ChangeDetectorRef) {}

    mapRange(x: number, inMin: number, inMax: number, outMin: number, outMax: number):number {
        return ((x - inMin) * (outMax - outMin)) / (inMax - inMin) + outMin;
    }

    writeValue(value: any): void {
        this.value = value;
        this.cd.markForCheck();
    }

    registerOnChange(fn: Function): void {
        this.onModelChange = fn;
    }

    registerOnTouched(fn: Function): void {
        this.onModelTouched = fn;
    }

    setDisabledState(val: boolean): void {
        this.disabled = val;
        this.cd.markForCheck();
    }

    containerClass() {
        return {
            'sk-knob p-component': true,
            'p-disabled': this.disabled
        };
    }

    rangePath() {
        let maxX: number|string = this.maxX();
        if(Number.isNaN(maxX)){
            maxX = "";
        }
        let maxY: number | string = this.maxY();
        if (Number.isNaN(maxY)) {
            maxY = "";
        }
        return `M ${this.minX()} ${this.minY()} A ${this.radius} ${this.radius} 0 1 1 ${maxX} ${maxY}`;
    }

    valuePath() {
        return `M ${this.zeroX()} ${this.zeroY()} A ${this.radius} ${this.radius} 0 ${this.largeArc()} ${this.sweep()} ${this.valueX()} ${this.valueY()}`;
    }    

    zeroRadians() {
        if (this.min > 0 && this.max > 0) return this.mapRange(this.min, this.min, this.max, this.minRadians, this.maxRadians);
        else return this.mapRange(0, this.min, this.max, this.minRadians, this.maxRadians);
    }

    valueRadians():number {
        return (typeof this._value == 'number' && !Number.isNaN(this._value)) 
            ? this.mapRange(this.clamp(this._value,this.min,this.max), this.min, this.max, this.minRadians, this.maxRadians)
            : 0;
    }

    marker1Radians():number|undefined {
        return (typeof this._marker1Value == 'number' && !Number.isNaN(this._marker1Value)) 
            ? this.mapRange(this.clamp(this._marker1Value, this.min,this.max), this.min, this.max, this.minRadians, this.maxRadians)
            : undefined;
    }

    marker2Radians():number|undefined {
        return (typeof this._marker2Value == 'number' && !Number.isNaN(this._marker2Value))
            ? this.mapRange(this.clamp(this._marker2Value, this.min, this.max), this.min, this.max, this.minRadians, this.maxRadians)
            : undefined;
    }

    private clamp(value: number, min: number, max: number):number{
        return Math.min(Math.max(min, value),max);
    }

    minX() {
        return this.midX + Math.cos(this.minRadians) * this.radius;
    }

    minY() {
        return this.midY - Math.sin(this.minRadians) * this.radius;
    }

    maxX() {
        return this.midX + Math.cos(this.maxRadians) * this.radius;
    }

    maxY() {
        return this.midY - Math.sin(this.maxRadians) * this.radius;
    }

    zeroX() {
        return this.midX + Math.cos(this.zeroRadians()) * this.radius;
    }

    zeroY() {
        return this.midY - Math.sin(this.zeroRadians()) * this.radius;
    }

    valueX() {
        return this.midX + Math.cos(this.valueRadians()) * this.radius;
    }

    valueY() {
        return this.midY - Math.sin(this.valueRadians()) * this.radius;
    }

    marker1X() {
        const rad = this.marker1Radians();
        return (typeof rad == 'number' && !Number.isNaN(rad)) 
            ? this.midX + Math.cos(rad) * this.radius
            : undefined;
    }

    marker1Y() {
        const rad = this.marker1Radians();
        return (typeof rad == 'number' && !Number.isNaN(rad))
            ? this.midY - Math.sin(rad) * this.radius
            : undefined;
    }

    marker1Radius(){
        return (typeof this.marker1 == 'number') 
            ? (3*this.strokeWidth)/4
            : 0;
    }

    marker2X() {
        const rad = this.marker2Radians();
        return (typeof rad == 'number' && !Number.isNaN(rad))
            ? this.midX + Math.cos(rad) * this.radius
            : undefined;
    }

    marker2Y() {
        const rad = this.marker2Radians();
        return (typeof rad == 'number' && !Number.isNaN(rad))
            ? this.midY - Math.sin(rad) * this.radius
            : undefined;
    }

    marker2Radius() {
        return (typeof this.marker2 == 'number')
            ? (3 * this.strokeWidth) / 4
            : 0;
    }

    largeArc() {
        return Math.abs(this.zeroRadians() - this.valueRadians()) < Math.PI ? 0 : 1;
    }

    sweep() {
        return this.valueRadians() > this.zeroRadians() ? 0 : 1;
    }

    valueToDisplay() {
        return this.valueTemplate.replace('{value}', this._value.toString());
    }

    get _value(): number {
        return this.value != null ? this.value : this.min;
    }

    get _marker1Value(): number|undefined {
        return (typeof this.marker1 == 'number') ? this.marker1 : undefined;
    }

    get _marker2Value(): number | undefined {
        return (typeof this.marker2 == 'number') ? this.marker2 : undefined;
    }
}