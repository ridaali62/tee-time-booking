import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Booking, Slot, TeeTimeService } from './tee-time.service';

@Component({
  selector: 'app-root',
  imports: [FormsModule, DatePipe],
  template: `
    <main>
      <h1>Tee-Time Booking</h1>

      <section class="controls">
        <label>Date <input type="date" [(ngModel)]="date" (change)="load()"></label>
        <label>Member
          <select [(ngModel)]="memberId">
            <option [ngValue]="1">Ayesha Khan</option>
            <option [ngValue]="2">Daniel Brooks</option>
            <option [ngValue]="3">Sara Malik</option>
          </select>
        </label>
        <label>Players <input type="number" min="1" max="4" [(ngModel)]="players"></label>
        <label>Guests <input type="number" min="0" max="2" [(ngModel)]="guests"></label>
      </section>

      @if (hasWeatherDelay) {
        <p class="banner" role="alert">Weather delay: some tee times today are delayed. Check the course status before you travel.</p>
      }
      @if (message) { <p class="ok" role="status">{{ message }}</p> }
      @if (error) { <p class="error" role="alert">{{ error }}</p> }

      <table>
        <thead><tr><th>Time</th><th>Booked</th><th></th></tr></thead>
        <tbody>
          @for (slot of slots; track slot.id) {
            <tr [class.delayed]="slot.weatherDelay">
              <td>{{ slot.startTime | date: 'shortTime' }} @if (slot.weatherDelay) { <span class="tag">delayed</span> }</td>
              <td>{{ slot.bookedPlayers }} / {{ slot.capacity }}</td>
              <td><button (click)="book(slot)" [disabled]="slot.bookedPlayers + players > slot.capacity">Book</button></td>
            </tr>
          } @empty {
            <tr><td colspan="3">No tee times on this date.</td></tr>
          }
        </tbody>
      </table>

      <h2>My bookings</h2>
      @for (b of myBookings; track b.id) {
        <p class="booking">
          {{ b.startTime | date: 'EEE d MMM, h:mm a' }}: {{ b.players }} player(s), {{ b.guests }} guest(s), {{ b.status }}
          @if (b.status === 'Active') { <button (click)="cancel(b)">Cancel</button> }
        </p>
      } @empty {
        <p>No bookings made in this session yet.</p>
      }
    </main>
  `,
  styles: `
    main { max-width: 720px; margin: 2rem auto; padding: 0 1rem; }
    h1 { color: #14532d; }
    .controls { display: flex; flex-wrap: wrap; gap: 1rem; margin-bottom: 1rem; }
    label { display: flex; flex-direction: column; font-size: .85rem; gap: .25rem; }
    input, select { padding: .4rem; font-size: 1rem; }
    table { width: 100%; border-collapse: collapse; background: white; }
    th, td { padding: .5rem; border-bottom: 1px solid #e5e7eb; text-align: left; }
    tr.delayed { background: #fef3c7; }
    .tag { font-size: .75rem; background: #f59e0b; color: white; padding: .1rem .4rem; border-radius: .25rem; }
    .banner { background: #fef3c7; border-left: 4px solid #f59e0b; padding: .75rem; }
    .ok { color: #15803d; }
    .error { color: #b91c1c; }
    button { padding: .35rem .8rem; background: #15803d; color: white; border: 0; border-radius: .35rem; cursor: pointer; }
    button:disabled { background: #9ca3af; cursor: not-allowed; }
  `,
})
export class AppComponent implements OnInit {
  private api = inject(TeeTimeService);

  date = new Date().toLocaleDateString('en-CA'); // YYYY-MM-DD in local time
  memberId = 1;
  players = 2;
  guests = 0;
  slots: Slot[] = [];
  myBookings: Booking[] = [];
  message = '';
  error = '';

  get hasWeatherDelay() {
    return this.slots.some(s => s.weatherDelay);
  }

  ngOnInit() {
    this.load();
  }

  load() {
    this.api.getSlots(this.date).subscribe({
      next: slots => (this.slots = slots),
      error: () => (this.error = 'Could not load tee times. Is the API running?'),
    });
  }

  book(slot: Slot) {
    this.clear();
    this.api.book(this.memberId, slot.id, this.players, this.guests).subscribe({
      next: booking => {
        this.myBookings.push(booking);
        this.message = 'Tee time booked.';
        this.load();
      },
      error: (e: HttpErrorResponse) => (this.error = e.error?.error ?? 'Booking failed.'),
    });
  }

  cancel(booking: Booking) {
    this.clear();
    this.api.cancel(booking.id).subscribe({
      next: () => {
        booking.status = 'Cancelled';
        this.message = 'Booking cancelled.';
        this.load();
      },
      error: (e: HttpErrorResponse) => (this.error = e.error?.error ?? 'Cancellation failed.'),
    });
  }

  private clear() {
    this.message = '';
    this.error = '';
  }
}
