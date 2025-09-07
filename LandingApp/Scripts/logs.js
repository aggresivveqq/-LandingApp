document.addEventListener('DOMContentLoaded', function () {
    const loginBtn = document.getElementById('loginBtn');
    const passwordInput = document.getElementById('password');
    const errorElement = document.getElementById('error');
    const loginDiv = document.getElementById('login');
    const logsDiv = document.getElementById('logs');

    if (!loginBtn || !passwordInput || !errorElement || !loginDiv || !logsDiv) {
        console.error('One or more required DOM elements not found.');
        return;
    }

    loginBtn.addEventListener('click', function () {
        const password = passwordInput.value.trim();
        errorElement.innerText = ''; 

        if (!password) {
            errorElement.innerText = 'Введите пароль';
            return;
        }

        fetch('/logs/stream?password=' + encodeURIComponent(password))
            .then(response => {
                if (response.status === 401) {
                    errorElement.innerText = 'Неверный пароль';
                    return Promise.reject('Unauthorized'); 
                }
                if (!response.ok) {
                    errorElement.innerText = 'Ошибка подключения или сервера';
                    return Promise.reject('Server error: ' + response.status);
                }
                return response;
            })
            .then(() => {
                loginDiv.style.display = 'none';
                logsDiv.style.display = 'block';

                const eventSource = new EventSource('/logs/stream?password=' + encodeURIComponent(password));

                eventSource.onmessage = function (event) {
                    logsDiv.innerText += event.data + '\n';
                    logsDiv.scrollTop = logsDiv.scrollHeight;
                };

                eventSource.onerror = function (event) {
                    console.error("Ошибка подключения к потоку логов:", event);
                    logsDiv.innerText += '\n--- Ошибка подключения к потоку логов. Пожалуйста, обновите страницу. ---\n';
                    eventSource.close(); 
                };
            })
            .catch(err => {
                console.error("Ошибка авторизации:", err);
                if (!errorElement.innerText) {
                    errorElement.innerText = 'Не удалось подключиться или авторизоваться.';
                }
            });
    });
});