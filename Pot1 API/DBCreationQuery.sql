CREATE DATABASE Pot1;
GO
USE Pot1;
GO
CREATE TABLE Tipos_Rol(
	id_tipo_rol INT PRIMARY KEY IDENTITY,
	nombre NVARCHAR(20) UNIQUE NOT NULL
);
GO
INSERT INTO Tipos_Rol VALUES ('Cliente'),('Soporte'),('Administrador');
GO
CREATE TABLE Roles(
	id_rol INT PRIMARY KEY IDENTITY,
	nombre NVARCHAR(100) UNIQUE NOT NULL,
	capacidad INT NOT NULL CHECK(capacidad > 0),
	tipo_rol INT NOT NULL,
	FOREIGN KEY (tipo_rol) REFERENCES Tipos_Rol(id_tipo_rol)
);
GO
INSERT INTO Roles VALUES
('Cliente', 50, 1), ('Soporte Técnico', 20, 2), ('Administrador', 4, 3);
GO
CREATE TABLE Usuarios(
	id_usuario INT PRIMARY KEY IDENTITY,
	nombre NVARCHAR(50) NOT NULL,
	apellido NVARCHAR(50) NOT NULL,
	telefono NVARCHAR(20) UNIQUE NOT NULL,
	email NVARCHAR(250) UNIQUE NOT NULL,
	contrasena NVARCHAR(260) NOT NULL,
	tel_contacto NVARCHAR(20) NOT NULL,
	id_rol INT NOT NULL,
	FOREIGN KEY (id_rol) REFERENCES Roles(id_rol)
);
GO
INSERT INTO Usuarios VALUES ('Xavier Alexander', 'Ávila Posada', '7780-1219', 'xavieravilaposada@gmail.com', 'admin', '2447-5225', 3);
GO
CREATE TABLE Tickets(
	id_ticket INT PRIMARY KEY IDENTITY,
	estado NVARCHAR(35) CHECK (estado IN ('CREADO', 'ASIGNADO', 'EN ESPERA DE INFORMACIÓN', 'EN RESOLUCIÓN', 'RESUELTO')),
	descripcion NVARCHAR(MAX) NOT NULL,
	prioridad NVARCHAR(20) CHECK (prioridad IN ('BAJA', 'NORMAL', 'IMPORTANTE', 'CRÍTICA')),
	servicio NVARCHAR(250) NOT NULL,
	resuelta BIT DEFAULT 0,
	fecha DATETIME DEFAULT GETDATE(),
	id_encargado INT NULL,
	id_cliente INT NOT NULL,
	FOREIGN KEY (id_encargado) REFERENCES Usuarios(id_usuario),
	FOREIGN KEY (id_cliente) REFERENCES Usuarios(id_usuario)
);
GO
CREATE TABLE Archivos(
	id_archivo INT PRIMARY KEY IDENTITY,
	url NVARCHAR(400) NOT NULL,
	id_ticket INT NOT NULL,
	FOREIGN KEY (id_ticket) REFERENCES Tickets(id_ticket)
);
GO
CREATE TABLE Notificaciones(
	id_notificacion INT PRIMARY KEY IDENTITY,
	dato NVARCHAR(500) NOT NULL,
	url_archivo NVARCHAR(400) NOT NULL,
	notificar_cliente BIT DEFAULT 0,
	fecha DATETIME DEFAULT GETDATE(),
	autogenerada BIT DEFAULT 1,
	remitente INT NULL,
	id_ticket INT NOT NULL,
	FOREIGN KEY (remitente) REFERENCES Usuarios(id_usuario),
	FOREIGN KEY (id_ticket) REFERENCES Tickets(id_ticket)
);
GO
CREATE TABLE Tareas(
	id_tarea INT PRIMARY KEY IDENTITY,
	nombre NVARCHAR(70) NOT NULL,
	info NVARCHAR(250) NOT NULL,
	prioridad NVARCHAR(20) CHECK (prioridad IN ('BAJA', 'NORMAL', 'IMPORTANTE', 'CRÍTICA')),
	estado NVARCHAR(MAX) NOT NULL,
	completada BIT DEFAULT 0,
	id_ticket INT NOT NULL,
	id_encargado INT NULL,
	FOREIGN KEY (id_encargado) REFERENCES Usuarios(id_usuario),
	FOREIGN KEY (id_ticket) REFERENCES Tickets(id_ticket)
);