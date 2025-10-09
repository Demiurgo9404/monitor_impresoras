// QOPIQ - Frontend JavaScript Functions

// Inicialización de la aplicación
window.init = () => {
    console.log('QOPIQ Frontend initialized');
    
    // Configurar tooltips de Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Configurar popovers de Bootstrap
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl, {
            container: 'body'
        });
    });
};

// Mostrar toast notifications
window.showToast = (title, message, type = 'info') => {
    const toastId = 'toast-' + Date.now();
    const iconClass = {
        'success': 'fas fa-check-circle text-success',
        'error': 'fas fa-exclamation-circle text-danger',
        'warning': 'fas fa-exclamation-triangle text-warning',
        'info': 'fas fa-info-circle text-info'
    }[type] || 'fas fa-info-circle text-info';
    
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <div class="d-flex align-items-center">
                        <i class="${iconClass} me-2"></i>
                        <div>
                            <strong>${title}</strong>
                            <div class="small text-muted">${message}</div>
                        </div>
                    </div>
                </div>
                <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 5000 });
    toast.show();
    
    // Remover el toast después de que se oculte
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
};

// Crear contenedor de toasts si no existe
function getOrCreateToastContainer() {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        const modalId = 'confirm-modal-' + Date.now();
        const modalHtml = `
            <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">
                                <i class="fas fa-question-circle text-warning me-2"></i>
                                ${title}
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <p>${message}</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                            <button type="button" class="btn btn-primary" id="confirm-btn">Confirmar</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHtml);
        
        const modalElement = document.getElementById(modalId);
        const modal = new bootstrap.Modal(modalElement);
        
        const confirmBtn = modalElement.querySelector('#confirm-btn');
        confirmBtn.addEventListener('click', () => {
            modal.hide();
            resolve(true);
        });
        
        modalElement.addEventListener('hidden.bs.modal', () => {
            modalElement.remove();
            resolve(false);
        });
        
        modal.show();
    });
};

// Descargar archivo
window.downloadFile = (filename, contentType, data) => {
    const blob = new Blob([data], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

// Copiar al portapapeles
window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        showToast('Copiado', 'Texto copiado al portapapeles', 'success');
        return true;
    } catch (err) {
        console.error('Error copying to clipboard:', err);
        showToast('Error', 'No se pudo copiar al portapapeles', 'error');
        return false;
    }
};

// Formatear números
window.formatNumber = (number, decimals = 0) => {
    return new Intl.NumberFormat('es-ES', {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    }).format(number);
};

// Formatear moneda
window.formatCurrency = (amount, currency = 'EUR') => {
    return new Intl.NumberFormat('es-ES', {
        style: 'currency',
        currency: currency
    }).format(amount);
};

// Formatear fecha
window.formatDate = (date, options = {}) => {
    const defaultOptions = {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    };
    
    return new Intl.DateTimeFormat('es-ES', { ...defaultOptions, ...options }).format(new Date(date));
};

// Formatear fecha y hora
window.formatDateTime = (date) => {
    return new Intl.DateTimeFormat('es-ES', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(new Date(date));
};

// Formatear tamaño de archivo
window.formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

// Crear gráfico con Chart.js
window.createChart = (canvasId, config) => {
    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error('Canvas element not found:', canvasId);
        return null;
    }
    
    // Configuración por defecto
    const defaultConfig = {
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    };
    
    // Merge configuraciones
    const mergedConfig = {
        ...defaultConfig,
        ...config,
        options: {
            ...defaultConfig.options,
            ...config.options
        }
    };
    
    return new Chart(ctx, mergedConfig);
};

// Inicializar calendario FullCalendar
window.initializeCalendar = (elementId, options = {}) => {
    const calendarEl = document.getElementById(elementId);
    if (!calendarEl) {
        console.error('Calendar element not found:', elementId);
        return null;
    }
    
    const defaultOptions = {
        initialView: 'dayGridMonth',
        locale: 'es',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        height: 'auto',
        eventDisplay: 'block',
        dayMaxEvents: 3,
        moreLinkClick: 'popover'
    };
    
    const calendar = new FullCalendar.Calendar(calendarEl, {
        ...defaultOptions,
        ...options
    });
    
    calendar.render();
    return calendar;
};

// Scroll suave a elemento
window.scrollToElement = (elementId, offset = 0) => {
    const element = document.getElementById(elementId);
    if (element) {
        const elementPosition = element.getBoundingClientRect().top;
        const offsetPosition = elementPosition + window.pageYOffset - offset;
        
        window.scrollTo({
            top: offsetPosition,
            behavior: 'smooth'
        });
    }
};

// Detectar tema del sistema
window.getSystemTheme = () => {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
};

// Aplicar tema
window.applyTheme = (theme) => {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('qopiq-theme', theme);
};

// Cargar tema guardado
window.loadSavedTheme = () => {
    const savedTheme = localStorage.getItem('qopiq-theme') || getSystemTheme();
    applyTheme(savedTheme);
    return savedTheme;
};

// Validar email
window.isValidEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
};

// Generar ID único
window.generateId = () => {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
};

// Debounce function
window.debounce = (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

// Throttle function
window.throttle = (func, limit) => {
    let inThrottle;
    return function() {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
};

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', () => {
    initializeApp();
    loadSavedTheme();
});

// Render preview chart for templates
window.renderPreviewChart = function () {
    const canvas = document.getElementById('previewChart');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    
    // Sample data for preview
    const data = {
        labels: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'],
        datasets: [{
            label: 'Páginas Impresas',
            data: [1200, 1900, 3000, 2500, 2200, 3000],
            backgroundColor: 'rgba(0, 123, 255, 0.1)',
            borderColor: 'rgba(0, 123, 255, 1)',
            borderWidth: 2,
            fill: true,
            tension: 0.4
        }]
    };

    const config = {
        type: 'line',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    };

    new Chart(ctx, config);
};

console.log('QOPIQ JavaScript utilities loaded successfully');
