function showToast(message, title = 'Thông báo', type = 'info', duration = 3000) {
    const toastId = `toast-${Date.now()}`;

    const config = {
        success: {
            color: '#28a745',
            icon: 'bi-check-circle-fill'
        },
        error: {
            color: '#dc3545',
            icon: 'bi-x-circle-fill'
        },
        warning: {
            color: '#ffc107',
            icon: 'bi-exclamation-triangle-fill'
        },
        info: {
            color: '#17a2b8',
            icon: 'bi-info-circle-fill'
        }
    };

    const { color, icon } = config[type] || config.info;

    if ($('#toast-container').length === 0) {
        $('body').append(`
            <div id="toast-container" class="position-fixed top-0 end-0 p-3" style="z-index: 9999;"></div>
        `);
    }

    const toastHtml = `
        <div id="${toastId}" class="toast border-0 mb-2" role="alert" aria-live="assertive" aria-atomic="true" style="min-width: 300px;">
            <div class="toast-header text-white" style="background-color: ${color};">
                <i class="bi ${icon} me-2"></i>
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    $('#toast-container').prepend(toastHtml);

    const $toastEl = $(`#${toastId}`);
    const bsToast = new bootstrap.Toast($toastEl[0], { delay: duration });
    bsToast.show();

    $toastEl.on('hidden.bs.toast', function () {
        $(this).remove();
    });
}