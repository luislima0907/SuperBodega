# Programación III - 2025
## Grupo No.1
### Integrantes
- **Luis Carlos Lima Pérez**
- **Angelica María Mejía Tzoc**
- **Mynor Ebenezer Alonso Miranda**
- **Josué Emanuel Ramírez Aquino**
- **Josseline Emerita Galeano Hernández**

# Estructura API Principal del Negocio (Backend)

## Entidades

### 1. Categoría
**Ubicación**: `SuperBodega.API/Models/Admin/Categoria.cs`
**Propiedades**:
- Id (int, PK)
- Nombre (string)
- Descripcion (string)
- Estado (bool)
- FechaDeRegistro (DateTime)

Este es un ejemplo para crear la tabla de Categoria en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table CATEGORIA(
IdCategoria int primary key identity,
Descripcion varchar(100),
Estado bit,
FechaDeRegistro datetime default getdate()
)
```
**Relaciones**:
- Productos (1:N)

### 2. Producto
**Ubicación**: `SuperBodega.API/Models/Admin/Producto.cs`
**Propiedades**:
- Id (int, PK)
- Codigo (string)
- Nombre (string)
- Descripcion (string)
- CategoriaId (int, FK)
- Stock (int)
- PrecioDeCompra (decimal)
- PrecioDeVenta (decimal)
- Estado (bool)
- Imagen (string, ruta)
- FechaDeRegistro (DateTime)

Este es un ejemplo para crear la tabla de Producto en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table PRODUCTO(
IdProducto int primary key identity,
Codigo varchar(50),
Nombre varchar(50),
Descripcion varchar(50),
IdCategoria int references CATEGORIA(IdCategoria),
Stock int not null default 0,
PrecioDeCompra decimal(10,2) default 0,
PrecioDeVenta decimal(10,2) default 0,
Estado bit,
Imagen varbinay(max) null,
FechaDeRegistro datetime default getdate()
)
```

**Relaciones**:
- Categoría (1:1)
- Proveedores (N:M)
- DetallesVenta (1:N)
- DetallesCompra (1:N)

### 3. Proveedor
**Ubicación**: `SuperBodega.API/Models/Admin/Proveedor.cs`
**Propiedades**:
- Id (int, PK)
- Nombre (string)
- Email (string)
- Telefono (string)
- Direccion (string)
- Estado (bool)
- FechaDeRegistro (DateTime)

Este es un ejemplo para crear la tabla de Proveedor en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table PROVEEDOR(
IdProveedor int primary key identity,
Nombre varchar(50),
Email varchar(50),
Telefono varchar(50),
Direccion varchar(100),
Estado bit,
FechaDeRegistro datetime default getdate()
)
```

**Relaciones**:
- Productos (N:M)
- Compras (1:N)

### 4. Cliente
**Ubicación**: `SuperBodega.API/Models/Admin/Cliente.cs`
**Propiedades**:
- Id (int, PK)
- Nombre (string)
- Apellido (string)
- Email (string)
- Telefono (string)
- Direccion (string)
- Estado (bool)
- FechaDeRegistro (DateTime)

Este es un ejemplo para crear la tabla de Cliente en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table CLIENTE(
IdCliente int primary key identity,
Nombre varchar(50),
Apellido varchar(50),
Email varchar(50),
Telefono varchar(50),
Direccion varchar(100),
Estado bit,
FechaDeRegistro datetime default getdate()
)
```
**Relaciones**:
- Ventas (1:N)

### 5. Compra
**Ubicación**: `SuperBodega.API/Models/Admin/Compra.cs`
**Propiedades**:
- Id (int, PK)
- NumeroDeFactura (string)
- IdProveedor (int, FK)
- FechaDeRegistro (DateTime)
- Total (decimal)

