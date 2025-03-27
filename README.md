# Programación III - 2025
## Grupo No.1
### Integrantes
- **Luis Carlos Lima Pérez**
- **Angelica María Mejía Tzoc**
- **Mynor Ebenezer Alonso Miranda**
- **Josué Emanuel Ramírez Aquino**
- **Josseline Emerita Galeano Hernández**

# Estructura API Principal del Negocio

## Entidades

### 1. Producto
**Ubicación**: `SuperBodega.API/Models/Producto.cs`
**Propiedades**:
- Id (int, PK)
- Codigo (string)
- Nombre (string)
- Descripcion (string)
- CategoriaId (int, FK)
- Stock (int)
- PrecioDeCompra (decimal)
- PrecioDeVenta (decimal)
- FechaDeRegistro (DateTime)
- Estado (bool)
- Imagen (string, ruta)

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

### 2. Categoría
**Ubicación**: `SuperBodega.API/Models/Categoria.cs`
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
FechaRegistro datetime default getdate()
)
```
**Relaciones**:
- Productos (1:N)

### 3. Proveedor
**Ubicación**: `SuperBodega.API/Models/Proveedor.cs`
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
**Ubicación**: `SuperBodega.API/Models/Cliente.cs`
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
**Ubicación**: `SuperBodega.API/Models/Compra.cs`
**Propiedades**:
- Id (int, PK)
- NumeroDeFactura (string)
- ProveedorId (int, FK)
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
**Ubicación**: `SuperBodega.API/Models/DetalleDeLaCompra.cs`
**Propiedades**:
- Id (int, PK)
- CompraId (int, FK)
- ProductoId (int, FK)
- PrecioDeCompra (decimal)
- PrecioDeVenta (decimal)
- Cantidad (int)
- MontoTotal (decimal)
- FechaDeRegistro (DateTime)

**Relaciones**:
- Compra (N:1)
- Producto (N:1)

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

### 7. EstadoDeLaVenta
**Ubicación**: `SuperBodega.API/Models/EstadoDeLaVenta.cs`
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
**Ubicación**: `SuperBodega.API/Models/Venta.cs`
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
**Ubicación**: `SuperBodega.API/Models/DetalleDeLaVenta.cs`
**Propiedades**:
- Id (int, PK)
- VentaId (int, FK)
- ProductoId (int, FK)
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
**Ubicación**: `SuperBodega.API/Repositories/Interfaces/`

1. **IGenericRepository.cs** (Base genérica para todos los repositorios)
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
**Ubicación**: `SuperBodega.API/Services/`
- Implementación correspondiente para cada interfaz

## Servicios
**Ubicación**: `SuperBodega.API/Services/`

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
**Ubicación**: `SuperBodega.API/DTOs/`

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
**Ubicación**: `SuperBodega.API/Controllers/`

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
