let counter = 0;
let lastClickTime = null;
let inactivityTimer = null;

const counterValueElement = document.getElementById("counter-value");
const fileSizeValueElement = document.getElementById("file-size-value");
const timeToResponseValueElement = document.getElementById("time-to-response-value");
const charCountValueElement = document.getElementById("char-count-value");

const catAnimationElement = document.getElementById("cat-animation");
const consoleElement = document.getElementById('cat-facts-console');
const cursorElement = document.querySelector('.blinking-cursor');
const clickTimeElement = document.getElementById("click-time");

const terminalWindow = document.querySelector('.terminal-window');

document.getElementById('btn-get-fact').addEventListener('click', () => {
    GetFact();
    ClickTime();
});

document.getElementById('btn-delete-file').addEventListener('click', DeleteFile);


function GetFact() {
    fetch("/CatFacts/GetFact")
        .then(res => {
            if (res.status === 429) {
                TerminalMessage('> ⚠ Nie tak szybko! Zwolnij trochę...', 'log-line log-line--error');
                return null;
            }
            if (res.status === 503) {
                TerminalMessage('> ⚠ Serwis zewnętrzny niedostępny. Spróbuj za chwilę.', 'log-line log-line--error');
                return null;
            }
            if (!res.ok) {
                TerminalMessage('> Błąd serwera. Spróbuj ponownie.', 'log-line log-line--error');
                return null;
            }
            return res.json();
        })
        .then(data => {
            if (!data) return;

            TerminalMessage("> " + data.fact);

            counter++;
            counterValueElement.textContent = counter + " faktów pobranych";
            fileSizeValueElement.textContent = data.fileSizeKb + " KB";
            timeToResponseValueElement.textContent = data.timeToResponseMs + " ms";
            charCountValueElement.textContent = data.charCount + " znaków";
        })
        .catch(error => console.error("Błąd:", error));
}

function TerminalMessage(message, style = 'log-line') {
    const msg = document.createElement('div');
    msg.className = style;
    msg.textContent = message;
    consoleElement.insertBefore(msg, cursorElement);
    terminalWindow.scrollTop = terminalWindow.scrollHeight;
}

function ClickTime() {
    const now = performance.now();
    let timeBetween = 0;
    if (lastClickTime !== null) {
        timeBetween = (now - lastClickTime).toFixed(0);
        clickTimeElement.textContent = `${timeBetween}ms`;
    }

    if (lastClickTime === null || timeBetween > 1000) {
        setCatAnimation("/css/cat-chillout.gif");
    } else if (timeBetween > 300) {
        setCatAnimation("/css/cat-walking.gif");
    } else {
        setCatAnimation("/css/cat-happy.gif");
    }

    clearTimeout(inactivityTimer);
    inactivityTimer = setTimeout(() => {
        catAnimationElement.src = "/css/cat-chillout.gif";
        clickTimeElement.textContent = "awaiting input...";
    }, 1500);

    lastClickTime = now;
}

function setCatAnimation(src) {
    if (!catAnimationElement.src.endsWith(src)) 
    { 
        catAnimationElement.src = src;
    }
}

function DeleteFile() {
    counter = 0;

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    fetch("/CatFacts/DeleteFile", {
        method: "DELETE",
        headers: { "RequestVerificationToken": token }
    })
        .then(res => {
            if (res.ok) {
                consoleElement.querySelectorAll('.log-line').forEach(el => el.remove());
                TerminalMessage('> Plik z faktami o kotach został usunięty.', 'log-line log-line--error');

                counterValueElement.textContent = "0 faktów pobranych";
                fileSizeValueElement.textContent = "0 KB";
                timeToResponseValueElement.textContent = "0 ms";
                charCountValueElement.textContent = "0 znaków";
            } else {
                TerminalMessage('> Nie udało się usunąć pliku z faktami o kotach.', 'log-line log-line--error');
            }
        })
        .catch(error => console.error("Błąd:", error));
}
