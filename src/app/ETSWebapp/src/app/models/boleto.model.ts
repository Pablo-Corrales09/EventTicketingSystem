export interface BoletoDto {
  idBoleto: number;
  numBoleto: string;
  fechaCompra: string;
  numeroFactura: string;
  precio: number;
  nombreEvento: string;
  fechaEvento: string;
  horaEvento: string;
  nombreLocalidad: string;
  nombreUsuario: string;
  apellidoUsuario: string;
  correoUsuario: string;
  telefonoUsuario: string;
}

export interface BoletoCreacionDto {
  idEventoLocalidad: number;
  idUsuario: number;
  idMedioPago: number;
}