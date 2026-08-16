CREATE TABLE role (
    id_role INT IDENTITY(1,1),
    nombre_role NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_role PRIMARY KEY (id_role)
);
GO

UPDATE [usuario]
SET id_role = 1
WHERE correo = 'pablo.prueba@correo.com';
GO



CREATE TABLE usuario (
    id_usuario INT IDENTITY(1,1),
    nombre NVARCHAR(50) NOT NULL,
    apellido NVARCHAR(50) NOT NULL,
    correo VARCHAR(100) NOT NULL UNIQUE,
    telefono NVARCHAR(20) NULL,
    password_hash NVARCHAR(MAX) NOT NULL,
    id_role INT NULL,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_usuario PRIMARY KEY (id_usuario),
    CONSTRAINT FK_usuario_role FOREIGN KEY (id_role) REFERENCES role (id_role)
);
GO

CREATE TABLE medio_pago(
    id_medio_pago INT IDENTITY(1,1),
    nombre_medio_pago NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_medio_pago PRIMARY KEY(id_medio_pago)
);
GO

CREATE TABLE sede_evento(
    id_sede_evento INT IDENTITY(1,1),
    nombre_sede_evento NVARCHAR(50) NOT NULL,
    ubicacion NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_sede_evento PRIMARY KEY(id_sede_evento)
);
GO

CREATE TABLE evento(
    id_evento INT IDENTITY(1,1),
    nombre_evento NVARCHAR(100) NOT NULL,
    fecha_evento DATETIME NOT NULL,
    hora_evento TIME NOT NULL,
    CONSTRAINT PK_evento PRIMARY KEY(id_evento)
);
GO

//Agrega columna para el manejo de las imagenes.
ALTER TABLE evento
ADD image_evento VARCHAR(255) NULL;
GO


ALTER TABLE evento
ADD CONSTRAINT FK_evento_sede_evento 
FOREIGN KEY (IdSede) 
REFERENCES sede_evento(id_sede_evento);
GO


CREATE TABLE asiento(
    id_asiento INT IDENTITY(1,1),
    id_localidad INT NOT NULL,
    fila NVARCHAR(10) NOT NULL,
    numero NVARCHAR(10) NOT NULL,
    CONSTRAINT PK_asiento PRIMARY KEY (id_asiento),
    CONSTRAINT FK_asiento_localidad FOREIGN KEY (id_localidad) REFERENCES localidad(id_localidad)
);
GO

ALTER TABLE boleto
ADD id_asiento INT NULL;
GO


ALTER TABLE boleto
ADD CONSTRAINT FK_boleto_asiento 
FOREIGN KEY (id_asiento) 
REFERENCES asiento(id_asiento);
GO

CREATE TABLE localidad(
    id_localidad INT IDENTITY(1,1),
    nombre_localidad NVARCHAR(100) NOT NULL,
    id_sede_evento INT NOT NULL,
    CONSTRAINT PK_localidad PRIMARY KEY (id_localidad),
    CONSTRAINT FK_localidad_sede_evento FOREIGN KEY (id_sede_evento) REFERENCES sede_evento(id_sede_evento)
);
GO


CREATE TABLE factura(
    id_factura INT IDENTITY(1,1),
    id_usuario INT NOT NULL,
    id_medio_pago INT NOT NULL,
    fecha_factura DATETIME DEFAULT GETDATE(),
    numero_factura NVARCHAR(50) NOT NULL UNIQUE,
    total DECIMAL(10, 2) NOT NULL,
    CONSTRAINT PK_factura PRIMARY KEY(id_factura),
    CONSTRAINT FK_factura_usuario FOREIGN KEY(id_usuario) REFERENCES usuario(id_usuario),
    CONSTRAINT FK_factura_medio_pago FOREIGN KEY(id_medio_pago) REFERENCES medio_pago(id_medio_pago)
);
GO

CREATE TABLE evento_localidad(
    id_evento_localidad INT IDENTITY(1,1),
    id_evento INT NOT NULL,
    id_localidad INT NOT NULL,
    precio DECIMAL(10,2) NOT NULL,
    capacidad_disponible INT NOT NULL,
    CONSTRAINT PK_evento_localidad PRIMARY KEY(id_evento_localidad),
    CONSTRAINT FK_evento FOREIGN KEY(id_evento) REFERENCES evento(id_evento),
    CONSTRAINT FK_localidad FOREIGN KEY(id_localidad) REFERENCES localidad(id_localidad)
);
GO


CREATE TABLE boleto(
    id_boleto INT IDENTITY(1,1),
    id_evento_localidad INT NOT NULL,
    id_factura INT NOT NULL,
    id_usuario INT NOT NULL,
    num_boleto NVARCHAR(50) NOT NULL UNIQUE,
    fecha_compra DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_boleto PRIMARY KEY (id_boleto),
    CONSTRAINT FK_evento_localidad FOREIGN KEY (id_evento_localidad) REFERENCES evento_localidad(id_evento_localidad),
    CONSTRAINT FK_factura FOREIGN KEY(id_factura) REFERENCES factura(id_factura),
    CONSTRAINT FK_usuario FOREIGN KEY(id_usuario) REFERENCES usuario(id_usuario)
);
GO