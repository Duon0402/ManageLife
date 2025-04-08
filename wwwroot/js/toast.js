function showToast(message, title = 'Thông báo', type = 'info', duration = 3000) {
    const toastId = `toast-${Date.now()}`;
    const colors = {
        success: '#28a745',
        error: '#dc3545',
        warning: '#ffc107',
        info: '#17a2b8'
    };

    const toastHtml = `
        <div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
          <div class="toast-header">
            <img src="..." class="rounded me-2" alt="...">
            <strong class="me-auto">${title}</strong>
            <small>11 mins ago</small>
            <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
          </div>
          <div class="toast-body">
            ${message}
          </div>
        </div>
    `;

    $('body').append(toastHtml);

    setTimeout(() => {
        $(`#${toastId}`).fadeOut(500, function () {
            $(this).remove();
        });
    }, duration);
}