Este es un ejemplo para crear la tabla de Compra en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table COMPRA(
IdCompra int primary key identity,
NumeroDeFactura varchar(50),
IdProveedor int references PROVEEDOR(IdProveedor),
FechaDeRegistro datetime default getdate(),
MontoTotal decimal(10,2)
)
```
**Relaciones**:
- Proveedor (N:1)
- DetallesCompra (1:N)

### 6. DetalleDeLaCompra
**Ubicación**: `SuperBodega.API/Models/Admin/DetalleDeLaCompra.cs`
**Propiedades**:
- Id (int, PK)
- IdCompra (int, FK)
- IdProducto (int, FK)
- PrecioDeCompra (decimal)
- PrecioDeVenta (decimal)
- Cantidad (int)
- MontoTotal (decimal)
- FechaDeRegistro (DateTime)

Este es un ejemplo para crear la tabla de DetalleDeLaCompra en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table DETALLE_DE_LA_COMPRA(
IdDetalleCompra int primary key identity,
IdCompra int references COMPRA(IdCompra),
IdProducto int references PRODUCTO(IdProducto),
PrecioDeCompra decimal(10,2) default 0,
PrecioDeVenta decimal(10,2) default 0,
Cantidad int,
MontoTotal decimal(10,2),
FechaRegistro datetime default getdate()
)
```

**Relaciones**:
- Compra (N:1)
- Producto (N:1)

### 7. EstadoDeLaVenta
**Ubicación**: `SuperBodega.API/Models/Admin/EstadoDeLaVenta.cs`
**Propiedades**:
- Id (int, PK)
- Nombre (string) - Pendiente, Procesada, Entregada, Cancelada

Este es un ejemplo para crear la tabla de EstadoDeLaVenta en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table ESTADO_DE_LA_VENTA(
IdEstado int primary key identity,
Nombre varchar(50)
)
```
**Relaciones**:
- Ventas (1:N)

### 8. Venta
**Ubicación**: `SuperBodega.API/Models/Admin/Venta.cs`
**Propiedades**:
- Id (int, PK)
- NumeroDeFactura (string)
- NombreDelCliente (string)
- MontoDePago (decimal)
- MotoDeCambio (decimal)
- MontoTotal (decimal)
- IdEstado (int, FK)
- FechaDeRegistro (DateTime)

Este es un ejemplo para crear la tabla de Venta en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table VENTA(
IdVenta int primary key identity,
NumeroDeFactura varchar(50),
NombreDelCliente varchar(100),
MontoDePago decimal(10,2),
MontoDeCambio decimal(10,2),
MontoTotal decimal(10,2),
IdEstadoDeLaVenta int references ESTADO_DE_LA_VENTA(IdEstado),
FechaDeRegistro datetime default getdate()
)

```
**Relaciones**:
- Cliente (N:1)
- Estado (N:1)
- DetallesVenta (1:N)

### 9. DetalleDeLaVenta
**Ubicación**: `SuperBodega.API/Models/Admin/DetalleDeLaVenta.cs`
**Propiedades**:
- Id (int, PK)
- IdVenta (int, FK)
- IdProducto (int, FK)
- PrecioDeVenta (decimal)
- Cantidad (int)
- Subtotal (decimal)
- FechaDeRegistro (DateTime)

Este es un ejemplo para crear la tabla de DetalleDeLaVenta en el SQL Server con Docker (esto debe crearse dentro de la API, no en el programa de SQL Server de manera local):
```SQL
create table DETALLE_DE_LA_VENTA(
IdDetalleVenta int primary key identity,
IdVenta int references VENTA(IdVenta),
IdProducto int references PRODUCTO(IdProducto),
PrecioDeVenta decimal(10,2),
Cantidad int,
SubTotal decimal(10,2),
FechaDeRegistro datetime default getdate()
)
```
**Relaciones**:
- Venta (N:1)
- Producto (N:1)

## Repositorios

### Interfaces
**Ubicación**: `SuperBodega.API/Repositories/Interfaces/Admin/`

