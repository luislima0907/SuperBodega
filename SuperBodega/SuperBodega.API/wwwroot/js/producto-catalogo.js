// Variables globales
let currentPage = 1;
let pageSize = 12;
let totalPages = 0;
let currentCategoriaId = 0;
let searchQuery = '';

document.addEventListener('DOMContentLoaded', function() {
    // Cargar categorías y productos
    loadCategorias();
    loadProductos();

    // Event listener para la búsqueda al hacer clic en el botón
    const searchButton = document.getElementById('searchButton');
    if (searchButton) {
        searchButton.addEventListener('click', function() {
            searchQuery = document.getElementById('searchInput').value.trim();
            currentPage = 1;
            loadProductos();
        });
    }

    // Event listener para la búsqueda al presionar Enter
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                searchQuery = this.value.trim();
                currentPage = 1;
                loadProductos();
            }
        });
    }

    // Event listener para limpiar la búsqueda
    const clearSearchButton = document.getElementById('clearSearchButton');
    if (clearSearchButton) {
        clearSearchButton.addEventListener('click', function() {
            const searchInput = document.getElementById('searchInput');
            if (searchInput) {
                searchInput.value = '';
                searchQuery = '';
                currentPage = 1;
                loadProductos();
            }
        });
    }
});

// Cargar categorías desde la API
function loadCategorias() {
    fetch('/api/ecommerce/ProductoCatalogo/categorias')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar categorías');
            }
            return response.json();
        })
        .then(categorias => {
            renderCategorias(categorias);
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta('Error al cargar categorías: ' + error.message, 'danger');
        });
}

// Renderizar categorías en el sidebar
function renderCategorias(categorias) {
    const categoriasContainer = document.getElementById('categoriasList');

    // Verificar si existe el contenedor de categorías
    if (!categoriasContainer) {
        console.log('No hay contenedor de categorías en esta página');
        return; // Salir de la función si no existe
    }

    // Mantener el primer elemento (Todas las categorías)
    const allCategoriesItem = categoriasContainer.querySelector('[data-categoria-id="0"]');

    // Verificar si existe el elemento "Todas las categorías"
    if (!allCategoriesItem) {
        console.log('No se encontró el elemento "Todas las categorías"');
        return; // Salir de la función si no existe
    }

    // Limpiar lista existente
    categoriasContainer.innerHTML = '';

    // Restaurar el elemento "Todas las categorías"
    categoriasContainer.appendChild(allCategoriesItem);

    // Añadir cada categoría
    categorias.forEach(categoria => {
        const categoriaItem = document.createElement('a');
        categoriaItem.href = '#';
        categoriaItem.className = 'list-group-item list-group-item-action';
        categoriaItem.setAttribute('data-categoria-id', categoria.id);
        categoriaItem.innerHTML = `<i class="bi bi-tag me-2"></i>${categoria.nombre}`;

        // Event listener para filtrar por categoría
        categoriaItem.addEventListener('click', function(e) {
            e.preventDefault();

            // Quitar clase active de todos los items
            document.querySelectorAll('#categoriasList a').forEach(item => {
                item.classList.remove('active');
            });

            // Añadir clase active al seleccionado
            this.classList.add('active');

            // Limpiar búsqueda al cambiar de categoría
            const searchInput = document.getElementById('searchInput');
            if (searchInput) {
                searchInput.value = '';
                searchQuery = '';
            }

            // Actualizar categoría actual y cargar productos
            currentCategoriaId = categoria.id;
            currentPage = 1;
            loadProductos();
        });

        categoriasContainer.appendChild(categoriaItem);
    });

    // Añadir event listener al item "Todas las categorías"
    allCategoriesItem.addEventListener('click', function(e) {
        e.preventDefault();

        // Quitar clase active de todos los items
        document.querySelectorAll('#categoriasList a').forEach(item => {
            item.classList.remove('active');
        });

        // Añadir clase active al seleccionado
        this.classList.add('active');

        // Limpiar búsqueda al cambiar a todas las categorías
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.value = '';
            searchQuery = '';
        }

        // Actualizar categoría actual y cargar productos
        currentCategoriaId = 0;
        currentPage = 1;
        loadProductos();
    });
}

