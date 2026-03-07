# Bolsa FEUCN - Backend

Backend del proyecto **Bolsa FEUCN**, una plataforma de bolsa de empleo y marketplace para estudiantes de la Universidad Católica del Norte.

Desarrollado con **.NET 9.0**, **ASP.NET Core**, **Entity Framework Core** y **PostgreSQL**, siguiendo una arquitectura limpia por capas.

---

## Tecnologias utilizadas

| Categoria | Tecnologia |
|---|---|
| Framework | .NET 9.0 / ASP.NET Core |
| ORM | Entity Framework Core |
| Base de datos | PostgreSQL 16 |
| Autenticacion | ASP.NET Core Identity + JWT Bearer |
| Mapeo de objetos | Mapster |
| Notificaciones de correo | Resend API |
| Almacenamiento de archivos | Cloudinary |
| Generacion de PDF | QuestPDF (Community License) |
| Tareas en segundo plano | Hangfire (MemoryStorage) |
| Logging | Serilog (consola + archivo rotativo) |
| Contenedor de BD | Docker / Docker Compose |

---

## Arquitectura

El proyecto sigue el patron de **Clean Architecture** dividido en cuatro capas:

```
backend/src/
├── Domain/          # Modelos y enums (sin dependencias externas)
├── Infrastructure/  # AppDbContext, Repositorios, DataSeeder
├── Application/     # Servicios, DTOs, Mappers, Validadores
└── API/             # Controladores, Middlewares
```

**Flujo de una solicitud:** Repositorio -> Servicio -> Controlador. Toda la logica de negocio reside en la capa de servicios.

---

## Requisitos previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)


```bash
choco install make
```

---

## Configuracion inicial

### 1. Extraer el codigo fuente

Descomprimir el archivo `.zip` recibido y navegar a la carpeta del backend:

```bash
cd BolsaFeUCN/backend
```

### 2. Configurar credenciales

Copiar el archivo de configuracion de ejemplo y completar los valores:

Editar `appsettings.json` completando los valores vacios con las credenciales de los servicios externos:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=HOST;Port=PORT;Database=DB-NAME;Username=DB-USER;Password=DB-PASSWORD"
  },
  "Cloudinary": {
    "CloudName": "<cloud_name>",
    "ApiKey": "<api_key>",
    "ApiSecret": "<api_secret>"
  },
  "Jwt": {
    "Key": "<clave_secreta_minimo_32_caracteres>"
  },
  "ResendApiKey": "<resend_api_key>",
  "Smtp": {
    "Host": "smtp.tu-dominio.com",
    "Port": 587,
    "EnableSsl": true,
    "User": "notificaciones@tu-dominio.com",
    "Password": "<password_smtp>"
  }
}
```

> **Nota:** Nunca compartir ni publicar el archivo `appsettings.json` con credenciales reales.

### 3. Levantar la base de datos con Docker

```bash
# Con Docker Compose (recomendado)
docker-compose up -d

# O con Make
make docker-create
```

Credenciales de desarrollo por defecto:

| Parametro | Valor |
|---|---|
| Host | localhost |
| Puerto | 5432 |
| Base de datos | bolsafeucn-db |
| Usuario | bolsafeucn-user |
| Contrasena | bolsafeucn-password |

### 4. Restaurar dependencias y aplicar migraciones

```bash
dotnet restore
dotnet ef database update
```

---

## Ejecucion del proyecto

```bash
# Modo normal
dotnet run

# Modo desarrollo con recarga automatica (recomendado)
dotnet watch --no-hot-reload
```

El servidor queda disponible en:

- HTTP: `http://localhost:5185`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger` (solo en Development)
- Hangfire Dashboard: `http://localhost:5185/hangfire` (solo en Development)

---

## Comandos Make

```bash
make db-restart      # Elimina la BD, aplica migraciones y lanza dotnet watch
make db-migrate      # Elimina la BD, crea una nueva migracion y actualiza
make run             # dotnet run
make watch           # dotnet watch --no-hot-reload
make docker-create   # Crea y lanza un nuevo contenedor PostgreSQL
make docker-start    # Inicia un contenedor PostgreSQL existente
make docker-stop     # Detiene el contenedor PostgreSQL
make docker-rm       # Detiene y elimina el contenedor PostgreSQL
make help            # Lista todos los comandos disponibles
```

