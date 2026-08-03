-------------------------------------Inserción de datos en las tablas-------------------------------

INSERT INTO usuario(nombre, apellido, correo)
VALUES
('Pablo', 'Corrales', 'pablo.corrales@correo.com'),
('Diego', 'Retana', 'diego.retana@correo.com'),
('Cesar', 'Obando', 'cesar.obando@correo.com'),
('Alexis','Sanchez','alexis.sanchez@correo.com');
GO

INSERT INTO medio_pago(nombre_medio_pago)
VALUES
('Tarjeta de Crédito'),
('Tarjeta de Débito'),
('Transferencia Bancaria'),
('Sinpe Movil');
GO

INSERT INTO sede_evento(nombre_sede_evento, ubicacion)
VALUES
('Estadio Nacional', 'San José, Costa Rica'),
('Parque Central', 'Alajuela, Costa Rica'),
('Teatro Melico Salazar', 'San José, Costa Rica'),
('Estadio Ricardo Saprissa', 'San Juan de Tibás, Costa Rica');
GO

INSERT INTO localidad(nombre_localidad, id_sede_evento)
VALUES
('Graderia Norte', 1),
('Graderia Sur', 1),
('Palco VIP', 2),
('Zona General', 3),
('Gramilla', 4);
GO

INSERT INTO evento(nombre_evento, fecha_evento, hora_evento)
VALUES
('Concierto System of a Down', '2024-07-15', '20:00:00'),
('X-Knights', '2024-08-10', '12:00:00'),
('90 Minutos por la vida', '2024-09-05', '19:30:00'),
('Saprissa vs Liga deportiva Alajuelense', '2024-10-20', '18:00:00');
GO

-- Asignar la Sede 1 a los eventos con ID 1 y 2
UPDATE evento 
SET IdSede = 1 
WHERE id_evento IN (1, 2);
GO
-- Asignar la Sede 2 a los eventos con ID 3 y 4
UPDATE evento 
SET IdSede = 2 
WHERE id_evento IN (3, 4);
GO

SELECT * FROM evento;

UPDATE sede_evento 
SET ubicacion = 'La Sabana, San José'
WHERE id_sede_evento = 1;
GO

SELECT * FROM sede_evento;
INSERT INTO evento_localidad(id_evento, id_localidad, precio, capacidad_disponible)
VALUES
(1, 1, 50000.00, 100),
(1, 2, 40000.00, 150),
(2, 3, 100000.00, 50),
(3, 4, 25000.00, 200),
(4, 5, 30000.00, 120);
GO

INSERT INTO factura (id_usuario, id_medio_pago, numero_factura, total)
VALUES
(1, 1, 'FAC-001', 50000.00),
(2, 2, 'FAC-002', 40000.00),
(3, 3, 'FAC-003', 100000.00),
(4, 4, 'FAC-004', 25000.00);
GO

INSERT INTO boleto (id_evento_localidad, id_factura, id_usuario, num_boleto)
VALUES
(1, 1, 1, 'BOL-001'),
(2, 2, 2, 'BOL-002'),
(3, 3, 3, 'BOL-003'),
(4, 4, 4, 'BOL-004');
GO


-----------------------------CONSULTA DE LAS TABLAS--------------------------------------

--Consulta para obtener los nombre de los roles.
SELECT nombre_role FROM role;

--Consulta para obtener la información completa de los usuarios.
SELECT u.nombre + ' ' + u.apellido AS 'Nombre completo',
u.correo AS 'Correo electronico',
u.telefono,
r.nombre_role AS 'Rol'
FROM usuario u
INNER JOIN role r ON u.id_role = r.id_role;
GO

-- Consulta para obtener los nombres de los medios de pago.
SELECT nombre_medio_pago AS 'Medio de pago'
FROM medio_pago;
GO


--Consulta para obtener la información completa de una sede y sus localidades.
SELECT n.nombre_sede_evento AS 'Sede evento', 
l.nombre_localidad AS 'localidad',  
el.capacidad_disponible AS 'Capacidad maxima',
el.precio AS 'Precio localidad'
FROM evento_localidad el
INNER JOIN localidad l ON el.id_localidad = l.id_localidad 
INNER JOIN sede_evento n ON l.id_sede_evento = n.id_sede_evento;
GO

--Consulta para obtener la lista completa de los eventos.
SELECT nombre_evento,
CAST(fecha_evento AS DATE) AS 'fecha_evento',
hora_evento
FROM evento;
GO 

--Consulta para obtener la información completa de un evento, su ubicación, fecha y hora.
SELECT ev.nombre_evento,
s.nombre_sede_evento, 
CAST(ev.fecha_evento AS DATE) AS 'fecha_evento', 
ev.hora_evento, 
s.ubicacion
FROM evento ev
INNER JOIN evento_localidad el ON ev.id_evento = el.id_evento
INNER JOIN localidad l ON el.id_localidad = l.id_localidad
INNER JOIN sede_evento s ON l.id_sede_evento = s.id_sede_evento
GO

--Consulta para obtener la información completa de las facturas.
SELECT u.nombre + ' ' + u.apellido AS 'Nombre completo', 
mp.nombre_medio_pago, 
f.numero_factura, 
f.total
FROM factura f
INNER JOIN usuario u ON f.id_usuario = u.id_usuario
INNER JOIN medio_pago mp ON f.id_medio_pago = mp.id_medio_pago
GO

-- Consulta para obtener la información completa de los boletos.
SELECT u.nombre + ' ' + u.apellido AS 'Nombre completo', 
e.nombre_evento, CAST(e.fecha_evento AS DATE) AS 'fecha_evento', e.hora_evento,
s.nombre_sede_evento, 
l.nombre_localidad, 
f.numero_factura, f.total
FROM boleto b
INNER JOIN usuario u ON b.id_usuario = u.id_usuario
INNER JOIN evento_localidad el ON b.id_evento_localidad = el.id_evento_localidad
INNER JOIN localidad l ON el.id_localidad = l.id_localidad
INNER JOIN evento e ON el.id_evento = e.id_evento
INNER JOIN sede_evento s ON l.id_sede_evento = s.id_sede_evento
INNER JOIN factura f ON b.id_factura = f.id_factura
GO