// Cargar productos con paginación y filtro por categoría
function loadProductos() {
    // Mostrar indicador de carga
    const productosContainer = document.getElementById('productosContainer');

    if (!productosContainer) {
        console.log('No se encontró el contenedor de productos en esta página');
        return;
    }

    if (productosContainer) {
        productosContainer.innerHTML = `
            <div class="col-12 text-center my-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
                <p class="mt-2">Cargando productos...</p>
            </div>
        `;
    }

    let url = '/api/ecommerce/ProductoCatalogo?page=' + currentPage + '&pageSize=' + pageSize;

    if (currentCategoriaId > 0) {
        url = '/api/ecommerce/ProductoCatalogo/categoria/' + currentCategoriaId + '?page=' + currentPage + '&pageSize=' + pageSize;
    }

    if (searchQuery) {
        url += '&search=' + encodeURIComponent(searchQuery);
    }

    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar productos');
            }
            return response.json();
        })
        .then(data => {
            renderProductos(data.productos);
            renderPagination(data.currentPage, data.totalPages);
            totalPages = data.totalPages;

            // Si hay término de búsqueda, mostrar cuántos resultados encontramos
            if (searchQuery && productosContainer) {
                const resultInfo = document.createElement('div');
                resultInfo.className = 'col-12 mb-3';
                resultInfo.innerHTML = `
                    <div class="alert alert-info">
                        <i class="bi bi-info-circle me-2"></i>
                        Se encontraron ${data.totalItems} resultado(s) para la búsqueda: "<strong>${searchQuery}</strong>"
                    </div>
                `;
                productosContainer.insertBefore(resultInfo, productosContainer.firstChild);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            if (productosContainer) {
                productosContainer.innerHTML = `
                    <div class="col-12 text-center mt-4">
                        <div class="alert alert-danger">
                            Error al cargar productos: ${error.message}
                        </div>
                    </div>
                `;
            }
            mostrarAlerta('Error al cargar productos: ' + error.message, 'danger');
        });
}

