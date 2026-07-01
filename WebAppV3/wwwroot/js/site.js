// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', () => {
    const themeToggle = document.getElementById('theme-toggle');
    const body = document.body;

    // Function to set the theme
    function setTheme(theme) {
        if (theme === 'dark') {
            body.classList.add('dark-theme');
            themeToggle.textContent = '🌙'; // Dark mode icon
        } else {
            body.classList.remove('dark-theme');
            themeToggle.textContent = '☀️'; // Light mode icon
        }
        localStorage.setItem('theme', theme);
    }

    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        setTheme(savedTheme);
    } else {
        setTheme('dark'); // Дефолтная тема для первого визита
    }

    if (savedTheme) {
        setTheme(savedTheme);
    } else if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        setTheme('dark'); // Default to dark if system prefers dark
    } else {
        setTheme('light'); // Default to light
    }

    // Toggle theme on button click
    if (themeToggle) {
        themeToggle.addEventListener('click', () => {
            const currentTheme = body.classList.contains('dark-theme') ? 'dark' : 'light';
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            setTheme(newTheme);
        });
    }
});

async function tailorResume() {
    const title = document.getElementById('jobTitle').value;
    const description = document.getElementById('jobDescription').value;

    const spinner = document.getElementById('tailor-spinner');
    const container = document.getElementById('tailored-resume-container');
    const iframe = document.getElementById('resume-preview-frame');

    if (!title || !description) {
        alert("Пожалуйста, заполните заголовок и описание вакансии!");
        return;
    }

    // Включаем лоадер и показываем контейнер
    spinner.classList.remove('d-none');
    container.classList.remove('d-none');

    // Очищаем старое содержимое iframe и пишем туда заглушку ожидания
    const iframeDoc = iframe.contentWindow.document;
    iframeDoc.open();
    iframeDoc.write("<p style='font-family: sans-serif; text-align: center; margin-top: 50px;'>Глубокий анализ Gemini 3.5 Flash... Пожалуйста, подождите, это занимает около 5-10 секунд.</p>");
    iframeDoc.close();

    try {
        // Делаем POST запрос на наш .NET контроллер
        const response = await fetch('/Ai/TailorResume', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ jobTitle: title, jobDescription: description })
        });

        if (!response.ok) {
            throw new Error(`Ошибка сервера: ${response.status}`);
        }

        const result = await response.json();

        if (result.success) {
            // Записываем полученный от ИИ HTML код прямо внутрь iframe
            iframeDoc.open();
            iframeDoc.write(result.html);
            iframeDoc.close();

            // СОХРАНЯЕМ HTML ДЛЯ ПОСЛЕДУЮЩЕГО СКАЧИВАНИЯ
            document.getElementById('hdn-html-content').value = result.html;

            // Показываем кнопку скачивания PDF вместо заглушки
            document.getElementById('btn-download-pdf').classList.remove('d-none');
        } else {
            iframeDoc.open();
            iframeDoc.write(`<p style='color: red;'>Ошибка: ${result.error}</p>`);
            iframeDoc.close();
        }
    } catch (error) {
        iframeDoc.open();
        iframeDoc.write(`<p style='color: red;'>Критическая ошибка сети: ${error.message}</p>`);
        iframeDoc.close();
    } finally {
        // Прячем лоадер
        spinner.classList.add('d-none');
    }
}

function triggerPdfDownload() {
    const htmlContent = document.getElementById('hdn-html-content').value;
    if (!htmlContent) {
        alert("Нет данных для скачивания!");
        return;
    }
    // Отправляем скрытую форму. Браузер сам поймет, что это скачивание файла, 
    // и страница не перезагрузится.
    document.getElementById('form-download-pdf').submit();
}