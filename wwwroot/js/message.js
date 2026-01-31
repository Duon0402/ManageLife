const messageType = {
    Info: "info",
    Success: "success",
    Warning: "warning",
    Error: "error",
    Question: "question"
};

const messageConfig = {
    info: {
        icon: "fa-solid fa-circle-info text-info",
        btnClass: "btn-primary",
        showCancel: false,
        defaultTitle: "Thông báo"
    },
    success: {
        icon: "fa-solid fa-circle-check text-success",
        btnClass: "btn-success",
        showCancel: false,
        defaultTitle: "Thành công"
    },
    warning: {
        icon: "fa-solid fa-triangle-exclamation text-warning",
        btnClass: "btn-warning",
        showCancel: false,
        defaultTitle: "Cảnh báo"
    },
    error: {
        icon: "fa-solid fa-circle-xmark text-danger",
        btnClass: "btn-danger",
        showCancel: false,
        defaultTitle: "Lỗi"
    },
    question: {
        icon: "fa-solid fa-circle-question text-primary",
        btnClass: "btn-primary",
        showCancel: true,
        defaultTitle: "Xác nhận"
    }
};

function showMessage({ type, message, title, onConfirm, onCancel }) {
    const cfg = messageConfig[type];

    console.log(cfg);

    if (!cfg) throw "Unsupported message type";

    $("#messageIcon").attr("class", cfg.icon);
    $("#messageTitle").text(title || cfg.title);
    $("#messageContent").text(message);

    const btnOk = $("#btnPrimary");
    const btnCancel = $("#btnCancel");

    btnOk
        .attr("class", `btn ${cfg.btnClass}`)
        .off("click")
        .on("click", () => {
            $("#messageModal").modal("hide");
            onConfirm?.();
        });

    btnCancel
        .toggleClass("d-none", !cfg.showCancel)
        .off("click")
        .on("click", () => {
            $("#messageModal").modal("hide");
            onCancel?.();
        });

    $("#messageModal").modal("show");
};


window.message = {
    info(message, title) {
        showMessage({ type: messageType.Info, message, title });
    },
    success(message, title) {
        showMessage({ type: messageType.Success, message, title });
    },
    warning(message, title) {
        showMessage({ type: messageType.Warning, message, title });
    },
    error(message, title) {
        showMessage({ type: messageType.Error, message, title });
    },
    confirm(message, onConfirm, title) {
        showMessage({
            type: messageType.Question,
            message,
            title,
            onConfirm
        });
    }
};