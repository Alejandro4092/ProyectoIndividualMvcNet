// Función para incrementar cantidad
function incrementQuantity(maxStock) {
    const input = document.getElementById('cantidad');
    let value = parseInt(input.value) || 1;
    if (value < maxStock) {
        input.value = value + 1;
    }
}

// Función para decrementar cantidad
function decrementQuantity() {
    const input = document.getElementById('cantidad');
    let value = parseInt(input.value) || 1;
    if (value > 1) {
        input.value = value - 1;
    }
}

// Validar que no se exceda el stock al escribir manualmente
document.getElementById('cantidad')?.addEventListener('input', function () {
    const maxStock = parseInt(this.max);
    let value = parseInt(this.value);

    if (value < 1) {
        this.value = 1;
    } else if (value > maxStock) {
        this.value = maxStock;
    }
});

// Calificación por estrellas
document.addEventListener('DOMContentLoaded', function () {
    const starLabels = document.querySelectorAll('.star-rating-label');

    if (starLabels.length === 0) {
        return;
    }

    function pintarHasta(valor) {
        starLabels.forEach(label => {
            const icon = label.querySelector('i[data-rating]');
            const rating = parseInt(icon.getAttribute('data-rating'));

            if (rating <= valor) {
                icon.classList.add('active');
            } else {
                icon.classList.remove('active');
            }
        });
    }

    starLabels.forEach(label => {
        const input = label.querySelector('input[type="radio"]');
        const icon = label.querySelector('i[data-rating]');
        const valor = parseInt(icon.getAttribute('data-rating'));

        // Click: marca el radio y pinta hasta esa estrella
        label.addEventListener('click', function () {
            input.checked = true;
            pintarHasta(valor);
        });

        // Hover: mostrar preview sin cambiar el valor marcado
        label.addEventListener('mouseenter', function () {
            pintarHasta(valor);
        });
    });

    // Al salir del área de estrellas, restaurar según el radio seleccionado
    const starsContainer = starLabels[0].parentElement;
    starsContainer.addEventListener('mouseleave', function () {
        const checked = document.querySelector('.star-rating-label input[type="radio"]:checked');
        if (checked) {
            const valor = parseInt(checked.value);
            pintarHasta(valor);
        } else {
            pintarHasta(0);
        }
    });

    // Estado inicial
    const inicial = document.querySelector('.star-rating-label input[type="radio"]:checked');
    if (inicial) {
        pintarHasta(parseInt(inicial.value));
    } else {
        pintarHasta(0);
    }
});