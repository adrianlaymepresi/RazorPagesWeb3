// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// Toggle de la barra lateral
const boton_toggle = document.getElementById('toggleSidebar');
const cuerpo_html = document.body;

function aplicar_estado_inicial() {
    const guardado = sessionStorage.getItem('estado_sidebar'); // "abierto" | "cerrado" | null

    // controlar estado 
    if (guardado === 'cerrado') {
        cuerpo_html.classList.add('sidebar-cerrado');
    } else if (guardado === 'abierto') {
        cuerpo_html.classList.remove('sidebar-cerrado');
    }
}

function alternar_sidebar() {
    const quedo_cerrado = cuerpo_html.classList.toggle('sidebar-cerrado');
    sessionStorage.setItem('estado_sidebar', quedo_cerrado ? 'cerrado' : 'abierto');
}

aplicar_estado_inicial();

if (boton_toggle) {
    boton_toggle.addEventListener('click', alternar_sidebar);
}


// Write your JavaScript code.
