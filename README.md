# 🏢 Proyecto Final Lab2 MVC
#ReservAr
### *Creamos un sistema de gestión de turnos. Principalmente orientado a Canchas de Futbol*    

## 📅 Entrega
- Materia: Laboratorio de Software II
- Proyecto: Proyecto Integrador Final (ASP.NET Core MVC + MySQL)
  
## 👥 Integrantes
- *Hoyo, Jeremias - jeremiashoyo035@gmail.com - @Ego572 https://github.com/Ego572 - Discord: agony9999*  
- *Mazza, Agustin - agusmazza@gmail.com - @AgustinMazza https://github.com/AgustinMazza - Discord: cote8942*  
- *Rodríguez, Juan Cruz - juancruzrodriguez0@gmail.com - @JuanCRodriguez0 https://github.com/JuanCRodriguez0 - Discord: juancruzr*  



## 📊 Modelado de Datos

*A continuación se presenta el esquema del modelo de datos correspondiente a la aplicación.*

### *Diagrama de Clases* 
```mermaid
erDiagram
    Rol ||--o{ Usuario : tiene
    Categoria ||--o{ Negocio : clasifica
    Usuario ||--o{ Negocio : administra
    Negocio ||--o{ ImagenNegocio : tiene
    Negocio ||--o{ Recurso : tiene
    Recurso ||--o{ HorarioAtencion : atiende
    Recurso ||--o{ Tarifa : cobra
    Recurso ||--o{ Reserva : ocupa
    Negocio ||--o{ Reserva : recibe
    Usuario ||--o{ Reserva : reserva

    Rol {
        int IdRol PK
        string Nombre UK
    }

    Categoria {
        int IdCategoria PK
        string Nombre UK
        string Descripcion
        string Icono
    }

    Usuario {
        int IdUsuario PK
        int IdRol FK
        string Nombre
        string Apellido
        string Email UK
        string PasswordHash
        string Telefono
        string Avatar
        bool Activo
        datetime FechaAlta
    }

    Negocio {
        int IdNegocio PK
        int IdCategoria FK
        int IdUsuario FK "dueño con rol Admin"
        string Nombre
        string Descripcion
        string Direccion
        string Ciudad
        string Telefono
        string LogoUrl
        bool Activo
    }

    ImagenNegocio {
        int IdImagenNegocio PK
        int IdNegocio FK
        string Ruta
        int Orden
        bool EsPortada
    }

    Recurso {
        int IdRecurso PK
        int IdNegocio FK
        string Nombre
        string Descripcion
        int DuracionMin
        bool Activo
    }

    HorarioAtencion {
        int IdHorarioAtencion PK
        int IdRecurso FK
        tinyint DiaSemana "0=domingo a 6=sabado"
        time HoraDesde
        time HoraHasta
    }

    Tarifa {
        int IdTarifa PK
        int IdRecurso FK
        string Nombre
        time HoraDesde
        time HoraHasta
        decimal PrecioHora
        bool Activo
    }

    Reserva {
        int IdReserva PK
        int IdUsuario FK "cliente"
        int IdRecurso FK
        int IdNegocio FK
        date Fecha
        time HoraInicio
        time HoraFin
        tinyint Estado "enum EstadoReserva"
        decimal Precio
        string Observaciones
        string Comprobante
        datetime FechaCreacion
    }
```

## 💾 Pasos a seguir para levantar la BD

### 1. Requisitos Previos
* .NET SDK 10.0 o superior
* MySQL Server 8.0 o superior
* IDE recomendado: Visual Studio Code
* Interfaz recomendada para la Base de Datos: DBeaver

### 2. Base de Datos
## 🗄️ Configuración de la Base de Datos (DBeaver)

1. **Conectarse al servidor:**
   * Abrir DBeaver y conectarse al servidor MySQL local.
   * No hace falta crear la base a mano: el script `init.sql` la crea solo (`reservar`).

2. **Ejecutar el Script SQL:**
   * Abrir el archivo `init.sql` ubicado en la carpeta `/Scripts` del proyecto.
   * Copiar todo su contenido.
   * En DBeaver, con la conexión al servidor seleccionada, ir al menú **SQL Editor** $\rightarrow$ **New SQL Script**.
   * Pegar el contenido del script.
   * Ejecutar todo el script presionando el botón **Execute Script** (el ícono con la hoja y el rayo naranja) o `Alt + X`.
   * El script se puede ejecutar las veces que haga falta, pero **borra y recrea las tablas**, así que se pierden los datos que hubiera cargado.

3. **Verificar Tablas:**
   * Hacer clic derecho sobre la base de datos `reservar` en DBeaver y seleccionar **Refresh** (`F5`).
   * Desplegar la sección **Tables** para confirmar la presencia de `roles`, `categorias`, `usuarios`, `negocios`, `imagenes_negocio`, `recursos`, `horarios_atencion`, `tarifas` y `reservas`.

### 3. Cadena de conexión rapida

El archivo `appsettings.json` no se sube al repositorio porque contiene la contraseña de cada uno. En su lugar hay un molde, `appsettings.json.example`:

1. Copiar el molde con el nombre real. Desde la raíz del repositorio:
```bash
   cp ReservAr/appsettings.json.example ReservAr/appsettings.json
```
2. Abrir `ReservAr/appsettings.json` y reemplazar `TuUsuario` y `TuContraseña` por los datos de tu MySQL local:
```json
   "DefaultConnection": "Server=localhost;Port=3306;Database=reservar;User=TuUsuario;Password=TuContraseña;"
```


## ▶️ Cómo correr el proyecto

```bash
cd ReservAr
dotnet restore
dotnet run
```

`dotnet run` muestra en la consola la dirección donde queda corriendo la aplicación (por ejemplo, `https://localhost:xxxx`).
