# SuperBodega - Sistema de Gestión de Inventario y Ventas

## Descripción General del Proyecto

SuperBodega es un sistema integrado de gestión de inventario y ventas diseñado para optimizar las operaciones de tiendas y bodegas comerciales. El sistema permite administrar productos, proveedores, clientes, ventas, compras y generar reportes detallados. La plataforma está basada en una arquitectura de microservicios con una API backend y una interfaz de administración basada en Razor, además de un componente para comercio electrónico.

El sistema implementa un flujo completo de gestión comercial, desde el registro de productos y proveedores, hasta la gestión de ventas y envío de notificaciones automáticas a clientes.

## Equipo de Desarrollo

### Lista de Cotejo

- **Luis Carlos Lima Pérez** - Gestión de compras, ventas, pruebas en postman y pruebas de carga con grafana k6
- **Angelica María Mejía Tzoc** - Gestión de clientes y manejo de notificaciones de las ventas por correo con RabbitMQ
- **Mynor Ebenezer Alonso Miranda** - Gestión de proveedores y reportes de las ventas
- **Josué Emanuel Ramírez Aquino** - Gestión de productos y carrito de compras en el ecommerce
- **Josseline Emerita Galeano Hernández** - Gestión de categorías y el catalogo de los productos con el filtro de categorías

## Arquitectura del Sistema

### Componentes Principales

1. **API Backend** - Desarrollado en ASP.NET Core, proporciona todos los endpoints necesarios para la gestión del negocio.
2. **Panel de Administración** - Interfaz web basada en Razor para la administración del sistema.
3. **Base de Datos** - SQL Server para el almacenamiento de datos persistente.
4. **Sistema de Notificaciones** - Basado en RabbitMQ para el manejo de mensajes asíncronos y notificaciones por email.
5. **Sistema de Reportes** - Generación de reportes en PDF y Excel para análisis de datos.

### Stack Tecnológico

- **Backend**: .NET 9.0, ASP.NET Core, Entity Framework Core 7.0
- **Frontend**: Razor Pages, JavaScript, Bootstrap 5
- **Base de Datos**: Microsoft SQL Server 2022
- **Contenedorización**: Docker & Docker Compose
- **Cola de Mensajes**: RabbitMQ 3.x
- **Documentación API**: Swagger/OpenAPI
- **Generación de Reportes**: QuestPDF, EPPlus (Excel)
- **Pruebas**: xUnit y Grafana k6

## Configuración y Despliegue

### Requisitos Previos

- Docker y Docker Compose
- .NET SDK 9.0 o superior (para desarrollo local)
- Visual Studio 2022 o VS Code (recomendado)
- SQL Server Management Studio (opcional, para administración de base de datos)

### Configuración del Entorno

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/tu-usuario/SuperBodega.git
   cd SuperBodega
   ```

2. Crear archivo `.env` en el directorio raíz con las siguientes variables:
   ```
   # Configuración de Base de Datos
   DATABASE_NAME_ONE=SuperBodegaDB_1
   DATABASE_NAME_TWO=SuperBodegaDB_2
   DATABASE_NAME_THREE=SuperBodegaDB_3
   DATABASE_NAME_FOUR=SuperBodegaDB_4
   DATABASE_NAME_FIVE=SuperBodegaDB_5
   DATABASE_PASSWORD_ONE=ClaveSegura1!
   DATABASE_PASSWORD_TWO=ClaveSegura2!
   DATABASE_PASSWORD_THREE=ClaveSegura3!
   DATABASE_PASSWORD_FOUR=ClaveSegura4!
   DATABASE_PASSWORD_FIVE=ClaveSegura5!
   
   # Configuración RabbitMQ
   RABBITMQ_HOST=rabbitmq
   RABBITMQ_USER=superbodega
   RABBITMQ_PASSWORD=SuperbodegaRMQ2025!
   RABBITMQ_QUEUE_NAME=notificaciones_email
   
   # Configuración Email
   EMAIL_SMTP_HOST=smtp.example.com
   EMAIL_SMTP_PORT=587
   EMAIL_SMTP_USERNAME=tu-usuario-smtp
   EMAIL_SMTP_PASSWORD=tu-contraseña-smtp
   EMAIL_FROM_EMAIL=notificaciones@example.com
   EMAIL_FROM_NAME=SuperBodega
   ```

### Despliegue con Docker

1. Construir e iniciar los contenedores:
   ```bash
   docker-compose up -d
   ```

2. Acceder a los servicios:
    - **API y Panel de Administración**: http://localhost:8080
    - **Documentación Swagger**: http://localhost:8080/swagger
    - **Panel de Administración RabbitMQ**: http://localhost:15672 (usuario/contraseña del archivo .env)
    - **SQL Server**: localhost,1433 (accesible mediante SQL Server Management Studio u otras herramientas)

3. Para detener la aplicación:
   ```bash
   docker-compose down
   ```

### Desarrollo Local

Para desarrollo sin Docker:

1. Actualizar la cadena de conexión en `appsettings.Development.json`
2. Ejecutar la aplicación:
   ```bash
   cd SuperBodega/SuperBodega.API
   dotnet run
   ```

### Estructura de Directorios

```
SuperBodega/
├── docker-compose.yml    # Configuración de servicios Docker
├── README.md             # Documentación del proyecto
└── SuperBodega/          # Código fuente
    ├── SuperBodega.API/  # Proyecto principal API y frontend
    │   ├── Controllers/  # Controladores API y vistas
    │   ├── Data/         # Contexto de base de datos y migraciones
    │   ├── DTOs/         # Objetos de transferencia de datos
    │   ├── Models/       # Modelos de datos
    │   ├── Repositories/ # Repositorios para acceso a datos
    │   ├── Services/     # Servicios de negocio
    │   ├── Views/        # Vistas Razor
    │   └── wwwroot/      # Recursos estáticos
    │   └── DockerFile    # Configuración del contenedor de Docker
    │   └── Program.cs    # Configuración de los servicios principales de la API
    └── TestxUnitSuperBodega/ # Proyecto de pruebas unitarias con PostMan y pruebas de carga con Grafana k6
