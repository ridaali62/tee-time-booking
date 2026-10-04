import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export interface Slot {
  id: number;
  startTime: string;
  course: string;
  capacity: number;
  bookedPlayers: number;
  weatherDelay: boolean;
}

export interface Booking {
  id: number;
  memberId: number;
  teeSlotId: number;
  startTime: string;
  players: number;
  guests: number;
  status: string;
}

const API = 'http://localhost:5080';

@Injectable({ providedIn: 'root' })
export class TeeTimeService {
  private http = inject(HttpClient);

  getSlots(date: string) {
    return this.http.get<Slot[]>(`${API}/slots`, { params: { date } });
  }

  book(memberId: number, teeSlotId: number, players: number, guests: number) {
    return this.http.post<Booking>(`${API}/bookings`, { memberId, teeSlotId, players, guests });
  }

  cancel(id: number) {
    return this.http.delete<void>(`${API}/bookings/${id}`);
  }
}
