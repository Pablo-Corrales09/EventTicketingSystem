// Transición suave entre páginas: fade del contenido (main), sin afectar el layout.
(function () {
    'use strict';

    const reducirMovimiento = window.matchMedia('(prefers-reduced-motion: reduce)');
    const RETARDO_NAVEGACION = 100;
    const SEGURIDAD_RESTAURAR = RETARDO_NAVEGACION + 150;

    let navegando = false;

    document.addEventListener('click', function (e) {
        if (navegando) return;
        if (reducirMovimiento.matches) return;

        const ancla = e.target.closest('a[href]');
        if (!ancla) return;

        const href = ancla.getAttribute('href');
        if (!href || href.charAt(0) === '#' || /^javascript:/i.test(href)) return;

        if (ancla.target && ancla.target !== '_self') return;
        if (ancla.hasAttribute('download')) return;
        if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

        let destino;
        try {
            destino = new URL(ancla.href, window.location.origin);
        } catch (err) {
            return;
        }
        if (destino.origin !== window.location.origin) return;

        e.preventDefault();
        navegando = true;

        const main = document.querySelector('main');
        if (main) {
            main.classList.add('transicion-salida');

            // Red de seguridad: solo se ejecuta si la página no se descargó
            // (p. ej. enlace que devuelve un archivo), restaurando el contenido.
            setTimeout(function () {
                main.classList.remove('transicion-salida');
                navegando = false;
            }, SEGURIDAD_RESTAURAR);
        }

        setTimeout(function () {
            window.location.href = ancla.href;
        }, RETARDO_NAVEGACION);
    });

    window.addEventListener('pageshow', function () {
        navegando = false;
        const main = document.querySelector('main');
        if (main) {
            main.classList.remove('transicion-salida');
        }
    });
})();