```

## Modelo de Datos

### Entidades Principales

#### Categoria
- **Id** (int, PK) - Identificador único de la categoría
- **Nombre** (string) - Nombre de la categoría
- **Descripcion** (string) - Descripción de la categoría
- **Estado** (bool) - Estado activo/inactivo de la categoría
- **FechaDeRegistro** (DateTime) - Fecha de creación

#### Producto
- **Id** (int, PK) - Identificador único del producto
- **Codigo** (string) - Código único del producto (SKU)
- **Nombre** (string) - Nombre del producto
- **Descripcion** (string) - Descripción detallada
- **CategoriaId** (int, FK) - Referencia a Categoría
- **Stock** (int) - Cantidad en inventario
- **PrecioDeCompra** (decimal) - Precio de adquisición
- **PrecioDeVenta** (decimal) - Precio de venta al público
- **Estado** (bool) - Estado activo/inactivo del producto
- **Imagen** (string/binary) - Imagen del producto (ruta o datos)
- **FechaDeRegistro** (DateTime) - Fecha de creación

#### Proveedor
- **Id** (int, PK) - Identificador único del proveedor
- **Nombre** (string) - Nombre del proveedor
- **Email** (string) - Correo electrónico
- **Telefono** (string) - Número telefónico
- **Direccion** (string) - Dirección física
- **Estado** (bool) - Estado activo/inactivo
- **FechaDeRegistro** (DateTime) - Fecha de creación

#### Cliente
- **Id** (int, PK) - Identificador único del cliente
- **Nombre** (string) - Nombre del cliente
- **Apellido** (string) - Apellido del cliente
- **Email** (string) - Correo electrónico
- **Telefono** (string) - Número telefónico
- **Direccion** (string) - Dirección física
- **Estado** (bool) - Estado activo/inactivo
- **FechaDeRegistro** (DateTime) - Fecha de creación

#### Compra
- **Id** (int, PK) - Identificador único de la compra
- **NumeroDeFactura** (string) - Número de factura
- **IdProveedor** (int, FK) - Referencia al proveedor
- **FechaDeRegistro** (DateTime) - Fecha de la compra
- **MontoTotal** (decimal) - Monto total de la compra

#### DetalleDeLaCompra
- **Id** (int, PK) - Identificador único del detalle
- **IdCompra** (int, FK) - Referencia a la compra
- **IdProducto** (int, FK) - Referencia al producto
- **PrecioDeCompra** (decimal) - Precio de compra unitario
- **PrecioDeVenta** (decimal) - Precio de venta sugerido
- **Cantidad** (int) - Cantidad adquirida
- **MontoTotal** (decimal) - Monto total (precio × cantidad)
- **FechaDeRegistro** (DateTime) - Fecha de registro

#### EstadoDeLaVenta
- **Id** (int, PK) - Identificador único del estado
- **Nombre** (string) - Nombre del estado (Recibida, Despachada, Entregada, etc.)

#### Venta
- **Id** (int, PK) - Identificador único de la venta
- **NumeroDeFactura** (string) - Número de factura
- **NombreDelCliente** (string) - Nombre del cliente
- **ApellidoDelCliente** (string) - Apellido del cliente
- **NombreDelProducto** (string) - Nombre del producto
- **ImagenDelProducto** (binary) - Imagen del producto
- **MontoDePago** (decimal) - Monto recibido
- **MontoDeCambio** (decimal) - Cambio entregado
- **MontoTotal** (decimal) - Monto total de la venta
- **IdEstadoDeLaVenta** (int, FK) - Estado actual de la venta
- **FechaDeRegistro** (DateTime) - Fecha de la venta

#### DetalleDeLaVenta
- **Id** (int, PK) - Identificador único del detalle
- **IdVenta** (int, FK) - Referencia a la venta
- **IdEstadoDeLaVenta** (int, FK) - Estado de la línea
- **IdProveedor** (int, FK) - Proveedor del producto
- **NombreDelProveedor** (string) - Nombre del proveedor
- **IdProducto** (int, FK) - Referencia al producto
- **CodigoDelProducto** (string) - Código del producto
- **NombreDelProducto** (string) - Nombre del producto
- **ImagenDelProducto** (binary) - Imagen del producto
- **PrecioDeVenta** (decimal) - Precio de venta unitario
- **Cantidad** (int) - Cantidad vendida
- **Subtotal** (decimal) - Subtotal (precio × cantidad)
- **FechaDeRegistro** (DateTime) - Fecha de registro

### Entidades de E-commerce

#### Carrito
- **Id** (int, PK) - Identificador único del carrito
- **ClienteId** (int, FK) - Referencia al cliente propietario del carrito
- **FechaCreacion** (DateTime) - Fecha de creación del carrito
- **FechaActualizacion** (DateTime) - Fecha de última actualización
- **Estado** (bool) - Estado activo del carrito
- **TotalItems** (int, calculated) - Cantidad total de productos en el carrito
- **TotalMonto** (decimal, calculated) - Monto total calculado del carrito

#### ElementoCarrito
- **Id** (int, PK) - Identificador único del elemento
- **CarritoId** (int, FK) - Referencia al carrito
- **ProductoId** (int, FK) - Referencia al producto
- **Cantidad** (int) - Cantidad del producto en el carrito
- **PrecioUnitario** (decimal) - Precio unitario del producto al momento de agregar
- **Subtotal** (decimal, calculated) - Subtotal calculado (precio × cantidad)
- **FechaAgregado** (DateTime) - Fecha cuando se agregó el producto
- **FechaActualizado** (DateTime) - Fecha de última actualización de cantidad

#### Notificacion
- **Id** (int, PK) - Identificador único de la notificación
- **ClienteId** (int, FK) - Referencia al cliente destinatario
- **VentaId** (int, FK) - Referencia a la venta relacionada
- **Titulo** (string) - Título de la notificación
- **Mensaje** (string) - Contenido del mensaje
- **TipoNotificacion** (string) - Tipo de notificación (VentaCreada, EstadoCambiado, DevolucionSolicitada, etc.)
- **Leida** (bool) - Indica si la notificación ha sido leída
- **FechaCreacion** (DateTime) - Fecha de creación de la notificación
- **FechaLeida** (DateTime?) - Fecha cuando fue marcada como leída (nullable)

#### NotificacionEmail
- **Id** (int, PK) - Identificador único de la notificación de email
- **Para** (string) - Dirección de email del destinatario
- **Asunto** (string) - Asunto del email
- **Contenido** (string) - Contenido HTML del email
- **IdVenta** (int, FK) - Referencia a la venta
- **NumeroDeFactura** (string) - Número de factura de la venta
- **EstadoDeLaVenta** (string) - Estado actual de la venta
- **NombreCliente** (string) - Nombre completo del cliente
- **EmailCliente** (string) - Email del cliente
- **MontoTotal** (decimal) - Monto total de la venta
- **FechaDeRegistro** (DateTime) - Fecha de creación del email
- **Enviado** (bool) - Indica si el email fue enviado exitosamente
- **FechaEnvio** (DateTime?) - Fecha de envío del email (nullable)

#### DetalleNotificacionEmail
- **Id** (int, PK) - Identificador único del detalle
- **NotificacionEmailId** (int, FK) - Referencia a la notificación de email
- **NombreProducto** (string) - Nombre del producto
- **CodigoProducto** (string) - Código del producto
- **ImagenUrl** (string) - URL de la imagen del producto
- **Cantidad** (int) - Cantidad del producto en la venta
- **PrecioUnitario** (decimal) - Precio unitario del producto
- **Subtotal** (decimal) - Subtotal del producto (precio × cantidad)

### Relaciones entre Entidades

#### Relaciones del Módulo Administrativo
- **Categoria** → **Producto** (1:N)
- **Proveedor** → **Producto** (N:M)
- **Proveedor** → **Compra** (1:N)
- **Cliente** → **Venta** (1:N)
- **Compra** → **DetalleDeLaCompra** (1:N)
- **Producto** → **DetalleDeLaCompra** (1:N)
- **EstadoDeLaVenta** → **Venta** (1:N)
- **Venta** → **DetalleDeLaVenta** (1:N)
- **Producto** → **DetalleDeLaVenta** (1:N)
- **Proveedor** → **DetalleDeLaVenta** (1:N)

#### Relaciones del Módulo E-commerce
- **Cliente** → **Carrito** (1:1) - Cada cliente tiene un carrito activo
- **Carrito** → **ElementoCarrito** (1:N) - Un carrito puede tener múltiples elementos
- **Producto** → **ElementoCarrito** (1:N) - Un producto puede estar en múltiples carritos
- **Cliente** → **Notificacion** (1:N) - Un cliente puede tener múltiples notificaciones
- **Venta** → **Notificacion** (1:N) - Una venta puede generar múltiples notificaciones
- **Venta** → **NotificacionEmail** (1:N) - Una venta puede generar múltiples emails
- **NotificacionEmail** → **DetalleNotificacionEmail** (1:N) - Un email puede tener múltiples detalles de productos

#### Relaciones entre Módulos
- **Cliente** (Admin) → **Carrito** (E-commerce) (1:1)
- **Cliente** (Admin) → **Notificacion** (E-commerce) (1:N)
- **Producto** (Admin) → **ElementoCarrito** (E-commerce) (1:N)
- **Venta** (Admin) → **Notificacion** (E-commerce) (1:N)
- **Venta** (Admin) → **NotificacionEmail** (E-commerce) (1:N)

### Creación de Tablas

Las tablas se crean automáticamente al iniciar la aplicación utilizando Entity Framework Core con migraciones. El contexto `SuperBodegaContext` define todas las entidades y sus relaciones.

## API y Endpoints

La API de SuperBodega proporciona endpoints para todas las operaciones del sistema. A continuación se detallan los principales endpoints organizados por categoría.

### Endpoints de Productos

- `GET /api/Producto/GetAll` - Obtiene todos los productos
- `GET /api/Producto/{id}` - Obtiene un producto por ID
- `GET /api/Producto/ByCategoria/{id}` - Obtiene productos por categoría
- `POST /api/Producto/Create` - Crea un nuevo producto
- `PUT /api/Producto/Edit/{id}` - Actualiza un producto existente
- `DELETE /api/Producto/Delete/{id}` - Elimina un producto
- `POST /api/Producto/UpdateStock` - Actualiza el stock de un producto

### Endpoints de Categorías

- `GET /api/Categoria/GetAll` - Obtiene todas las categorías
- `GET /api/Categoria/{id}` - Obtiene una categoría por ID
- `POST /api/Categoria/Create` - Crea una nueva categoría
- `PUT /api/Categoria/Edit/{id}` - Actualiza una categoría existente
- `DELETE /api/Categoria/Delete/{id}` - Elimina una categoría

### Endpoints de Proveedores

- `GET /api/Proveedor/GetAll` - Obtiene todos los proveedores
- `GET /api/Proveedor/{id}` - Obtiene un proveedor por ID
- `POST /api/Proveedor/Create` - Crea un nuevo proveedor
- `PUT /api/Proveedor/Edit/{id}` - Actualiza un proveedor existente
- `DELETE /api/Proveedor/Delete/{id}` - Elimina un proveedor

### Endpoints de Clientes

- `GET /api/Cliente/GetAll` - Obtiene todos los clientes
- `GET /api/Cliente/{id}` - Obtiene un cliente por ID
- `GET /api/Cliente/ByEmail/{email}` - Busca un cliente por email
- `POST /api/Cliente/Create` - Crea un nuevo cliente
- `PUT /api/Cliente/Edit/{id}` - Actualiza un cliente existente
- `DELETE /api/Cliente/Delete/{id}` - Elimina un cliente

### Endpoints de Compras

- `GET /api/Compra/GetAll` - Obtiene todas las compras
- `GET /api/Compra/{id}` - Obtiene una compra por ID
- `GET /api/Compra/GetWithDetails` - Obtiene compras con detalles
- `POST /api/Compra/Create` - Crea una nueva compra
- `PUT /api/Compra/Edit/{id}` - Actualiza una compra existente
- `DELETE /api/Compra/Delete{id}` - Elimina una compra

### Endpoints de Ventas

- `GET /api/Venta/GetAll` - Obtiene todas las ventas
- `GET /api/Venta/{id}` - Obtiene una venta por ID
- `GET /api/Venta/GetWithDetails` - Obtiene ventas con detalles
- `GET /api/Venta/ByCliente/{id}` - Obtiene ventas por cliente
- `GET /api/Venta/ByEstado/{id}` - Obtiene ventas por estado
- `POST /api/Venta/Create` - Crea una nueva venta
- `PUT /api/Venta/estado/edit/{id}` - Cambia el estado de una venta
- `POST /api/Venta/devolucion/{id}` - Procesa una devolución
- `DELETE /api/Venta/Delete/{id}` - Elimina una venta

### Endpoints de Estados de Venta

- `GET /api/EstadoDeLaVenta/GetAll` - Obtiene todos los estados de venta
- `GET /api/EstadoDeLaVenta/{id}` - Obtiene un estado por ID
- `POST /api/EstadoDeLaVenta/Create` - Crea un nuevo estado
- `PUT /api/EstadoDeLaVenta/Edit/{id}` - Actualiza un estado existente
- `DELETE /api/EstadoDeLaVenta/Delete/{id}` - Elimina un estado

### Endpoints de E-commerce

#### Catálogo de Productos (ProductoCatalogoController)
- `GET /api/ecommerce/ProductoCatalogo` - Obtiene todos los productos del catálogo con paginación
    - **Parámetros de consulta**: `page`, `pageSize`, `search`
    - **Respuesta**: Lista paginada de productos con información de categoría e imagen
- `GET /api/ecommerce/ProductoCatalogo/categoria/{categoriaId}` - Obtiene productos por categoría con paginación
    - **Parámetros de consulta**: `page`, `pageSize`, `search`
    - **Respuesta**: Lista paginada de productos filtrados por categoría
- `GET /api/ecommerce/ProductoCatalogo/categorias` - Obtiene todas las categorías activas para el catálogo
    - **Respuesta**: Lista de categorías disponibles para filtros

#### Carrito de Compras (CarritoController)
- `GET /api/ecommerce/Carrito/{clienteId}` - Obtiene el carrito completo de un cliente
    - **Respuesta**: Cliente, elementos del carrito con detalles de productos, conteo y total
- `GET /api/ecommerce/Carrito/count/{clienteId}` - Obtiene la cantidad de items en el carrito
    - **Respuesta**: Número de elementos en el carrito
- `GET /api/ecommerce/Carrito/clientes` - Obtiene todos los clientes activos para selección
    - **Respuesta**: Lista de clientes disponibles para asignar carrito
- `POST /api/ecommerce/Carrito` - Agrega un producto al carrito
    - **Body**: `{ clienteId, productoId, cantidad }`
    - **Respuesta**: Confirmación de elemento agregado
- `PUT /api/ecommerce/Carrito/Edit/{elementoId}` - Actualiza la cantidad de un elemento del carrito
    - **Body**: `{ cantidad }`
    - **Respuesta**: Nueva cantidad y subtotal actualizado
- `DELETE /api/ecommerce/Carrito/Delete/{elementoId}` - Elimina un elemento específico del carrito
    - **Respuesta**: Confirmación de eliminación
- `DELETE /api/ecommerce/Carrito/clear/client/{clienteId}` - Vacía completamente el carrito de un cliente
    - **Respuesta**: Confirmación de carrito limpiado

#### Notificaciones (NotificacionController)
- `GET /api/Notificacion/cliente/{clienteId}` - Obtiene todas las notificaciones de un cliente
    - **Respuesta**: Lista de notificaciones con estado de lectura y detalles
- `POST /api/Notificacion/enviar/{ventaId}` - Envía notificación de cambio de estado de venta
    - **Respuesta**: Confirmación de notificación enviada
- `POST /api/Notificacion/marcar-leida/{id}` - Marca una notificación como leída
    - **Respuesta**: Confirmación de notificación marcada como leída

### Endpoints de Reportes

- `GET /api/Reporte/Ventas/Periodo` - Informe de ventas por período
- `GET /api/Reporte/Ventas/Producto` - Informe de ventas por producto
- `GET /api/Reporte/Ventas/Cliente` - Informe de ventas por cliente
- `GET /api/Reporte/Ventas/Proveedor` - Informe de ventas por proveedor

### Endpoints de Validaciones

Los endpoints de validaciones permiten verificar la existencia de ventas activas para diferentes entidades del sistema, ayudando a prevenir eliminaciones que podrían comprometer la integridad referencial.

#### Validaciones de Ventas Activas (ValidacionesController)
- `GET /api/Validaciones/producto/{id}/tieneVentasActivas` - Verifica si un producto tiene ventas activas
    - **Parámetros**: `id` (int) - ID del producto a verificar
    - **Respuesta**: `{ "tieneVentasActivas": boolean }`
    - **Uso**: Validar antes de eliminar o desactivar un producto

- `GET /api/Validaciones/categoria/{id}/tieneVentasActivas` - Verifica si una categoría tiene ventas activas
    - **Parámetros**: `id` (int) - ID de la categoría a verificar
    - **Respuesta**: `{ "tieneVentasActivas": boolean }`
    - **Uso**: Validar antes de eliminar o desactivar una categoría

- `GET /api/Validaciones/cliente/{id}/tieneVentasActivas` - Verifica si un cliente tiene ventas activas
    - **Parámetros**: `id` (int) - ID del cliente a verificar
    - **Respuesta**: `{ "tieneVentasActivas": boolean }`
    - **Uso**: Validar antes de eliminar o desactivar un cliente

- `GET /api/Validaciones/proveedor/{id}/tieneVentasActivas` - Verifica si un proveedor tiene ventas activas
    - **Parámetros**: `id` (int) - ID del proveedor a verificar
    - **Respuesta**: `{ "tieneVentasActivas": boolean }`
    - **Uso**: Validar antes de eliminar o desactivar un proveedor

### Documentación Swagger

La documentación completa de la API está disponible a través de Swagger/OpenAPI en la ruta `/swagger` cuando la aplicación está en ejecución. Incluye todos los endpoints, modelos de datos, parámetros requeridos y posibles respuestas.

## Estructura del Frontend

La interfaz de usuario está construida utilizando ASP.NET Core MVC con vistas Razor. A continuación se detalla la organización de la interfaz de usuario.

### Layout Principal

- `/Views/Shared/_DashboardLayout.cshtml` - Layout principal con menú lateral de navegación, header y contenedor principal

### Dashboard Principal

- `/Views/Dashboard/AdminDashboard.cshtml` - Panel de control con resumen de ventas diarias/semanales/mensuales, productos con bajo stock, últimas ventas y gráficos de ventas por período

### Módulos Principales

Cada módulo sigue un patrón consistente con las siguientes vistas:

#### 1. Módulo de Categorías
- `/Views/CategoriaView/Index.cshtml` - Lista de categorías con filtros y paginación
- `/Views/CategoriaView/Create.cshtml` - Formulario de creación
- `/Views/CategoriaView/Edit.cshtml` - Formulario de edición

#### 2. Módulo de Productos
- `/Views/ProductoView/Index.cshtml` - Lista de productos con filtros y paginación
- `/Views/ProductoView/Create.cshtml` - Formulario de creación
- `/Views/ProductoView/Edit.cshtml` - Formulario de edición

Características:
- Tabla con ordenamiento y búsqueda
- Carga de imágenes
- Selección de categoría
- Gestión de stock

#### 3. Módulo de Proveedores
- `/Views/ProveedorView/Index.cshtml` - Lista de proveedores con filtros y paginación
- `/Views/ProveedorView/Create.cshtml` - Formulario de creación
- `/Views/ProveedorView/Edit.cshtml` - Formulario de edición

#### 4. Módulo de Clientes
- `/Views/ClienteView/Index.cshtml` - Lista de clientes con filtros y paginación
- `/Views/ClienteView/Create.cshtml` - Formulario de creación
- `/Views/ClienteView/Edit.cshtml` - Formulario de edición

#### 5. Módulo de Compras
- `/Views/CompraView/Index.cshtml` - Lista de compras con filtros y paginación
- `/Views/CompraView/Create.cshtml` - Formulario de creación
- `/Views/CompraView/Edit.cshtml` - Formulario de edición
- `/Views/CompraView/Details.cshtml` - Detalles de la compra
- `/Views/CompraView/_ProductoSeleccion.cshtml` - Selección de producto (ventana modal)
- `/Views/CompraView/_ProveedorSeleccion.cshtml` - Selección de proveedor (ventana modal)

Características:
- Selección dinámica de productos
- Selección dinámica de proveedores
- Cálculo automático de totales

#### 6. Módulo de Ventas
- `/Views/VentaView/Index.cshtml` - Lista de ventas con filtros y paginación
- `/Views/VentaView/Details.cshtml` - Detalles de la venta
- `/Views/VentaView/_CambioEstado.cshtml` - Cambio de estado de venta (ventana modal)

Características:
- Sistema tipo POS para nueva venta
- Gestión de estados
- Integración con notificaciones

#### 7. Módulo de Reportes
- `/Views/ReporteView/VentasPorPeriodo.cshtml` - Selección de tipo de reporte según el período
- `/Views/ReporteView/VentasPorCliente.cshtml` - Selección de tipo de reporte según el cliente
- `/Views/ReporteView/VentasPorProducto.cshtml` - Selección de tipo de reporte según el producto
- `/Views/ReporteView/VentasPorProveedor.cshtml` - Selección de tipo de reporte según el proveedor

Características:
- Filtros por fecha, cliente, producto o proveedor
- Exportación a PDF/Excel

### Módulos del E-commerce

El sistema SuperBodega incluye un completo módulo de e-commerce con las siguientes vistas para los clientes:

#### 1. Catálogo de Productos
- `/Views/ProductoCatalogoView/Index.cshtml` - Página principal del catálogo con todos los productos
- `/Views/ProductoCatalogoView/Categorias.cshtml` - Vista de todas las categorías disponibles
- `/Views/ProductoCatalogoView/Categoria.cshtml` - Vista de productos filtrados por categoría específica

Características:
- Filtrado por categorías
- Búsqueda de productos
- Visualización de imágenes de productos
- Información de precios y stock
- Botón de agregar al carrito
- Navegación paginada

#### 2. Carrito de Compras
- `/Views/CarritoView/Index.cshtml` - Vista del carrito de compras del cliente

Características:
- Lista de productos en el carrito
- Actualización de cantidades
- Eliminación de productos
- Cálculo automático de totales
- Botón para proceder al checkout
- Resumen de compra

#### 3. Proceso de Compra
- `/Views/RealizarCompraView/Index.cshtml` - Vista del proceso de checkout y finalización de compra

Características:
- Formulario de información de envío
- Resumen de productos y precios
- Selección de método de pago
- Confirmación de pedido
- Integración con sistema de notificaciones

#### 4. Mis Pedidos
- `/Views/MisPedidosView/Index.cshtml` - Vista del historial de pedidos del cliente

Características:
- Lista de pedidos realizados
- Estados de cada pedido (Recibida, Despachada, Entregada)
- Detalles de cada pedido
- Seguimiento de envíos
- Opción de solicitar devoluciones

#### 5. Notificaciones
- `/Views/NotificacionView/Index.cshtml` - Vista de notificaciones del cliente

Características:
- Lista de notificaciones recibidas
- Marcado de notificaciones como leídas
- Filtrado por estado (leídas/no leídas)
- Detalles de cada notificación

### Recursos Estáticos

- `/wwwroot/css/` - Archivos CSS para estilos
- `/wwwroot/js/` - Archivos JavaScript para funcionalidad dinámica
- `/wwwroot/images/productos/` - Imágenes de productos
- `/wwwroot/reportes/` - Reportes generados

### Controladores del Sistema

SuperBodega está organizado en controladores separados para la administración y el e-commerce:

#### Controladores de Administración (Admin)

1. **CategoriaController.cs** - API REST para gestión de categorías
2. **CategoriaViewController.cs** - Controlador de vistas para categorías
3. **ClienteController.cs** - API REST para gestión de clientes
4. **ClienteViewController.cs** - Controlador de vistas para clientes
5. **CompraController.cs** - API REST para gestión de compras
6. **CompraViewController.cs** - Controlador de vistas para compras
7. **DashboardController.cs** - Controlador para el dashboard administrativo
8. **EstadoDeLaVentaController.cs** - API REST para estados de venta
9. **ProductoController.cs** - API REST para gestión de productos
10. **ProductoViewController.cs** - Controlador de vistas para productos
11. **ProveedorController.cs** - API REST para gestión de proveedores
12. **ProveedorViewController.cs** - Controlador de vistas para proveedores
13. **ReporteController.cs** - API REST para generación de reportes
14. **ReporteViewController.cs** - Controlador de vistas para reportes
15. **ValidacionesController.cs** - API REST para validaciones de datos
16. **VentaController.cs** - API REST para gestión de ventas
17. **VentaViewController.cs** - Controlador de vistas para ventas

#### Controladores de E-commerce

1. **CarritoController.cs** - API REST para gestión del carrito de compras
2. **CarritoViewController.cs** - Controlador de vistas para el carrito
3. **MisPedidosViewController.cs** - Controlador de vistas para historial de pedidos
4. **NotificacionController.cs** - API REST para notificaciones de clientes
5. **NotificacionViewController.cs** - Controlador de vistas para notificaciones
6. **ProductoCatalogoController.cs** - API REST para el catálogo de productos
7. **ProductoCatalogoViewController.cs** - Controlador de vistas para el catálogo
8. **RealizarCompraViewController.cs** - Controlador de vistas para el proceso de compra

### Funcionalidades Específicas por Módulo

#### Módulo de Carrito de Compras
- **Agregar productos**: Permite agregar productos al carrito con validación de stock
- **Actualizar cantidades**: Modificación de cantidades con recálculo automático
- **Eliminar productos**: Remoción individual o vaciado completo del carrito
- **Persistencia**: El carrito se mantiene por sesión/cliente
- **Validaciones**: Verificación de disponibilidad y stock en tiempo real

#### Módulo de Catálogo
- **Filtrado por categorías**: Navegación por categorías de productos
- **Búsqueda avanzada**: Búsqueda por nombre, descripción y código
- **Paginación**: Navegación eficiente de grandes catálogos
- **Imágenes**: Visualización de imágenes de productos con fallback
- **Información detallada**: Precios, stock, descripciones y especificaciones

#### Módulo de Notificaciones
- **Notificaciones en tiempo real**: Actualización del estado de pedidos
- **Historial completo**: Registro de todas las notificaciones recibidas
- **Marcado de lectura**: Sistema de gestión de notificaciones leídas/no leídas
- **Integración con email**: Envío automático de notificaciones por correo

#### Módulo de Pedidos
- **Seguimiento completo**: Visualización del estado actual de cada pedido
- **Historial detallado**: Acceso a todos los pedidos realizados
- **Gestión de devoluciones**: Solicitud y seguimiento de devoluciones
- **Detalles de productos**: Información completa de cada producto en el pedido

## Sistema de Notificaciones

SuperBodega incluye un sistema de notificaciones robusto para informar a los clientes sobre cambios en el estado de sus pedidos. Este sistema utiliza RabbitMQ para gestionar la cola de mensajes y envío asíncrono de emails.

### Componentes del Sistema de Notificaciones

1. **Servicio RabbitMQ** - Maneja la cola de mensajes para notificaciones
2. **Servicio de Email** - Gestiona el formato y envío de emails
3. **Worker en Background** - Procesa las notificaciones en cola de manera asíncrona
4. **Plantillas de Email** - Plantillas HTML personalizadas según el estado del pedido

### Flujo del Sistema de Notificaciones

1. Cuando se crea una venta o cambia su estado, se genera una notificación
2. La notificación se envía a una cola de RabbitMQ
3. Un servicio en background procesa la cola y envía emails vía SMTP
4. Los clientes reciben emails personalizados con los detalles y estado de su pedido

### Estructura de Notificaciones

```csharp
public class NotificacionEmail
{
    public string Para { get; set; }
    public string Asunto { get; set; }
    public string Contenido { get; set; }
    public int IdVenta { get; set; }
    public string NumeroDeFactura { get; set; }
    public string EstadoDeLaVenta { get; set; }
    public DateTime FechaDeRegistro { get; set; }
    public string NombreCliente { get; set; }
    public string EmailCliente { get; set; }
    public decimal MontoTotal { get; set; }
    public List<DetalleNotificacionEmail> Productos { get; set; }
}