// Renderizar productos en la página
function renderProductos(productos) {
    const template = document.getElementById('producto-template');

    const productosContainer = document.getElementById('productosContainer');
    if (!productosContainer) {
        console.log('No se encontró el contenedor de productos en esta página');
        return;
    }

    productosContainer.innerHTML = '';

    if (productos.length === 0) {
        productosContainer.innerHTML = `
            <div class="col-12 text-center mt-4">
                <div class="alert alert-warning">
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    No se encontraron productos${searchQuery ? ' para la búsqueda: "' + searchQuery + '"' : ''}
                </div>
                ${searchQuery ? '<div class="text-center mt-3"><button id="btnClearSearch" class="btn btn-outline-primary">Limpiar búsqueda</button></div>' : ''}
            </div>
        `;

        // Añadir event listener al botón de limpiar búsqueda dentro del mensaje
        const btnClearSearch = document.getElementById('btnClearSearch');
        if (btnClearSearch) {
            btnClearSearch.addEventListener('click', function() {
                const searchInput = document.getElementById('searchInput');
                if (searchInput) {
                    searchInput.value = '';
                    searchQuery = '';
                    currentPage = 1;
                    loadProductos();
                }
            });
        }
        return;
    }

    productos.forEach(producto => {
        const clone = document.importNode(template.content, true);
        const card = clone.querySelector('.product-card');

        card.dataset.id = producto.id;
        card.dataset.precio = producto.precioVenta || producto.precioDeVenta;
        card.dataset.stock = producto.stock;
        card.dataset.nombre = producto.nombre;
        card.dataset.descripcion = producto.descripcion || 'Sin descripción';

        // Configurar imagen del producto
        const img = clone.querySelector('img');
        if (producto.imagenUrl) {
            img.src = producto.imagenUrl;
        } else {
            img.src = '/images/productos/default.png'; // Imagen por defecto
        }

        img.alt = producto.nombre;
        img.setAttribute('data-nombre', producto.nombre);

        // Configurar detalles del producto
        clone.querySelector('.producto-nombre').textContent = producto.nombre;
        clone.querySelector('.producto-categoria').textContent = producto.categoriaNombre || 'Sin categoría';

        // Acortar descripción si es muy larga
        let descripcion = producto.descripcion || 'Sin descripción';
        if (descripcion.length > 100) {
            descripcion = descripcion.substring(0, 97) + '...';
        }
        clone.querySelector('.producto-descripcion').textContent = descripcion;

        // Formatear precio con moneda de Guatemala (Quetzales)
        clone.querySelector('.producto-precio').textContent = 'Q ' + parseFloat(producto.precioVenta).toFixed(2);

        // Configurar badge de stock
        const stockBadge = clone.querySelector('.producto-stock');
        if (producto.stock > 10) {
            stockBadge.classList.add('bg-success');
            stockBadge.textContent = 'En stock';
        } else if (producto.stock > 0) {
            stockBadge.classList.add('bg-warning');
            stockBadge.textContent = 'Stock bajo: ' + producto.stock;
        } else {
            stockBadge.classList.add('bg-danger');
            stockBadge.textContent = 'Agotado';
        }

        // Event listener para añadir al carrito
        const addToCartBtn = clone.querySelector('.btn-add-cart');

        // Si no hay stock, deshabilitar botón
        if (producto.stock <= 0) {
            addToCartBtn.disabled = true;
            addToCartBtn.classList.replace('btn-primary', 'btn-secondary');
            addToCartBtn.innerHTML = '<i class="bi bi-x-circle me-2"></i>Agotado';
        }

        productosContainer.appendChild(clone);
    });

    // Configurar imágenes ampliables después de agregar los productos al DOM
    setTimeout(() => {
        const productCards = document.querySelectorAll('.product-card');
        productCards.forEach(card => {
            const img = card.querySelector('img');
            const nombre = card.dataset.nombre;

            // Asegurarse que la imagen tiene los atributos correctos
            if (img && nombre) {
                img.alt = nombre;
                img.setAttribute('data-nombre', nombre);
            }
        });

        configurarImagenesAmpliables();
    }, 100);
}

