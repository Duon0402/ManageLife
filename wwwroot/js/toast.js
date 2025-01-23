function showToast(message, type = 'info', duration = 3000) {
    const toastId = `toast-${Date.now()}`;
    const colors = {
        success: '#28a745',
        error: '#dc3545',
        warning: '#ffc107',
        info: '#17a2b8'
    };

    const toastHtml = `
        <div id="${toastId}" style="
            position: fixed;
            top: 20px;
            right: 20px;
            background-color: ${colors[type] || colors.info};
            color: white;
            padding: 10px 15px;
            margin-bottom: 10px;
            border-radius: 5px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
            animation: fadeIn 0.5s;
        ">
            <span>${message}</span>
        </div>
    `;

    $('body').append(toastHtml);

    setTimeout(() => {
        $(`#${toastId}`).fadeOut(500, function () {
            $(this).remove();
        });
    }, duration);
}
