$(document).ready(function() {
    // Calcular el ancho del scrollbar para compensarlo durante animaciones
    var scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;

    // Alternar el sidebar
    $('#sidebarCollapse').on('click', function() {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    // Manejar los menús desplegables con delegación de eventos
    $(document).on('click', '.submenu > a', function(e) {
        e.preventDefault();
        var $parent = $(this).parent('li');
        var duracionAnimacion = 300; // Duración en milisegundos

        // Antes de la animación, compensar el scrollbar para evitar saltos
        if ($(document).height() > $(window).height()) {
            $('body').css('padding-right', scrollbarWidth + 'px');
        }

        // Cierra todos los otros submenús
        $('.submenu').not($parent).removeClass('open');
        $('.submenu').not($parent).find('.submenu-items').removeClass('open').slideUp(duracionAnimacion);

        // Alterna el menú actual
        $parent.toggleClass('open');

        // Si el menú ahora está abierto, asegurémonos de que se muestre correctamente
        if ($parent.hasClass('open')) {
            $parent.find('.submenu-items').addClass('open').slideDown(duracionAnimacion, function() {
                // Restaurar el padding después de que termina la animación
                $('body').css('padding-right', '');
            });
        } else {
            $parent.find('.submenu-items').removeClass('open').slideUp(duracionAnimacion, function() {
                // Restaurar el padding después de que termina la animación
                $('body').css('padding-right', '');
            });
        }
    });

    // Abrir el menú correspondiente si la URL coincide
    let currentUrl = window.location.pathname;
    $('.submenu-items li a').each(function() {
        let menuItemUrl = $(this).attr('href');
        if (menuItemUrl && currentUrl.indexOf(menuItemUrl) !== -1) {
            $(this).addClass('active');

            // Asegurar que el menú padre esté abierto
            var $parentMenu = $(this).closest('.submenu');
            $parentMenu.addClass('open');

            // Asegurar que los items del submenú estén visibles
            var $submenuItems = $parentMenu.find('.submenu-items');
            $submenuItems.addClass('open').show(); // Mostrar sin animación para el menú activo
        }
    });

    // Inicializar tooltips de Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });
});