1. **IGenericOperationsRepository.cs** (Base genérica para todos los repositorios)
   - GetAll(), GetById(), Create(), Update(), Delete()

2. **IProductoRepository.cs**
   - Métodos específicos: GetByCategoriaId(), UpdateStock()

3. **ICategoriaRepository.cs**
4. **IProveedorRepository.cs**
5. **IClienteRepository.cs**
    - Métodos específicos: GetByEmail()
6. **ICompraRepository.cs**
   - Métodos específicos: GetWithDetails()

7. **IVentaRepository.cs**
   - Métodos específicos: GetWithDetails(), GetByClienteId(), GetByEstadoId()

### Implementaciones
**Ubicación**: `SuperBodega.API/Services/Admin/`
- Implementación correspondiente para cada interfaz

## Servicios
**Ubicación**: `SuperBodega.API/Services/Admin/`

1. **ProductoService.cs**
   - CRUD completo
   - GetByCategoriaId()
   - UpdateStock()

2. **CategoriaService.cs**
   - CRUD completo

3. **ProveedorService.cs**
   - CRUD completo

4. **ClienteService.cs**
   - CRUD completo
   - GetByEmail()

5. **CompraService.cs**
   - CRUD completo
   - GetWithDetails()

6. **VentaService.cs**
   - CRUD completo
   - GetWithDetails()
   - GetByClienteId()
   - GetByEstadoId()
   - ChangeState()

7. **ReporteService.cs**
   - GenerateSalesReportByPeriod()
   - GenerateSalesReportByProduct()
   - GenerateSalesReportByCustomer()
   - GenerateSalesReportBySupplier()

## DTO (Data Transfer Objects)
**Ubicación**: `SuperBodega.API/DTOs/Admin/`

Para cada entidad principal:
- ProductoDTO, CreateProductoDTO, UpdateProductoDTO
- CategoriaDTO, CreateCategoriaDTO, UpdateCategoriaDTO
- ProveedorDTO...
- ClienteDTO...
- CompraDTO, CreateCompraDTO (con detalles)
- VentaDTO, CreateVentaDTO (con detalles)

Para reportes:
- ReporteVentasPorPeriodoDTO
- ReporteVentasPorProductoDTO
- ReporteVentasPorClienteDTO
- ReporteVentasPorProveedorDTO

## Controladores
**Ubicación**: `SuperBodega.API/Controllers/Admin/`

1. **ProductosController.cs**
   - GET: api/productos (todos)
   - GET: api/productos/{id} (por ID)
   - GET: api/productos/categoria/{id} (por categoría)
   - POST: api/productos (crear)
   - PUT: api/productos/{id} (actualizar)
   - DELETE: api/productos/{id} (eliminar)

2. **CategoriasController.cs**
   - Endpoints CRUD básicos

3. **ProveedoresController.cs**
   - Endpoints CRUD básicos

4. **ClientesController.cs**
   - Endpoints CRUD básicos

5. **ComprasController.cs**
   - Endpoints CRUD con filtros para proveedor

6. **VentasController.cs**
   - Endpoints CRUD
   - PUT: api/ventas/{id}/estado (cambiar estado)
   - GET: api/ventas/cliente/{id} (ventas por cliente)
   - GET: api/ventas/estado/{id} (ventas por estado)

7. **ReportesController.cs**
   - GET: api/reportes/ventas/periodo
   - GET: api/reportes/ventas/producto
   - GET: api/reportes/ventas/cliente
   - GET: api/reportes/ventas/proveedor

## DbContext
**Ubicación**: `SuperBodega.API/Data/SuperBodegaContext.cs`

Agregar cada DbSet correspondiente a las entidades y configurar las relaciones en el método OnModelCreating para que SQL Server mediante Docker
pueda generar las tablas para su uso en la base de datos.

# Estructura del Frontend con Razor para SuperBodega

## Estructura Base