> Los comandos `make docker-*` leen las credenciales directamente desde `appsettings.json`.

---

## Migraciones de base de datos

```bash
# Aplicar migraciones existentes
dotnet ef database update

# Crear una nueva migracion
dotnet ef migrations add NombreDeLaMigracion
dotnet ef database update

# Revertir a una migracion anterior
dotnet ef database update NombreMigracionAnterior

# Eliminar la base de datos (util en desarrollo)
dotnet ef database drop --force
```

---

## Comandos utiles de Docker

```bash
# Ver logs del contenedor
docker logs bolsafeucn-container

# Conectarse a PostgreSQL desde la terminal
docker exec -it bolsafeucn-container psql -U bolsafeucn-user -d bolsafeucn-db

# Detener todos los servicios del compose
docker-compose down
```

---

## Credenciales de prueba

Al iniciar por primera vez, el sistema ejecuta `DataSeeder` y crea los siguientes usuarios (contrasena `Test123!`):

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📋 CREDENCIALES DE PRUEBA:
👨‍🎓 ESTUDIANTE: estudiante@alumnos.ucn.cl / Test123!
🏢 EMPRESA: empresa@techcorp.cl / Test123!
👤 PARTICULAR: particular@ucn.cl / Test123!
👑 ADMIN: admin@ucn.cl / Test123!
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Funcionalidades del sistema

### Autenticacion y usuarios (`/api/auth`, `/api/profile`, `/api/cv`)

- Registro de usuarios por tipo: Estudiante, Particular, Empresa (con validacion de dominio de correo).
- Registro de Administradores (restringido a SuperAdmin).
- Verificacion de correo electronico con codigo de 6 digitos (valido 15 minutos).
- Reenvio del codigo de verificacion.
- Login con JWT Bearer (token valido 24 horas).
- Restablecimiento de contrasena por correo electronico.
- Cambio de correo electronico con verificacion del nuevo correo.
- Cambio de contrasena autenticado.
- Consulta y actualizacion del perfil de usuario (nombre, datos de contacto, foto de perfil via Cloudinary).
- Subida, descarga y eliminacion de CV en formato PDF (solo Estudiantes).
- Activacion/desactivacion de notificaciones por correo.

Restricciones de dominio configuradas en `appsettings.json`:

- Estudiantes: `@alumnos.ucn.cl`
- Administradores: `@ucn.cl`

---

### Publicaciones (`/api/publications`)

El sistema maneja dos tipos de publicaciones que extienden de un modelo base (`Publication`):

**Ofertas laborales (`Offer`)**

- Creacion de ofertas con titulo, descripcion, remuneracion, fechas, informacion de contacto y cupos disponibles. Restringido a usuarios con rol `Offeror`.
- Flujo de validacion por administrador antes de ser publicada (`ApprovalStatus`: `Pendiente`, `Approbada`, `Rechazada`).
- Avance manual del estado de la oferta por el oferente.
- Cierre manual de la oferta por el oferente o el administrador.
- Exploracion publica de ofertas con filtros y paginacion.
- Vista de detalles publica (informacion basica) y autenticada (informacion completa, solo para Applicants).

**Publicaciones de Compra/Venta (`BuySell`)**

- Creacion con titulo, descripcion, precio, condicion e imagenes (hasta 3, almacenadas en Cloudinary). Restringido a `Offeror`.
- Edicion de detalles e imagenes por el oferente.
- Activacion/desactivacion de visibilidad por el oferente.
- Exploracion publica con filtros y paginacion.
- Vista de detalles publica y autenticada (datos de contacto del vendedor, solo Applicants).

**Vista del oferente**

- Listado de publicaciones propias con filtros y paginacion.
- Detalle de una publicacion propia con sus postulaciones asociadas.

---

### Postulaciones (`/api/publications/offers/{id}/apply`, `/api/publications/my-applications`)

- Postulacion a una oferta laboral con carta de presentacion. Restringido a `Applicant`.
- Cancelacion de una postulacion activa por el estudiante.
- Listado de postulaciones propias con filtros y paginacion.
- Detalle de una postulacion especifica.
- Actualizacion de la carta de presentacion de una postulacion.
- Listado de postulantes a una oferta del oferente con filtros y paginacion.
- Cambio del estado de una postulacion por el oferente (`Pendiente`, `Aceptada`, `Rechazada`, etc.).
- Descarga del CV del postulante por el oferente.

