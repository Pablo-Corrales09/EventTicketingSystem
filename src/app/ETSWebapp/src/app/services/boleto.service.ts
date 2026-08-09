import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BoletoDto, BoletoCreacionDto } from '../models/boleto.model';

@Injectable({
  providedIn: 'root'
})
export class BoletoService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5123/api/boletos';

  obtenerBoletos(): Observable<BoletoDto[]> {
    return this.http.get<BoletoDto[]>(this.apiUrl);
  }

  comprarBoleto(dto: BoletoCreacionDto): Observable<BoletoDto> {
    return this.http.post<BoletoDto>(`${this.apiUrl}/comprar`, dto);
  }
}