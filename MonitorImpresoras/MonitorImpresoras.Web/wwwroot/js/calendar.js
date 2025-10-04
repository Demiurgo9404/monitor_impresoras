// Calendar functionality for scheduled reports
let calendar = null;

window.initializeCalendar = function (view, events) {
    const calendarEl = document.getElementById('calendar-container');
    if (!calendarEl) return;

    // Destroy existing calendar if it exists
    if (calendar) {
        calendar.destroy();
    }

    calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: view === 'week' ? 'timeGridWeek' : 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,listWeek'
        },
        events: events,
        eventClick: function(info) {
            showEventDetails(info.event);
        },
        eventMouseEnter: function(info) {
            showEventTooltip(info.event, info.jsEvent);
        },
        eventMouseLeave: function(info) {
            hideEventTooltip();
        },
        height: 'auto',
        locale: 'es',
        firstDay: 1, // Monday
        businessHours: {
            daysOfWeek: [1, 2, 3, 4, 5], // Monday - Friday
            startTime: '08:00',
            endTime: '18:00'
        },
        eventDisplay: 'block',
        dayMaxEvents: 3,
        moreLinkClick: 'popover',
        eventTimeFormat: {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        }
    });

    calendar.render();
};

window.updateCalendarEvents = function (events) {
    if (calendar) {
        calendar.removeAllEvents();
        calendar.addEventSource(events);
    }
};

function showEventDetails(event) {
    const props = event.extendedProps;
    const content = `
        <div class="event-details">
            <h6 class="mb-2">${event.title}</h6>
            <div class="row g-2">
                <div class="col-6">
                    <small class="text-muted">Proyecto:</small><br>
                    <span class="badge bg-light text-dark">${props.projectName}</span>
                </div>
                <div class="col-6">
                    <small class="text-muted">Tipo:</small><br>
                    <span>${props.reportType}</span>
                </div>
                <div class="col-6">
                    <small class="text-muted">Estado:</small><br>
                    <span class="badge ${props.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${props.isActive ? 'Activo' : 'Pausado'}
                    </span>
                </div>
                <div class="col-6">
                    <small class="text-muted">Última ejecución:</small><br>
                    <span>${props.lastExecution ? formatDateTime(props.lastExecution) : 'Nunca'}</span>
                </div>
                <div class="col-12">
                    <small class="text-muted">Programación:</small><br>
                    <code class="small">${props.cronExpression}</code>
                </div>
            </div>
        </div>
    `;

    showModal('Detalle del Reporte', content);
}

function showEventTooltip(event, jsEvent) {
    const props = event.extendedProps;
    const tooltip = document.createElement('div');
    tooltip.className = 'calendar-tooltip';
    tooltip.innerHTML = `
        <div class="tooltip-content">
            <strong>${event.title}</strong><br>
            <small>Proyecto: ${props.projectName}</small><br>
            <small>Próxima: ${formatDateTime(event.start)}</small><br>
            <small>Estado: ${props.isActive ? 'Activo' : 'Pausado'}</small>
        </div>
    `;
    
    tooltip.style.position = 'absolute';
    tooltip.style.left = jsEvent.pageX + 10 + 'px';
    tooltip.style.top = jsEvent.pageY + 10 + 'px';
    tooltip.style.zIndex = '9999';
    tooltip.style.backgroundColor = '#333';
    tooltip.style.color = 'white';
    tooltip.style.padding = '8px 12px';
    tooltip.style.borderRadius = '4px';
    tooltip.style.fontSize = '12px';
    tooltip.style.maxWidth = '250px';
    tooltip.style.boxShadow = '0 2px 8px rgba(0,0,0,0.3)';
    
    document.body.appendChild(tooltip);
    
    // Store reference for cleanup
    window.currentTooltip = tooltip;
}

function hideEventTooltip() {
    if (window.currentTooltip) {
        document.body.removeChild(window.currentTooltip);
        window.currentTooltip = null;
    }
}

function formatDateTime(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function showModal(title, content) {
    const modalHtml = `
        <div class="modal fade" id="eventDetailModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        ${content}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if any
    const existingModal = document.getElementById('eventDetailModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Add new modal
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('eventDetailModal'));
    modal.show();
    
    // Clean up when modal is hidden
    document.getElementById('eventDetailModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
}

// Calendar navigation helpers
window.calendarGoToDate = function (date) {
    if (calendar) {
        calendar.gotoDate(date);
    }
};

window.calendarChangeView = function (view) {
    if (calendar) {
        calendar.changeView(view);
    }
};

// Cleanup on page unload
window.addEventListener('beforeunload', function () {
    if (calendar) {
        calendar.destroy();
        calendar = null;
    }
    hideEventTooltip();
});
