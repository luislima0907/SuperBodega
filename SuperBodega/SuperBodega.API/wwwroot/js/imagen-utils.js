/**
 * Utilidades para el manejo de imágenes en toda la aplicación
 */

// Función para mostrar imagen ampliada en ventana modal
function mostrarImagenAmpliada(src, titulo) {
    // Verificación de imagen por defecto
    if (!src || src.includes('/images/productos/default.png')) {
        mostrarAlertaImagen('No hay imagen disponible para este producto', 'info');
        return;
    }

    // Normalizar la URL de la imagen
    let imagenUrl = src;
    if (imagenUrl && !imagenUrl.startsWith('/') && !imagenUrl.startsWith('http')) {
        imagenUrl = '/' + imagenUrl;
    }
    if (imagenUrl.includes('?')) {
        imagenUrl = imagenUrl.split('?')[0];
    }

    // Verificar si ya hay un modal SweetAlert abierto
    const existingModal = document.querySelector('.swal2-container');

    if (existingModal) {
        // Si ya existe un modal, insertamos la imagen dentro de él
        const contenedorActual = document.querySelector('.swal2-html-container');
        if (contenedorActual) {
            // Crear el contenedor de la imagen si no existe
            let imageContainer = document.getElementById('nested-image-preview');
            if (!imageContainer) {
                imageContainer = document.createElement('div');
                imageContainer.id = 'nested-image-preview';
                imageContainer.className = 'nested-image-overlay';
                imageContainer.style.position = 'fixed';
                imageContainer.style.top = '0';
                imageContainer.style.left = '0';
                imageContainer.style.right = '0';
                imageContainer.style.bottom = '0';
                imageContainer.style.backgroundColor = 'rgba(0,0,0,0.8)';
                imageContainer.style.zIndex = '10000';
                imageContainer.style.display = 'flex';
                imageContainer.style.flexDirection = 'column';
                imageContainer.style.alignItems = 'center';
                imageContainer.style.justifyContent = 'center';
                imageContainer.style.padding = '20px';

                // Botón de cierre
                const closeBtn = document.createElement('button');
                closeBtn.innerHTML = '&times;';
                closeBtn.className = 'nested-image-close';
                closeBtn.style.position = 'absolute';
                closeBtn.style.top = '10px';
                closeBtn.style.right = '10px';
                closeBtn.style.fontSize = '24px';
                closeBtn.style.background = 'transparent';
                closeBtn.style.border = 'none';
                closeBtn.style.color = 'white';
                closeBtn.style.cursor = 'pointer';
                closeBtn.title = 'Cerrar';

                closeBtn.onclick = function() {
                    document.body.removeChild(imageContainer);
                };

                // Título
                const titleElem = document.createElement('h3');
                titleElem.className = 'nested-image-title';
                titleElem.style.color = 'white';
                titleElem.style.marginBottom = '15px';
                titleElem.textContent = titulo || 'Vista previa';

                // Imagen
                const imgElem = document.createElement('img');
                imgElem.className = 'nested-image';
                imgElem.src = imagenUrl;
                imgElem.alt = 'Imagen ampliada';
                imgElem.style.maxHeight = '70vh';
                imgElem.style.maxWidth = '90%';
                imgElem.style.boxShadow = '0 0 15px rgba(255,255,255,0.3)';
                imgElem.onerror = function() {
                    this.src = '/images/productos/default.png';
                };

                imageContainer.appendChild(closeBtn);
                imageContainer.appendChild(titleElem);
                imageContainer.appendChild(imgElem);

                document.body.appendChild(imageContainer);
            } else {
                // Si ya existe, actualizar la imagen y título
                const titleElem = imageContainer.querySelector('.nested-image-title');
                const imgElem = imageContainer.querySelector('.nested-image');

                if (titleElem) {
                    titleElem.textContent = titulo || 'Vista previa';
                }

                if (imgElem) {
                    imgElem.src = imagenUrl;
                }
            }
        }
    } else {
        // Si no hay un modal existente, usar SweetAlert normal
        Swal.fire({
            title: titulo || 'Vista previa',
            html: `<div class="text-center">
                    <img src="${imagenUrl}" 
                         class="img-fluid swal-image-preview" 
                         style="max-height: 70vh; max-width: 100%;" 
                         alt="Imagen ampliada"
                         onerror="this.onerror=null;this.src='/images/productos/default.png';">
                </div>`,
            showCloseButton: true,
            showConfirmButton: false,
            width: 'auto',
            padding: '1rem',
            background: '#fff',
            backdrop: 'rgba(0,0,0,0.8)',
            customClass: {
                container: 'image-preview-swal-container',
                popup: 'image-preview-swal-popup',
            },
            didOpen: () => {
                const previewImg = document.querySelector('.swal-image-preview');
                if (previewImg) {
                    previewImg.style.display = 'inline-block';
                }
            }
        });
    }
}

