// Favoritos de Juegos (localStorage)
function getFavoritos() {
    try {
        return JSON.parse(localStorage.getItem('favoritos') || '[]');
    } catch { return []; }
}
function setFavoritos(favs) {
    localStorage.setItem('favoritos', JSON.stringify(favs));
}
function toggleFavorito(juegoId) {
    let favs = getFavoritos();
    if (favs.includes(juegoId)) {
        favs = favs.filter(id => id !== juegoId);
    } else {
        favs.push(juegoId);
    }
    setFavoritos(favs);
    updateFavButtons();
}
function updateFavButtons() {
    let favs = getFavoritos();
    document.querySelectorAll('.btn-fav').forEach(btn => {
        let id = parseInt(btn.getAttribute('data-juego-id'));
        if (favs.includes(id)) {
            btn.classList.add('active');
            btn.querySelector('i').classList.remove('bi-heart');
            btn.querySelector('i').classList.add('bi-heart-fill');
        } else {
            btn.classList.remove('active');
            btn.querySelector('i').classList.remove('bi-heart-fill');
            btn.querySelector('i').classList.add('bi-heart');
        }
    });
    // Actualiza badge
    const badge = document.getElementById('badge-favoritos');
    if (badge) {
        badge.textContent = favs.length > 0 ? favs.length : '';
        badge.classList.toggle('d-none', favs.length === 0);
    }
    const badgePerfil = document.getElementById('badge-fav-count');
    if (badgePerfil) badgePerfil.textContent = favs.length;
}
window.updateFavoritosBadge = function() {
    const favs = getFavoritos();
    const badge = document.getElementById('badge-favoritos');
    if (badge) {
        if (favs.length > 0) {
            badge.textContent = favs.length;
            badge.classList.remove('d-none');
        } else {
            badge.textContent = '';
            badge.classList.add('d-none');
        }
    }
};
document.addEventListener('DOMContentLoaded', function () {
    function getFavoritos() {
        try {
            return JSON.parse(localStorage.getItem('favoritos') || '[]');
        } catch { return []; }
    }
    function setFavoritos(ids) {
        localStorage.setItem('favoritos', JSON.stringify(ids));
    }
    function updateFavButtons() {
        const favoritos = getFavoritos();
        document.querySelectorAll('.btn-fav').forEach(btn => {
            const id = parseInt(btn.getAttribute('data-juego-id'));
            if (favoritos.includes(id)) {
                btn.classList.add('active');
                btn.querySelector('i').classList.add('bi-heart-fill');
                btn.querySelector('i').classList.remove('bi-heart');
            } else {
                btn.classList.remove('active');
                btn.querySelector('i').classList.remove('bi-heart-fill');
                btn.querySelector('i').classList.add('bi-heart');
            }
        });
        // Actualiza badge
        const badge = document.getElementById('badge-favoritos');
        if (badge) {
            badge.textContent = favoritos.length > 0 ? favoritos.length : '';
            badge.classList.toggle('d-none', favoritos.length === 0);
        }
        const badgePerfil = document.getElementById('badge-fav-count');
        if (badgePerfil) badgePerfil.textContent = favoritos.length;
    }
    document.querySelectorAll('.btn-fav').forEach(btn => {
        btn.addEventListener('click', function () {
            const id = parseInt(this.getAttribute('data-juego-id'));
            fetch('/Juegos/ToggleFavorito', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({ idJuego: id })
            })
            .then(res => res.json())
            .then(data => {
                let favoritos = getFavoritos();
                if (favoritos.includes(id)) {
                    favoritos = favoritos.filter(f => f !== id);
                } else {
                    favoritos.push(id);
                }
                setFavoritos(favoritos);
                updateFavButtons();
            });
        });
    });
    updateFavButtons();
    window.getFavoritos = getFavoritos;
});