### Directorios principales:
```
/Views
  /Shared
  /Dashboard
  /Productos
  /Categorias
  /Proveedores
  /Clientes
  /Compras
  /Ventas
  /Reportes
/wwwroot
  /css
  /js
  /img
  /lib
```

## Arquitectura del Dashboard

### Layout Principal
Ubicación: `/Views/Shared/_DashboardLayout.cshtml`

Este archivo contendrá:
- Estructura HTML base
- Menú lateral con todos los módulos
- Header con información de usuario
- Scripts y CSS comunes
- Contenedor principal para las vistas

## Módulos Específicos

### 1. Dashboard (Inicio)
Ubicación: `/Views/Dashboard/AdminDashboard.cshtml`

Elementos (esto esta implementado de manera de ejemplo, si da tiempo se puede implementar en la api final):
- Resumen de ventas diarias/semanales/mensuales
- Productos con bajo stock
- Últimas ventas
- Gráfico de ventas por período

### 2. Categorías
Archivos:
- `/Views/Categorias/Index.cshtml` - Lista con filtros y paginación, incluir las opciones para
  editar y eliminar la categoria seleccionada.
- `/Views/Categorias/Create.cshtml` - Formulario de creación
- `/Views/Categorias/Edit.cshtml` - Formulario de edición

### 3. Productos
Archivos:
- `/Views/Productos/Index.cshtml` - Lista con filtros y paginación, incluir las opciones para
editar y eliminar el producto seleccionado.
- `/Views/Productos/Create.cshtml` - Formulario de creación
- `/Views/Productos/Edit.cshtml` - Formulario de edición

Características:
- Tabla con ordenamiento y búsqueda
- Carga de imágenes
- Selección de categoría
- Gestión de stock (aumento/disminución) esto ultimo lo hara el sistema de compras(aumentar) y ventas(disminuir)

### 4. Proveedores
Archivos:
- `/Views/Proveedores/Index.cshtml` - Lista con filtros y paginación, incluir las opciones para
editar y eliminar el proveedor seleccionado.
- `/Views/Proveedores/Create.cshtml` - Formulario de creación
- `/Views/Proveedores/Edit.cshtml` - Formulario de edición

### 5. Clientes
Archivos:
- `/Views/Clientes/Index.cshtml` - Lista con filtros y paginación, incluir las opciones para
editar y eliminar el cliente seleccionado.
- `/Views/Clientes/Create.cshtml` - Formulario de creación
- `/Views/Clientes/Edit.cshtml` - Formulario de edición

### 6. Compras
Archivos:
- `/Views/Compras/Index.cshtml` - Lista con filtros y paginación, incluir las opciones para
editar y eliminar la compra seleccionada (tener cuidado con las validaciones del stock del producto
ya que van relacionadas en caso de editar o eliminar la compra).
- `/Views/Compras/Create.cshtml` - Formulario de creación
- `/Views/Compras/Edit.cshtml` - Formulario de edición
- `/Views/Compras/Details.cshtml` - Detalles de la compra
- `/Views/Compras/_ProductoSeleccion.cshtml` - Selección de producto (esto ira dentro del formulario de creación, pero en ventana emergente al dar click en el botón de agregar producto)
- `/Views/Compras/_ProveedorSeleccion.cshtml` - Selección de proveedor (esto ira dentro del formulario de creación, pero en ventana emergente al dar click en el botón de agregar proveedor)

Características:
- Selección dinámica de productos
- Selección dinámica de proveedores
- Cálculo automático de totales (tomando como base la tabla creada en SQL Server)

### 7. Ventas
Archivos:
- `/Views/Ventas/Index.cshtml` - Aqui caeran las compras que se realicen desde la api de ecommerce.
Lista con filtros y paginación, incluir la opción para cambiar el estado de la venta seleccionada. 
Ya que la venta como tal no se podra modificar ni eliminar. Porque esta vendra desde la api de ecommerce 
al momento de que un usuario haga una compra.
- `/Views/Ventas/Details.cshtml` - Detalles de la venta, se obtendran el detalle de las ventas por medio
de su numero de factura.
- `/Views/Ventas/_CambioEstado.cshtml` - Cambiar estado de venta, para notificarle el estado de la compra al usuario de
la api de ecommerce.

