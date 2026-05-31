namespace App.Constants {
    export const HttpMethod = {
        GET: 'GET',
        POST: 'POST',
        PUT: 'PUT',
        DELETE: 'DELETE'
    };

    export const ApiCode = {
        SUCCESS: '00',
        EXCEPTION: '99',
        FORBIDDEN: '403'
    };

    export const HttpStatus = {
        UNAUTHORIZED: 401,
        FORBIDDEN: 403
    };

    export const Messages = {
        DEFAULT_ERROR: 'Đã có lỗi xảy ra',
        FORBIDDEN: 'Bạn không có quyền thực hiện chức năng này',
        NOTIFICATION_TITLE: 'Thông báo'
    };

    export const Urls = {
        LOGIN: '/Auth/Login'
    };

    export const Defaults = {
        CONTENT_TYPE: 'application/json',
        DATA_TYPE: 'json'
    };
}