// Función auxiliar para mostrar alertas relacionadas con imágenes
function mostrarAlertaImagen(mensaje, tipo, duracion = 3000) {
    let icon;
    switch (tipo) {
        case 'success': icon = 'success'; break;
        case 'danger': icon = 'error'; break;
        case 'warning': icon = 'warning'; break;
        default: icon = 'info'; break;
    }

    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: duracion,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    Toast.fire({
        icon: icon,
        title: mensaje
    });
}

/**
 * Función para configurar las imágenes ampliables en toda la aplicación
 */
function configurarImagenesAmpliables() {
    // Seleccionar todas las imágenes de productos en cualquier vista
    const imagenes = document.querySelectorAll(
        '.img-thumbnail, .product-img, [data-imagen-producto], ' +
        '.producto-thumbnail, .card-img-top, .product-image, ' +
        '.cart-item-details img'
    );

    imagenes.forEach(imagen => {
        // Solo configurar si aún no tiene la funcionalidad
        if (!imagen.getAttribute('data-ampliable')) {
            // Marcar como configurada
            imagen.setAttribute('data-ampliable', 'true');

            // Estilizar para indicar que es clicable
            imagen.style.cursor = 'pointer';
            if (!imagen.title) {
                imagen.title = 'Clic para ampliar';
            }

            // Agregar el evento de clic
            imagen.addEventListener('click', function(e) {
                // Evitar la propagación del evento si viene de un botón dentro de la imagen
                if (e.target.closest('button')) return;

                // búsqueda más completa del nombre del producto
                let nombreProducto = 'Producto';

                // Intento 1: Buscar en el atributo alt de la imagen
                if (imagen.alt && imagen.alt !== 'Producto') {
                    nombreProducto = imagen.alt;
                }
                // Intento 2: Buscar en data-nombre de la imagen
                else if (imagen.getAttribute('data-nombre')) {
                    nombreProducto = imagen.getAttribute('data-nombre');
                }
                // Intento 3: Buscar en el elemento padre más cercano con data-nombre
                else {
                    // Buscar hasta 3 niveles hacia arriba para encontrar el nombre
                    let parent = imagen.parentElement;
                    for (let i = 0; i < 3 && parent; i++) {
                        if (parent.dataset && parent.dataset.nombre) {
                            nombreProducto = parent.dataset.nombre;
                            break;
                        }
                        parent = parent.parentElement;
                    }
                }

                // Intento 4: Buscar un elemento hermano que tenga la clase producto-nombre
                if (nombreProducto === 'Producto') {
                    const card = imagen.closest('.product-card');
                    if (card) {
                        const nombreElement = card.querySelector('.producto-nombre');
                        if (nombreElement && nombreElement.textContent) {
                            nombreProducto = nombreElement.textContent.trim();
                        }
                    }
                }

                mostrarImagenAmpliada(imagen.src, nombreProducto);
            });
        }
    });
}

/**
 * Observa cambios en el DOM para configurar nuevas imágenes
 */
function observarCambiosDOM() {
    // Crear un observador que detecte cambios en el DOM
    const observer = new MutationObserver(function(mutations) {
        let debeActualizar = false;

        mutations.forEach(function(mutation) {
            // Verificar si se agregaron elementos
            if (mutation.addedNodes.length) {
                // Verificar si alguno de los nodos agregados es o contiene una imagen
                mutation.addedNodes.forEach(node => {
                    if (node.nodeType === 1) {
                        if (node.tagName === 'IMG' || node.querySelectorAll('img').length > 0) {
                            debeActualizar = true;
                        }
                    }
                });
            }
        });

        // Si se detectaron imágenes nuevas, configurarlas
        if (debeActualizar) {
            configurarImagenesAmpliables();
        }
    });

    // Configurar el observador para observar cambios en el DOM
    observer.observe(document.body, {
        childList: true, // Observar cambios en hijos directos
        subtree: true,   // Observar cambios en todo el árbol
        attributes: false // No necesitamos observar cambios de atributos
    });
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    configurarImagenesAmpliables();
    observarCambiosDOM();
});

// Exportar funciones para uso global
window.configurarImagenesAmpliables = configurarImagenesAmpliables;
window.mostrarImagenAmpliada = mostrarImagenAmpliada;