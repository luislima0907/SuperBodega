$(document).ready(function() {
    // Crear contenedor para los toast si es necesario
    $('body').append('<div class="toast-container"></div>');
    
    // Efecto de aparecer gradualmente para la sección principal
    $('.hero-section').hide().fadeIn(800);
    
    // Funcionalidad básica
    setTimeout(() => {
        showToast('info', 'Bienvenido', 'Bienvenido a SuperBodega.');
    }, 1000);
    
    // Función simplificada para toast de notificación
    function showToast(type, title, message) {
        console.log(`Mostrando toast: ${type} - ${title} - ${message}`);
        
        const toastId = 'toast-' + new Date().getTime();
        let bgClass = 'bg-primary text-white';
        let iconClass = 'bi-info-circle';
        
        switch(type) {
            case 'success':
                bgClass = 'bg-success text-white';
                iconClass = 'bi-check-circle';
                break;
            case 'error':
                bgClass = 'bg-danger text-white';
                iconClass = 'bi-exclamation-circle';
                break;
            case 'warning':
                bgClass = 'bg-warning text-dark';
                iconClass = 'bi-exclamation-triangle';
                break;
        }
        
        const $toast = $(`
            <div id="${toastId}" class="toast ${bgClass}" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header">
                    <i class="bi ${iconClass} me-2"></i>
                    <strong class="me-auto">${title}</strong>
                    <small>Ahora</small>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Cerrar"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `);
        
        $('.toast-container').append($toast);
        
        const toast = new bootstrap.Toast($toast[0], {
            delay: 5000,
            autohide: true
        });
        
        toast.show();
        
        $toast.on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }
});