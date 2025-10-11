// Configuración global de Chart.js
Chart.defaults.font.family = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
Chart.defaults.color = '#6c757d';
Chart.defaults.borderColor = 'rgba(0, 0, 0, 0.1)';

let chart;

// Función para actualizar el gráfico
export function updateChart(online, warning, offline) {
    const ctx = document.getElementById('statusChart');
    
    // Destruir el gráfico anterior si existe
    if (chart) {
        chart.destroy();
    }
    
    // Configuración del gráfico
    const config = {
        type: 'doughnut',
        data: {
            labels: ['En Línea', 'Con Advertencias', 'Inactivas/Error'],
            datasets: [{
                data: [online, warning, offline],
                backgroundColor: [
                    '#00b894',  // Verde para en línea
                    '#fdcb6e',  // Amarillo para advertencias
                    '#ff7675'   // Rojo para inactivas/error
                ],
                borderWidth: 0,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '70%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20,
                        usePointStyle: true,
                        pointStyle: 'circle',
                        font: {
                            size: 13
                        }
                    }
                },
                tooltip: {
                    backgroundColor: '#2d3436',
                    titleFont: { size: 14, weight: 'bold' },
                    bodyFont: { size: 13 },
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.raw || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = Math.round((value / total) * 100);
                            return `${label}: ${value} (${percentage}%)`;
                        }
                    }
                }
            },
            animation: {
                animateScale: true,
                animateRotate: true
            }
        }
    };
    
    // Crear el nuevo gráfico
    if (ctx) {
        chart = new Chart(ctx, config);
    }
}

// Función para inicializar el gráfico (opcional)
export function initializeChart() {
    // Inicializar con valores por defecto
    updateChart(0, 0, 0);
}

// Inicializar el gráfico cuando se cargue el módulo
initializeChart();