> Un estudiante no puede postular si tiene mas de 3 reseñas pendientes de completar.

---

### Sistema de reseñas (`/api/reviews`)

El sistema de reseñas es **bidireccional**: al finalizar una oferta, tanto el oferente como el estudiante deben calificarse mutuamente dentro de una ventana de 14 dias.

- Registro de reseña del estudiante hacia el oferente (`PATCH /reviews/{id}/applicant`).
- Registro de reseña del oferente hacia el estudiante (`PATCH /reviews/{id}/offeror`).
- Cada reseña incluye calificacion numerica, comentario y (para el oferente) un checklist de puntos de evaluacion (`IsOnTime`, `IsPresentable`, `IsRespectful`).
- Las reseñas no completadas en el plazo se cierran automaticamente mediante un job de Hangfire (programado cuando se creo la oferta y actualizado durante su vida util).
- Listado de reseñas propias con filtros y paginacion.
- Detalle de una reseña especifica.
- Generacion de reporte PDF con todas las reseñas del usuario (`GET /reviews/pdf`).
- La calificacion promedio (`Rating`) del usuario se calcula automaticamente a partir de sus reseñas.

---

### Panel de administracion (`/api/admin`, `/api/admin/publications`)

**Gestion de usuarios**

- Listado de todos los usuarios con filtros y paginacion.
- Consulta del perfil de cualquier usuario.
- Bloqueo/desbloqueo de usuarios (`toggle-block`).
- Registro de nuevos administradores (restringido a SuperAdmin).
- Eliminacion de administradores (restringido a SuperAdmin).

**Gestion de publicaciones**

- Listado de todas las publicaciones del sistema con filtros.
- Detalle de cualquier publicacion.
- Listado de publicaciones de un usuario especifico.
- Cierre manual de publicaciones con motivo.
- Listado de postulantes a cualquier oferta.

**Flujo de aprobacion de publicaciones** (`/api/admin/publications/pending`)

- Listado de publicaciones pendientes de aprobacion con filtros.
- Detalle de una publicacion pendiente.
- Aprobar o rechazar una publicacion (`PATCH /pending/{id}/validate`).
- Soporte para apelaciones por parte del oferente (maximo 3 apelaciones, respuesta en 48 horas).

**Gestion de reseñas**

- Listado de todas las reseñas del sistema con filtros.
- Detalle de cualquier reseña.
- Ocultamiento de informacion de una reseña (comentarios inapropiados).
- Generacion de reporte PDF del sistema o de un usuario especifico.

---

### Notificaciones y correos electronicos

- Correos transaccionales via **Resend** (verificacion de email, bienvenida, restablecimiento de contrasena, cambio de correo).
- Notificaciones internas al sistema (modelo `UserNotification`) para eventos relevantes como cambios de estado en postulaciones o resenas.
- Resumen periodico de notificaciones por correo (`IEmailDigestService`).
- Configuracion de notificaciones por usuario (activas/desactivadas).

 **Puntos de mejoras**
 - El sistema de notificaciones actual usa el patron Domain Events para generar eventos de dominio (ejemplo: `ApplicationStatusChangedEvent`) que son manejados por handlers (`ApplicationStatusChangedHandler`) para crear registros de notificacion en la base de datos (`UserNotification`) y enviar correos electronicos via Resend. Sin embargo, este enfoque tiene algunas limitaciones:
   - Acoplamiento entre la logica de negocio y el sistema de notificaciones.
   - Dificultad para manejar escenarios complejos como reintentos, fallos en el envio de correos o personalizacion de mensajes.
   - El uso de Hangfire para enviar resumenes periodicos es una solucion temporal que no escala bien a medida que crece el numero de usuarios y eventos.
