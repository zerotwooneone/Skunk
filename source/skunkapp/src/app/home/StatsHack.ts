export class StatsHack {
  constructor(
    private readonly _maxSampleCount: number,
    private _max: number | undefined = undefined,
    private _sampleTotal: number = 0,
    private _sampleCount: number = 0,
    private _avg: number = 0) { }
  get maxSampleCount(): number {
    return this._maxSampleCount;
  }
  get max(): number | undefined {
    return this._max;
  }
  get sampleTotal(): number {
    return this._sampleTotal;
  }
  get sampleCount(): number {
    return this._sampleCount;
  }
  get avg(): number {
    return this._avg;
  }
  addSample(sample: number): void {
    if ((typeof this._max == 'undefined') || sample > this._max) {
      this._max = sample;
    }
    if (this._sampleCount >= this._maxSampleCount) {
      this._avg = (this._sampleTotal + sample) / (this._sampleCount + 1);
      this._sampleTotal = this._avg * this._maxSampleCount;
    } else {
      this._sampleCount++;
      this._sampleTotal += sample;
      this._avg = this._sampleTotal / this._sampleCount;
    }
  }
}
