import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { BoletoService } from './services/boleto.service';
import { BoletoDto } from './models/boleto.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private boletoService = inject(BoletoService);

  protected boletos = signal<BoletoDto[]>([]);
  protected cargando = signal<boolean>(true);

  ngOnInit(): void {
    this.cargarBoletos();
  }

  cargarBoletos() {
    this.boletoService.obtenerBoletos().subscribe({
      next: (datos) => {
        this.boletos.set(datos);
        this.cargando.set(false);
      },
      error: (err) => {
        console.error('Error al conectar con la API de C#:', err);
        this.cargando.set(false);
      }
    });
  }
}