$(document).ready(function() {
    // Alternar el sidebar
    $('#sidebarCollapse').on('click', function() {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    // Manejar los menús desplegables
    $('.submenu > a').on('click', function(e) {
        e.preventDefault();
        var $parent = $(this).parent('li');
        
        // Cierra todos los otros submenús
        $('.submenu').not($parent).removeClass('open');
        $('.submenu').not($parent).find('.submenu-items').removeClass('open').slideUp();
        
        // Alterna el menú actual
        $parent.toggleClass('open');
        $parent.find('.submenu-items').toggleClass('open').slideToggle();
    });

    // Abrir el menú correspondiente si la URL coincide
    let currentUrl = window.location.pathname;
    $('.submenu-items li a').each(function() {
        let menuItemUrl = $(this).attr('href');
        if (menuItemUrl && currentUrl.indexOf(menuItemUrl) !== -1) {
            $(this).addClass('active');
            $(this).closest('.submenu').addClass('open');
            $(this).closest('.submenu-items').addClass('open').show();
        }
    });

    // Inicializar tooltips de Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });
});