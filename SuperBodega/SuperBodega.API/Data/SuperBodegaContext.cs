using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Models.Ecommerce;

namespace SuperBodega.API.Data
{
    public class SuperBodegaContext : DbContext
    {
        // Bandera estática para asegurar que la creación de la base de datos se haga una sola vez por aplicación
        private static bool _databaseInitialized = false;
        private static readonly object _lockObject = new object();
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleDeLaCompra> DetallesDeLaCompra { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<ElementoCarrito> ElementosCarrito { get; set; }
        
        public DbSet<EstadoDeLaVenta> EstadosDeLaVenta { get; set; }
        
        public DbSet<Venta> Ventas { get; set; }
        
        public DbSet<DetalleDeLaVenta> DetallesDeLaVenta { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuración de la entidad Compra
            modelBuilder.Entity<Compra>(entity =>
            {
                entity.ToTable("Compra");
                entity.Property(e => e.Id).HasColumnName("IdCompra");
                entity.Property(e => e.NumeroDeFactura)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.IdProveedor).IsRequired();
                entity.Property(e => e.FechaDeRegistro).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.MontoTotal)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);
                
                // Relación con Proveedor
                entity.HasOne(e => e.Proveedor)
                    .WithMany()
                    .HasForeignKey(e => e.IdProveedor)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configuración de la entidad DetalleDeLaCompra
            modelBuilder.Entity<DetalleDeLaCompra>(entity =>
            {
                entity.ToTable("DetalleDeLaCompra");
                entity.Property(e => e.Id).HasColumnName("IdDetalleDeLaCompra");
                entity.Property(e => e.IdCompra).IsRequired();
                entity.Property(e => e.IdProducto).IsRequired();
                entity.Property(e => e.PrecioDeCompra)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);
                entity.Property(e => e.PrecioDeVenta)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);
                entity.Property(e => e.Cantidad).IsRequired();
                entity.Property(e => e.Montototal)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);
                entity.Property(e => e.FechaDeRegistro).HasColumnName("FechaDeRegistro")
                    .HasDefaultValueSql("GETDATE()");
                
                // Relación con Compra
                entity.HasOne(e => e.Compra)
                    .WithMany(c => c.DetallesDeLaCompra)
                    .HasForeignKey(e => e.IdCompra)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Relación con Producto
                entity.HasOne(e => e.Producto)
                    .WithMany()
                    .HasForeignKey(e => e.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // Configuración de la entidad Carrito
            modelBuilder.Entity<Carrito>(entity =>
            {
                entity.ToTable("Carrito");
                entity.Property(e => e.Id).HasColumnName("IdCarrito");
                entity.Property(e => e.ClienteId).IsRequired();
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                // Relación con Cliente
                entity.HasOne(e => e.Cliente)
                    .WithMany()
                    .HasForeignKey(e => e.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ignorar la propiedad calculada Total
                entity.Ignore(c => c.Total);
            });

            // Configuración de la entidad ElementoCarrito
            modelBuilder.Entity<ElementoCarrito>(entity =>
            {
                entity.ToTable("ElementoCarrito");
                entity.Property(e => e.Id).HasColumnName("IdElementoCarrito");
                entity.Property(e => e.CarritoId).IsRequired();
                entity.Property(e => e.ProductoId).IsRequired();
                entity.Property(e => e.Cantidad).IsRequired();
                entity.Property(e => e.PrecioUnitario)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0m);

                // Relación con Carrito
                entity.HasOne(e => e.Carrito)
                    .WithMany(c => c.Elementos)
                    .HasForeignKey(e => e.CarritoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Producto
                entity.HasOne(e => e.Producto)
                    .WithMany()
                    .HasForeignKey(e => e.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ignorar la propiedad calculada Subtotal
                entity.Ignore(e => e.Subtotal);
            });
            
            //Venta Configuration
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("Venta");
                entity.HasOne(v => v.Cliente)
                      .WithMany() // No navigation property specified from Cliente to Ventas
                      .HasForeignKey(v => v.IdCliente)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deleting client if they have sales

                entity.HasOne(v => v.EstadoDeLaVenta)
                      .WithMany(e => e.Ventas)
                      .HasForeignKey(v => v.IdEstadoDeLaVenta)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deleting state if used in sales

                entity.HasMany(v => v.DetallesDeLaVenta)
                      .WithOne(d => d.Venta)
                      .HasForeignKey(d => d.IdVenta)
                      .OnDelete(DeleteBehavior.Cascade); // Deleting a Venta deletes its details
            });

            // DetalleDeLaVenta Configuration
            modelBuilder.Entity<DetalleDeLaVenta>(entity =>
            {
                entity.ToTable("DetalleDeLaVenta");
                entity.HasOne(d => d.Producto)
                      .WithMany() // No navigation property needed from Producto to DetallesDeLaVenta
                      .HasForeignKey(d => d.IdProducto)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deleting product if sold

                entity.HasOne(d => d.Proveedor)
                      .WithMany() // No navigation property needed from Proveedor to DetallesDeLaVenta
                      .HasForeignKey(d => d.IdProveedor)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deleting provider if linked
            });

             // Seed initial EstadoDeLaVenta if needed
            modelBuilder.Entity<EstadoDeLaVenta>().HasData(
                new EstadoDeLaVenta { Id = 1, Nombre = "Recibida" },
                new EstadoDeLaVenta { Id = 2, Nombre = "Despachada" },
                new EstadoDeLaVenta { Id = 3, Nombre = "Entregada" },
                new EstadoDeLaVenta { Id = 4, Nombre = "Devolución Solicitada" },
                new EstadoDeLaVenta { Id = 5, Nombre = "Devolución Completada" }
            );
        }

        
        public SuperBodegaContext(DbContextOptions<SuperBodegaContext> options) : base(options)
        {
            // Solo intentar crear la base de datos si aún no está inicializada
            if (!_databaseInitialized)
            {
                // Usar un lock para asegurar que solo un hilo intente inicializar la base de datos
                lock (_lockObject)
                {
                    if (!_databaseInitialized)
                    {
                        try
                        {
                            var dbCreator = Database.GetService<IRelationalDatabaseCreator>() as RelationalDatabaseCreator;
                            if (dbCreator != null)
                            {
                                if (!dbCreator.CanConnect())
                                {
                                    dbCreator.Create();
                                    Console.WriteLine("Base de datos creada.");
                                }

                                if (!dbCreator.HasTables())
                                {
                                    dbCreator.CreateTables();
                                    Console.WriteLine("Tablas creadas en la base de datos.");
                                }
                            }
                            
                            // Marcar como inicializada después de la creación exitosa
                            _databaseInitialized = true;
                            Console.WriteLine("Base de datos inicializada correctamente.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al inicializar base de datos: {ex.Message}");
                            // No marcamos como inicializada para que se intente nuevamente en el próximo reinicio
                        }
                    }
                }
            }
        }
    }
}