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
    const previewContainer = document.getElementById('tailored-resume-container');

    if (!title || !description) {
        alert("Пожалуйста, заполните заголовок и описание вакансии!");
        return;
    }

    if (spinner) spinner.classList.remove('d-none');

    try {
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
            if (previewContainer) previewContainer.classList.remove('d-none');

            // 1. Изолированная вставка Резюме
            const resumeDiv = document.getElementById('resume-div');
            if (resumeDiv) {
                // Если теневой купол еще не создан — создаем его (в режиме 'open')
                let shadowRoot = resumeDiv.shadowRoot;
                if (!shadowRoot) {
                    shadowRoot = resumeDiv.attachShadow({ mode: 'open' });
                }
                // Записываем полный HTML внутрь изолированной зоны
                shadowRoot.innerHTML = result.html;

                const hdnResume = document.getElementById('hdn-html-content');
                if (hdnResume) hdnResume.value = result.html;
            }

            // 2. Изолированная вставка Сопроводительного письма
            if (result.coverHtml) {
                const coverDiv = document.getElementById('cover-div');
                if (coverDiv) {
                    let shadowRoot = coverDiv.shadowRoot;
                    if (!shadowRoot) {
                        shadowRoot = coverDiv.attachShadow({ mode: 'open' });
                    }
                    shadowRoot.innerHTML = result.coverHtml;

                    const hdnCover = document.getElementById('hdn-cover-content');
                    if (hdnCover) hdnCover.value = result.coverHtml;
                }
            }

            // Показываем кнопку скачивания PDF
            document.getElementById('btn-download-pdf')?.classList.remove('d-none');

        } else {
            showErrorInPreview(result.error || "Неизвестная ошибка бэкенда.");
        }
    } catch (error) {
        showErrorInPreview(`Критическая ошибка: ${error.message}`);
    } finally {
        if (spinner) spinner.classList.add('d-none');
    }
}

// Вспомогательная функция вывода ошибок в div
function showErrorInPreview(errorMessage) {
    const previewContainer = document.getElementById('tailored-resume-container');
    if (previewContainer) previewContainer.classList.remove('d-none');

    const resumeDiv = document.getElementById('resume-div');
    if (resumeDiv) {
        resumeDiv.innerHTML = `<div class="alert alert-danger m-3">
            <h3>⚠️ Ошибка генерации</h3>
            <p>${errorMessage}</p>
        </div>`;
    } else {
        alert(errorMessage);
    }
}

function triggerPdfDownload(type) {
    let htmlContent = "";

    if (type === 'resume') {
        htmlContent = document.getElementById('hdn-html-content').value;
    } else if (type === 'cover') {
        htmlContent = document.getElementById('hdn-cover-content').value;
    }

    if (!htmlContent) {
        alert("Content for downloading not found (press button 'Tailor My Resume' again!");
        return;
    }

    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Ai/DownloadPdf';

    // Поле с HTML контентом
    const inputHtml = document.createElement('input');
    inputHtml.type = 'hidden';
    inputHtml.name = 'htmlContent';
    inputHtml.value = htmlContent;
    form.appendChild(inputHtml);

    const inputType = document.createElement('input');
    inputType.type = 'hidden';
    inputType.name = 'docType';
    inputType.value = type;
    form.appendChild(inputType);

    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
}