- Una mejor solucion, dado los suficientes recursos disponibles, seria implementar un sistema de notificaciones mas robusto y desacoplado utilizando una arquitectura basada en eventos con una cola de mensajes (ejemplo: RabbitMQ o Azure Service Bus). 
- Una gran inconvenecia actual es la unica forma de comunicar notificaciones al usuario es a traves de correos electronicos, lo cual no es ideal para algunos eventos, ni para todos los usuarios. Implementar un sistema de notificaciones en tiempo real dentro de la aplicacion (ejemplo: usando SignalR) mejoraria significativamente la experiencia del usuario y permitiria una comunicacion mas fluida y efectiva. Ademas, esto permitiria reducir la cantidad de correos enviados, evitando saturar a los usuarios con mensajes transaccionales que pueden ser mejor recibidos como notificaciones dentro de la plataforma.
- El sistema utiliza un servicio de correo con limitacion de tasa (`EmailRateLimitedService`) para evitar exceder los limites de envio de Resend, pero no esta probado extensamente, solo lo necesario para comprobar que no se violen los limites en escenarios de prueba. Seria necesario realizar pruebas de carga mas exhaustivas para asegurar que el sistema se comporta correctamente bajo condiciones de alto volumen de notificaciones.


---

### Tareas programadas (Hangfire)

Las siguientes tareas corren automaticamente en segundo plano:

| Tarea | Frecuencia | Descripcion |
|---|---|---|
| `CloseExpiredReviewsAsync` | Cada hora | Cierra resenas cuya ventana de 14 dias vencio |
| Limpieza de usuarios sin verificar | Configurable | Elimina cuentas no verificadas tras N dias |
| Limpieza de cambios de correo pendientes | Configurable | Elimina solicitudes de cambio de correo antiguas |

El dashboard de Hangfire esta disponible en `http://localhost:5185/hangfire` en entorno de desarrollo.

---

### Manejo global de errores

El middleware `ErrorHandlingMiddleware` intercepta las excepciones no manejadas y retorna respuestas JSON con el siguiente formato:

```json
{ "title": "Descripcion del error", "message": "Detalle del error" }
```

| Excepcion | Codigo HTTP |
|---|---|
| `UnauthorizedAccessException` | 401 |
| `SecurityException` | 403 |
| `KeyNotFoundException` | 404 |
| `InvalidOperationException` | 400 |
| Otras | 500 |

---

### Logging

Serilog escribe logs en consola y en archivos rotativos diarios en la carpeta `logs/` (retencion de 14 dias). Niveles configurados en `appsettings.json`.

---

## Formato de respuesta de la API

Todos los endpoints retornan el siguiente formato estandar:

```json
{
  "message": "Descripcion de la operacion",
  "data": { }
}
```

Los errores retornan:

```json
{
  "title": "Tipo de error",
  "message": "Descripcion del error"
}
```

### Reiniciar la base de datos (eliminar todos los datos)

```bash
docker-compose down -v
docker-compose up -d
dotnet ef database update
```

### Listar volúmenes de Docker

```bash
docker volume ls
```

## 📂 Estructura del proyecto

```
backend-PIS/
├── backend/
│   ├── src/
│   │   ├── API/
│   │   │   ├── Controllers/
│   │   │   └── Middlewares/
│   │   ├── Application/
│   │   │   ├── DTOs/
│   │   │   ├── Services/
│   │   │   └── Validators/
│   │   ├── Domain/
│   │   │   └── Models/
│   │   └── Infrastructure/
│   │       ├── Data/
│   │       └── Repositories/
│   ├── Migrations/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
├── docker-compose.yml
└── README.md
```

## 📦 Modelo de base de datos

El modelo de base de datos (en formato Visual Paradigm .vpp) se encuentra en:  
`/database_diagram/PIS.vpp`
Cabe destacar que este modelo esta desactualizado, y sirve mas que nada como referencia historica. El modelo actual se refleja en las entidades de la capa de dominio (`Domain/Models`).

## 🔒 Seguridad

- ⚠️ **NO** subas archivos `appsettings.Development.json` o `appsettings.Production.json` al repositorio
- Las credenciales de desarrollo en Docker son solo para entorno local
- Para producción, usa variables de entorno o servicios de secretos

## 📝 Endpoints de la API

Consulta la documentación completa de endpoints en:  
`/backend/API_ENDPOINTS.md`

## Uso de Resend

Consulta la documentacion completa para el uso correcto de Resend con el plan gratuito en:
`/backend/RESEND_README.md`

## 🧠 Autores

Estudiantes de Proyecto Integrador Software II-2025  
Proyecto académico - Universidad Católica del Norte  
Facultad de Ingeniería y Ciencias Geológicas

## 📄 Licencia

Este proyecto no es de uso comercial, fue desarrollado exclusivamente para uso por la Federacion de Estudiantes de la Universidad Católica del Norte (FEUCN) y no debe ser utilizado por terceros sin autorización expresa.
