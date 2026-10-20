-- =========================================================
-- ReservAr - Sistema de reservas de canchas
-- =========================================================

-- 1) Crear BD
CREATE DATABASE IF NOT EXISTS reservar
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE reservar;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS reservas;
DROP TABLE IF EXISTS tarifas;
DROP TABLE IF EXISTS horarios_atencion;
DROP TABLE IF EXISTS recursos;
DROP TABLE IF EXISTS imagenes_negocio;
DROP TABLE IF EXISTS negocios;
DROP TABLE IF EXISTS usuarios;
DROP TABLE IF EXISTS categorias;
DROP TABLE IF EXISTS roles;

-- 2) Roles
CREATE TABLE roles (
    IdRol   INT AUTO_INCREMENT PRIMARY KEY,
    Nombre  VARCHAR(30) NOT NULL,
    UNIQUE KEY uq_roles_nombre (Nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3) Categorias (rubros)
CREATE TABLE categorias (
    IdCategoria  INT AUTO_INCREMENT PRIMARY KEY,
    Nombre       VARCHAR(100) NOT NULL,
    Descripcion  VARCHAR(300) NULL,
    Icono        VARCHAR(100) NULL,
    UNIQUE KEY uq_categorias_nombre (Nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4) Usuarios (clientes, duenos y superadmin; se distinguen por IdRol)
CREATE TABLE usuarios (
    IdUsuario     INT AUTO_INCREMENT PRIMARY KEY,
    IdRol         INT NOT NULL,
    Nombre        VARCHAR(100) NOT NULL,
    Apellido      VARCHAR(100) NOT NULL,
    Email         VARCHAR(150) NOT NULL,
    PasswordHash  VARCHAR(255) NOT NULL,
    Telefono      VARCHAR(30)  NULL,
    Avatar        VARCHAR(300) NULL,
    Activo        BOOLEAN      NOT NULL DEFAULT 1,
    FechaAlta     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_usuarios_email (Email),
    CONSTRAINT fk_usuarios_rol FOREIGN KEY (IdRol) REFERENCES roles (IdRol)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5) Negocios (IdUsuario = el duenio, con rol Admin)
CREATE TABLE negocios (
    IdNegocio    INT AUTO_INCREMENT PRIMARY KEY,
    IdCategoria  INT NOT NULL,
    IdUsuario    INT NOT NULL,
    Nombre       VARCHAR(150) NOT NULL,
    Descripcion  VARCHAR(500) NULL,
    Direccion    VARCHAR(200) NOT NULL,
    Ciudad       VARCHAR(100) NOT NULL,
    Telefono     VARCHAR(30)  NULL,
    LogoUrl      VARCHAR(300) NULL,
    Activo       BOOLEAN      NOT NULL DEFAULT 1,
    CONSTRAINT fk_negocios_categoria FOREIGN KEY (IdCategoria) REFERENCES categorias (IdCategoria),
    CONSTRAINT fk_negocios_usuario   FOREIGN KEY (IdUsuario)   REFERENCES usuarios (IdUsuario)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6) Imagenes del negocio
CREATE TABLE imagenes_negocio (
    IdImagenNegocio  INT AUTO_INCREMENT PRIMARY KEY,
    IdNegocio        INT NOT NULL,
    Ruta             VARCHAR(300) NOT NULL,
    Orden            VARCHAR(300) NOT NULL,
    EsPortada        BOOLEAN NOT NULL DEFAULT 0,
    CONSTRAINT fk_imagenes_negocio FOREIGN KEY (IdNegocio) REFERENCES negocios (IdNegocio) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7) Recursos (las canchas). DuracionMin = cuanto dura cada reserva (en minutos)
CREATE TABLE recursos (
    IdRecurso    INT AUTO_INCREMENT PRIMARY KEY,
    IdNegocio    INT NOT NULL,
    Nombre       VARCHAR(100) NOT NULL,
    Descripcion  VARCHAR(300) NULL,
    DuracionMin  INT NOT NULL DEFAULT 60,
    Activo       BOOLEAN NOT NULL DEFAULT 1,
    CONSTRAINT fk_recursos_negocio FOREIGN KEY (IdNegocio) REFERENCES negocios (IdNegocio),
    CONSTRAINT ck_recursos_duracion CHECK (DuracionMin > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8) Horario de atencion de cada cancha.
--    DiaSemana: 0=domingo ... 6=sabado (igual que DayOfWeek de C#)
--    Para "hasta medianoche" se usa '24:00:00'
CREATE TABLE horarios_atencion (
    IdHorarioAtencion  INT AUTO_INCREMENT PRIMARY KEY,
    IdRecurso          INT NOT NULL,
    DiaSemana          TINYINT NOT NULL,
    HoraDesde          TIME NOT NULL,
    HoraHasta          TIME NOT NULL,
    CONSTRAINT fk_horarios_recurso FOREIGN KEY (IdRecurso) REFERENCES recursos (IdRecurso) ON DELETE CASCADE,
    CONSTRAINT ck_horarios_dia  CHECK (DiaSemana BETWEEN 0 AND 6),
    CONSTRAINT ck_horarios_hora CHECK (HoraDesde < HoraHasta)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 9) Tarifas: precio por hora segun la parte del dia
--    El nombre es libre (Manana, Noche, Premium...). El sistema usa horario y precio
CREATE TABLE tarifas (
    IdTarifa    INT AUTO_INCREMENT PRIMARY KEY,
    IdRecurso   INT NOT NULL,
    Nombre      VARCHAR(50) NOT NULL,
    HoraDesde   TIME NOT NULL,
    HoraHasta   TIME NOT NULL,
    PrecioHora  DECIMAL(12,2) NOT NULL,
    Activo      BOOLEAN NOT NULL DEFAULT 1,
    CONSTRAINT fk_tarifas_recurso FOREIGN KEY (IdRecurso) REFERENCES recursos (IdRecurso) ON DELETE CASCADE,
    CONSTRAINT ck_tarifas_precio  CHECK (PrecioHora > 0),
    CONSTRAINT ck_tarifas_hora    CHECK (HoraDesde < HoraHasta)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 10) Reservas
--    Estado: 0=Pendiente, 1=Confirmada, 2=Cancelada, 3=Completada (enum EstadoReserva en C#)
--    IdNegocio se repite a proposito (evita joins al listar las reservas de un negocio, ayudin del profe)
--    Precio es una foto del precio calculado al reservar: no cambia si cambian las tarifas, en criollo un ticket
CREATE TABLE reservas (
    IdReserva      INT AUTO_INCREMENT PRIMARY KEY,
    IdUsuario      INT NOT NULL,
    IdRecurso      INT NOT NULL,
    IdNegocio      INT NOT NULL,
    Fecha          DATE NOT NULL,
    HoraInicio     TIME NOT NULL,
    HoraFin        TIME NOT NULL,
    Estado         TINYINT NOT NULL DEFAULT 0,
    Precio         DECIMAL(12,2) NOT NULL,
    Observaciones  VARCHAR(500) NULL,
    Comprobante    VARCHAR(300) NULL,
    FechaCreacion  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_reservas_recurso_fecha (IdRecurso, Fecha),
    INDEX idx_reservas_negocio_fecha (IdNegocio, Fecha),
    CONSTRAINT fk_reservas_usuario FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario),
    CONSTRAINT fk_reservas_recurso FOREIGN KEY (IdRecurso) REFERENCES recursos (IdRecurso),
    CONSTRAINT fk_reservas_negocio FOREIGN KEY (IdNegocio) REFERENCES negocios (IdNegocio),
    CONSTRAINT ck_reservas_hora   CHECK (HoraInicio < HoraFin),
    CONSTRAINT ck_reservas_estado CHECK (Estado BETWEEN 0 AND 3)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================================================
-- Datos de prueba
-- =========================================================
INSERT INTO roles (Nombre) VALUES ('Usuario'), ('Admin'), ('SuperAdmin');

INSERT INTO categorias (Nombre, Descripcion) VALUES
('Canchas de futbol', 'Alquiler de canchas de futbol por hora');

-- PasswordHash es un valor de relleno. Cuando este hecho el login,
-- se reemplaza por los hashes reales generados por la aplicacion
INSERT INTO usuarios (IdRol, Nombre, Apellido, Email, PasswordHash, Telefono) VALUES
(3, 'Super', 'Admin',  'superadmin@reservar.com', '600000.QonjeZg1t97+2sobE/XI/Q==.qnecRmqs4PEh7nWemaMObxyQtO4SzfaYF7g0zBx1ick=', NULL),
(2, 'Marta', 'Gomez',  'marta@reservar.com',      '600000.bVm0kDiXaJWwqkCN6B7Lqw==.SAkXCq2rZ0ULi4wM7I9Titnit12eaF9m2D49+1Her8E=', '3511112222'),
(1, 'Juan',  'Perez',  'juan@reservar.com',       '600000.tblMc32kISK6JO94RJL/8Q==.i/E1R6Yk843PHjW4zd7hmUDvnXcI15tqms4/kfvs56Q=', '3513334444'),
(1, 'Sofia', 'Lopez',  'sofia@reservar.com',      '600000.Gkd3HMNP5CpbSiHBRQAAdQ==.K3HqxOLggMoWEWBwJmMpI6Y2QCrrRnzqeOPc19yajQk=', '3515556666');

INSERT INTO negocios (IdCategoria, IdUsuario, Nombre, Descripcion, Direccion, Ciudad, Telefono) VALUES
(1, 2, 'Complejo La Pelota', 'Canchas de futbol 5 con vestuarios y buffet', 'Av. Colon 1234', 'Cordoba', '3511112222');

INSERT INTO recursos (IdNegocio, Nombre, Descripcion, DuracionMin) VALUES
(1, 'Cancha 1', 'Cesped sintetico, techada', 60),
(1, 'Cancha 2', 'Cesped sintetico, al aire libre', 60);

-- Horario de atencion: todos los dias de 08:00 a 24:00
INSERT INTO horarios_atencion (IdRecurso, DiaSemana, HoraDesde, HoraHasta)
SELECT r.IdRecurso, d.dia, '08:00:00', '24:00:00'
FROM recursos r
CROSS JOIN (SELECT 0 AS dia UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3
            UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6) d;

INSERT INTO tarifas (IdRecurso, Nombre, HoraDesde, HoraHasta, PrecioHora) VALUES
(1, 'Manana', '08:00:00', '14:00:00',  8000.00),
(1, 'Tarde',  '14:00:00', '18:00:00', 10000.00),
(1, 'Noche',  '18:00:00', '24:00:00', 15000.00),
(2, 'Manana', '08:00:00', '14:00:00',  7000.00),
(2, 'Tarde',  '14:00:00', '18:00:00',  9000.00),
(2, 'Noche',  '18:00:00', '24:00:00', 13000.00);

INSERT INTO reservas (IdUsuario, IdRecurso, IdNegocio, Fecha, HoraInicio, HoraFin, Estado, Precio) VALUES
(3, 1, 1, '2026-10-17', '10:00:00', '11:00:00', 1,  8000.00),
(4, 1, 1, '2026-10-17', '17:30:00', '18:30:00', 0, 12500.00),
(3, 2, 1, '2026-10-18', '20:00:00', '21:00:00', 1, 13000.00);

SET FOREIGN_KEY_CHECKS = 1;