// Renderizar controles de paginación
function renderPagination(currentPage, totalPages) {
    const paginationContainer = document.getElementById('pagination');
    paginationContainer.innerHTML = '';

    if (totalPages <= 1) {
        return;
    }

    // Botón Anterior
    const prevLi = document.createElement('li');
    prevLi.className = 'page-item ' + (currentPage === 1 ? 'disabled' : '');
    const prevLink = document.createElement('a');
    prevLink.className = 'page-link';
    prevLink.href = '#';
    prevLink.textContent = 'Anterior';
    prevLink.setAttribute('aria-label', 'Anterior');
    if (currentPage > 1) {
        prevLink.addEventListener('click', function(e) {
            e.preventDefault();
            changePage(currentPage - 1);
        });
    }
    prevLi.appendChild(prevLink);
    paginationContainer.appendChild(prevLi);

    // Determinar rango de páginas para mostrar
    const maxVisiblePages = 5;
    let startPage = Math.max(currentPage - Math.floor(maxVisiblePages / 2), 1);
    let endPage = Math.min(startPage + maxVisiblePages - 1, totalPages);

    if (endPage - startPage + 1 < maxVisiblePages) {
        startPage = Math.max(endPage - maxVisiblePages + 1, 1);
    }

    // Botón Primera página (si estamos lejos del inicio)
    if (startPage > 1) {
        const firstLi = document.createElement('li');
        firstLi.className = 'page-item';
        const firstLink = document.createElement('a');
        firstLink.className = 'page-link';
        firstLink.href = '#';
        firstLink.textContent = '1';
        firstLink.addEventListener('click', function(e) {
            e.preventDefault();
            changePage(1);
        });
        firstLi.appendChild(firstLink);
        paginationContainer.appendChild(firstLi);

        // Añadir elipsis si es necesario
        if (startPage > 2) {
            const ellipsisLi = document.createElement('li');
            ellipsisLi.className = 'page-item disabled';
            const ellipsisSpan = document.createElement('span');
            ellipsisSpan.className = 'page-link';
            ellipsisSpan.innerHTML = '&hellip;';
            ellipsisLi.appendChild(ellipsisSpan);
            paginationContainer.appendChild(ellipsisLi);
        }
    }

    // Generar links de página
    for (let i = startPage; i <= endPage; i++) {
        const pageLi = document.createElement('li');
        pageLi.className = 'page-item ' + (i === currentPage ? 'active' : '');

        const pageLink = document.createElement('a');
        pageLink.className = 'page-link';
        pageLink.href = '#';
        pageLink.textContent = i;
        pageLink.addEventListener('click', function(e) {
            e.preventDefault();
            changePage(i);
        });

        pageLi.appendChild(pageLink);
        paginationContainer.appendChild(pageLi);
    }

    // Botón Última página (si estamos lejos del final)
    if (endPage < totalPages) {
        // Añadir elipsis si es necesario
        if (endPage < totalPages - 1) {
            const ellipsisLi = document.createElement('li');
            ellipsisLi.className = 'page-item disabled';
            const ellipsisSpan = document.createElement('span');
            ellipsisSpan.className = 'page-link';
            ellipsisSpan.innerHTML = '&hellip;';
            ellipsisLi.appendChild(ellipsisSpan);
            paginationContainer.appendChild(ellipsisLi);
        }

        const lastLi = document.createElement('li');
        lastLi.className = 'page-item';
        const lastLink = document.createElement('a');
        lastLink.className = 'page-link';
        lastLink.href = '#';
        lastLink.textContent = totalPages;
        lastLink.addEventListener('click', function(e) {
            e.preventDefault();
            changePage(totalPages);
        });
        lastLi.appendChild(lastLink);
        paginationContainer.appendChild(lastLi);
    }

    // Botón Siguiente
    const nextLi = document.createElement('li');
    nextLi.className = 'page-item ' + (currentPage === totalPages ? 'disabled' : '');
    const nextLink = document.createElement('a');
    nextLink.className = 'page-link';
    nextLink.href = '#';
    nextLink.textContent = 'Siguiente';
    nextLink.setAttribute('aria-label', 'Siguiente');
    if (currentPage < totalPages) {
        nextLink.addEventListener('click', function(e) {
            e.preventDefault();
            changePage(currentPage + 1);
        });
    }
    nextLi.appendChild(nextLink);
    paginationContainer.appendChild(nextLi);
}

// Cambiar de página
function changePage(page) {
    currentPage = page;
    loadProductos();
    window.scrollTo(0, 0); // Scroll al inicio de la página
}

// Mostrar alerta en la página
function mostrarAlerta(mensaje, tipo) {
    const alertContainer = document.getElementById('alertContainer');
    alertContainer.innerHTML = `
        <div class="alert alert-${tipo} alert-dismissible fade show" role="alert">
            ${mensaje}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    alertContainer.style.display = 'block';

    // Hacer scroll hasta la alerta
    alertContainer.scrollIntoView({ behavior: 'smooth' });

    // Ocultar después de 5 segundos
    setTimeout(() => {
        const alertElement = alertContainer.querySelector('.alert');
        if (alertElement) {
            const bootstrapAlert = new bootstrap.Alert(alertElement);
            bootstrapAlert.close();
        }
    }, 5000);
}