Características:
- Sistema tipo POS para nueva venta
- Gestión de estados

### 8. Reportes
Archivos:
- `/Views/Reportes/Index.cshtml` - Selección de tipo de reporte por medio de un filtro.

Características:
- Filtros por fecha
- Gráficos interactivos
- Exportación a PDF/Excel

## Archivos JavaScript y CSS

### CSS
- `/wwwroot/css/site.css` - Estilos generales
- `/wwwroot/css/dashboard.css` - Estilos del dashboard
- `/wwwroot/css/categorias.css` - Estilos de categorías
- `/wwwroot/css/productos.css` - Estilos de productos
- `/wwwroot/css/proveedores.css` - Estilos de proveedores
- `/wwwroot/css/clientes.css` - Estilos de clientes
- `/wwwroot/css/compras.css` - Estilos de compras
- `/wwwroot/css/ventas.css` - Estilos de ventas
- `/wwwroot/css/reportes.css` - Estilos de reportes

### JavaScript
- `/wwwroot/js/site.js` - Funciones generales
- `/wwwroot/js/dashboard.js` - Funciones del dashboard
- `/wwwroot/js/categorias.js` - Gestión de categorías
- `/wwwroot/js/productos.js` - Gestión de productos
- `/wwwroot/js/proveedores.js` - Gestión de proveedores
- `/wwwroot/js/clientes.js` - Gestión de clientes
- `/wwwroot/js/compras.js` - Sistema de compras
- `/wwwroot/js/ventas.js` - Sistema de ventas
- `/wwwroot/js/reportes.js` - Generación de gráficos

## Pasos de Implementación
1. Desarrollar los módulos CRUD en este orden:
### Joseeline
   - Categorías (dependencia de Productos)
   - Utilizar las variables de entorno en el archivo `appsettings.json` y `docker-compose.yml` en donde se define la conexion
     de la base de datos:
     - DATABASE_NAME_FOUR (para el nombre de base de datos personal) 
     - DATABASE_PASSWORD_FOUR (para la contraseña de la base de datos personal)
### Josue
   - Productos
   - Utilizar las variables de entorno en el archivo `appsettings.json` y `docker-compose.yml` en donde se define la conexion
     de la base de datos:
      - DATABASE_NAME_FIVE (para el nombre de base de datos personal)
      - DATABASE_PASSWORD_FIVE (para la contraseña de la base de datos personal)
### Mynor
   - Proveedores
   - Utilizar las variables de entorno en el archivo `appsettings.json` y `docker-compose.yml` en donde se define la conexion
     de la base de datos:
      - DATABASE_NAME_THREE (para el nombre de base de datos personal)
      - DATABASE_PASSWORD_THREE (para la contraseña de la base de datos personal)
### Angelica
   - Clientes
   - Utilizar las variables de entorno en el archivo `appsettings.json` y `docker-compose.yml` en donde se define la conexion
     de la base de datos:
      - DATABASE_NAME_TWO (para el nombre de base de datos personal)
      - DATABASE_PASSWORD_TWO (para la contraseña de la base de datos personal)
### Luis
   - Compras
   - Ventas
   - Utilizar las variables de entorno en el archivo `appsettings.json` y `docker-compose.yml` en donde se define la conexion
     de la base de datos:
      - DATABASE_NAME_ONE (para el nombre de base de datos personal)
      - DATABASE_PASSWORD_ONE (para la contraseña de la base de datos personal)
2. Implementar los reportes al final
3. Asegurar que todos los formularios tengan validación cliente y servidor
4. Integrar los endpoints de la API en cada vista