public class DetalleNotificacionEmail
{
    public string NombreProducto { get; set; }
    public string CodigoProducto { get; set; }
    public string ImagenUrl { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
```

### Tipos de Notificaciones

Las notificaciones se personalizan según el estado del pedido:

1. **Recibida** (Color azul) - Confirmación de pedido recibido
2. **Despachada** (Color amarillo) - Aviso de que el pedido está en camino
3. **Entregada** (Color verde) - Confirmación de entrega exitosa
4. **Devolución Solicitada** (Color rojo) - Aviso de solicitud de devolución
5. **Devolución Completada** (Color gris) - Confirmación de devolución procesada

### Configuración

El sistema de notificaciones utiliza variables de entorno definidas en `.env` y `appsettings.json`. Las principales configuraciones incluyen:

```json
{
  "RabbitMQ": {
    "Host": "rabbitmq",
    "User": "superbodega",
    "Password": "SuperbodegaRMQ2025!"
  },
  "Email": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "tu-smtp-username",
    "SmtpPassword": "tu-smtp-password",
    "FromEmail": "ventas@superbodega.com",
    "FromName": "SuperBodega"
  }
}
```

## Sistema de Reportes

SuperBodega incorpora un completo sistema de reportes que permite generar informes detallados sobre ventas, inventario, proveedores y clientes. Estos reportes pueden ser visualizados en línea o exportados en formatos PDF y Excel.

### Tipos de Reportes

#### 1. Reportes de Ventas
- **Reporte de Ventas por Período** - Ventas en un rango de fechas personalizable
- **Reporte de Ventas por Producto** - Análisis de ventas por producto
- **Reporte de Ventas por Cliente** - Historial de compras por cliente
- **Reporte de Ventas por Proveedor** - Productos vendidos por proveedor

### Tecnologías de Reportes

El sistema utiliza las siguientes tecnologías para la generación de reportes:

1. **QuestPDF** - Biblioteca para generación de reportes en formato PDF
2. **EPPlus** - Biblioteca para generación de reportes en formato Excel

### Acceso a los Reportes

Los reportes pueden accederse desde el módulo de reportes en el panel de administración. El usuario puede:

1. Seleccionar el tipo de reporte
2. Configurar los filtros específicos (fechas, productos, clientes, etc.)
3. Exportar el reporte a PDF o Excel
4. Programar generación automática de reportes recurrentes

## Sistema E-Commerce

SuperBodega incluye un módulo de E-commerce que permite a los clientes realizar compras en línea. Este módulo se integra con el sistema principal de inventario y ventas.

### Características Principales

1. **Catálogo de Productos** - Visualización del inventario disponible para venta
2. **Carrito de Compras** - Gestión de productos seleccionados
3. **Proceso de Checkout** - Flujo de finalización de compra
4. **Seguimiento de Pedidos** - Visualización del estado de los pedidos
5. **Historial de Compras** - Registro de compras anteriores

### Integración con Sistema Principal

El módulo de E-commerce se comunica con el sistema principal a través de la API, compartiendo:

1. **Base de datos** - Acceso a inventario, clientes y pedidos
2. **Gestión de inventario** - Actualización en tiempo real del stock
3. **Sistema de notificaciones** - Envío de confirmaciones de pedido
4. **Sistema de reportes** - Inclusión de ventas en línea en los reportes

## Seguridad del Sistema

SuperBodega implementa diversas medidas de seguridad para proteger la información del negocio y los clientes.

### Roles del Sistema

- **Administrador** - Acceso completo a todas las funcionalidades
- **Cliente** - Acceso al portal de E-commerce y sus pedidos

## Solución de Problemas

### Problemas Comunes y Soluciones

#### 1. Problemas de Conexión con la Base de Datos

**Síntomas**: Error "Cannot open database" o "Login failed"

**Soluciones**:
- Verificar que el contenedor de SQL Server esté en ejecución
- Comprobar la cadena de conexión en `appsettings.json`
- Verificar las credenciales en el archivo `.env`

```bash
# Ver estado de contenedores
docker-compose ps

# Reiniciar contenedor de base de datos
docker-compose restart sqlserver
```

#### 2. Problemas con RabbitMQ

**Síntomas**: Las notificaciones por email no se envían

**Soluciones**:
- Verificar que el contenedor de RabbitMQ esté en ejecución
- Comprobar credenciales en el archivo `.env`
- Revisar logs del servicio de RabbitMQ

```bash
# Ver logs de RabbitMQ
docker-compose logs rabbitmq

# Acceder al panel de administración
# URL: http://localhost:15672
# Usuario/Contraseña del archivo .env
```

#### 3. Problemas de Actualización de Inventario

**Síntomas**: El stock no se actualiza correctamente después de una venta

**Soluciones**:
- Verificar las transacciones en la base de datos
- Revisar los logs de la API en `/logs`
- Comprobar el funcionamiento del servicio `InventarioService`

```sql
-- Consulta para verificar el stock de un producto
SELECT Id, Codigo, Nombre, Stock FROM Producto WHERE Id = <id_producto>;
```

### Logs y Diagnóstico

El sistema registra logs detallados para facilitar el diagnóstico de problemas:

- **Logs de Aplicación**: `/logs/app-{fecha}.log` - Logs generales de la aplicación
- **Logs de API**: `/logs/api-{fecha}.log` - Registro de solicitudes y respuestas de API
- **Logs de Errores**: `/logs/error-{fecha}.log` - Registro detallado de excepciones
- **Logs de Docker**: Accesibles mediante `docker-compose logs`

## Pruebas Automatizadas

SuperBodega implementa un conjunto de pruebas automatizadas para garantizar la calidad y estabilidad del sistema.

### Pruebas de Integración con Postman

SuperBodega incluye una colección de Postman para probar los flujos de negocio principales, especialmente el sistema de notificaciones. Esta colección está ubicada en `SuperBodega/TestxUnitSuperBodega/superbodega-tests.json`.

#### Estructura de la Colección

La colección de Postman incluye pruebas para los siguientes escenarios:

1. **Creación de Ventas**
    - Creación sincrónica (estado "Recibida")
    - Creación asincrónica (estado "Recibida")

2. **Cambios de Estado**
    - Cambio sincrónico de "Recibida" a "Despachada"
    - Cambio sincrónico de "Despachada" a "Entregada"
    - Cambio asincrónico de "Recibida" a "Despachada"
    - Cambio asincrónico de "Despachada" a "Entregada"

3. **Gestión de Devoluciones**
    - Solicitud de devolución sincrónica
    - Procesamiento de devolución sincrónica
    - Solicitud de devolución asincrónica
    - Procesamiento de devolución asincrónica

#### Ejemplo de Solicitud Postman

```json
{
  "name": "Crear Venta - Sincrónica (Recibida)",
  "request": {
    "method": "POST",
    "body": {
      "raw": "{\n    \"NumeroDeFactura\": \"T-{{$randomInt}}\",\n    \"IdCliente\": 1,\n    \"MontoDePago\": 1500.00,\n    \"MontoDeCambio\": 100.00,\n    \"MontoTotal\": 1400.00,\n    \"Detalles\": [\n        {\n            \"idProducto\": 1,\n            \"nombreDelProducto\": \"Laptop\",\n            \"idProveedor\": 1,\n            \"nombreDelProveedor\": \"TechSupplier\",\n            \"cantidad\": 1,\n            \"precioDeVenta\": 999.99,\n            \"montoTotal\": 999.99\n        },\n        {\n            \"idProducto\": 2,\n            \"nombreDelProducto\": \"Smartphone\",\n            \"idProveedor\": 2,\n            \"nombreDelProveedor\": \"MobileWorld\",\n            \"cantidad\": 1,\n            \"precioDeVenta\": 400.01,\n            \"montoTotal\": 400.01\n        }\n    ],\n    \"UsarNotificacionSincronica\": true\n}"
    },
    "url": {
      "path": ["api", "Venta", "Create"]
    },
    "description": "Crea una nueva venta usando notificación sincrónica"
  }
}
```

#### Ejecución de Pruebas con Postman

Para ejecutar las pruebas de Postman:

1. Importar la colección `superbodega-tests.json` en Postman
2. Configurar las variables de entorno necesarias
3. Ejecutar la colección completa o pruebas específicas

### Pruebas de Carga y Rendimiento con K6

SuperBodega incluye scripts de prueba de carga implementados con K6 para evaluar el rendimiento del sistema bajo diferentes condiciones de carga. Los scripts están ubicados en `SuperBodega/TestxUnitSuperBodega/TestDeCarga/`.

#### Scripts de Prueba de Carga

1. **purchase-confirmation-test.js** - Prueba el rendimiento del proceso de confirmación de compras
2. **load-test-notification.js** - Evalúa el rendimiento del sistema de notificaciones
3. **stress-test-returns.js** - Prueba el sistema de devoluciones bajo carga

#### Métricas Evaluadas

Los scripts de K6 recopilan las siguientes métricas clave:

- **Tiempo de respuesta** - Para operaciones sincrónicas y asincrónicas
- **Tasa de éxito** - Porcentaje de solicitudes exitosas
- **Usuarios activos** - Número de usuarios concurrentes
- **Contador de errores** - Número de errores encontrados

#### Configuración de Pruebas de Carga

Cada script de prueba incluye una configuración que define los escenarios de carga:

```javascript
export const options = {
  scenarios: {
    purchase_test: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '5s', target: 5 },   // Rampa hasta 5 usuarios
        { duration: '15s', target: 5 },  // Mantén 5 usuarios
        { duration: '3s', target: 0 },   // Finaliza
      ],
      gracefulRampDown: '2s',
    },
  },
  thresholds: {
    'sync_purchase_time': ['p(95)<3000'],
    'async_purchase_time': ['p(95)<500'],
    'sync_success': ['rate>0.8'],
    'async_success': ['rate>0.8'],
    'errors': ['count<10'],
  },
};
```

#### Ejecución de Pruebas de Carga

Para ejecutar las pruebas de carga:

```bash
# Instalar k6
npm install -g k6

# Ejecutar una prueba específica
k6 run SuperBodega/TestxUnitSuperBodega/TestDeCarga/purchase-confirmation-test.js

# Ver resultados con formato detallado
k6 run --out json=results.json SuperBodega/TestxUnitSuperBodega/TestDeCarga/load-test-notification.js
```

#### Resultados y Análisis

Los resultados de las pruebas de carga pueden analizarse para identificar:

1. **Cuellos de botella** - Componentes con rendimiento insuficiente
2. **Capacidad máxima** - Número máximo de usuarios concurrentes soportados
3. **Impacto de la asincronía** - Comparación entre operaciones sincrónicas y asincrónicas
4. **Estabilidad** - Comportamiento del sistema bajo carga sostenida