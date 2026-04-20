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


function GetFact() {
    fetch("/CatFacts/GetFact")
        .then(res => res.json())
        .then(data => {
            
            const newLine = document.createElement('div');
            newLine.className = 'log-line';
            newLine.textContent = "> " + data.fact;

            consoleElement.insertBefore(newLine, cursorElement);

            terminalWindow.scrollTop = terminalWindow.scrollHeight;

            counter++;
            counterValueElement.textContent = counter + " faktów pobranych";
            fileSizeValueElement.textContent = data.fileSizeKb + " KB";
            timeToResponseValueElement.textContent = data.timeToResponseMs + " ms";
            charCountValueElement.textContent = data.charCount + " znaków";
        })
        .catch(error => console.error("Błąd:", error));
}

function ClickTime() {
    const now = performance.now();
    let timeBetween = 0;
    if (lastClickTime !== null) {
        timeBetween = (now - lastClickTime).toFixed(0);
        clickTimeElement.textContent = `Czas od ostatniego kliknięcia: ${timeBetween}ms`;
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
        clickTimeElement.textContent = "Kliknij, aby pobrać fakt o kotach";
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
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    fetch("/CatFacts/DeleteFile", {
        method: "DELETE",
        headers: { "RequestVerificationToken": token }
    })
        .then(res => {
            if (res.ok) {
                alert("Plik z faktami o kotach został usunięty!");
                
                consoleElement.querySelectorAll('.log-line').forEach(el => el.remove());
                
                const msg = document.createElement('div');
                msg.className = 'log-line';
                msg.textContent = '> Plik z faktami o kotach został usunięty!';
                consoleElement.insertBefore(msg, cursorElement); 

                counterValueElement.textContent = "0 faktów pobranych";
                fileSizeValueElement.textContent = "0 KB";
                timeToResponseValueElement.textContent = "0 ms";
                charCountValueElement.textContent = "0 znaków";
            } else {
                alert("Nie udało się usunąć pliku z faktami o kotach.");
            }
        })
        .catch(error => console.error("Błąd:", error));
}
