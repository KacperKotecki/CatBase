# 🐱 CatBase

You are a secret agent. Your mission: collect classified intel on cats.

The **red button** connects to a covert feline database and extracts one random cat fact. Every fact is automatically logged to a classified file on the server.

The **blue button** destroys all evidence. The file is wiped. No trace remains.

The terminal shows everything. The cat watches.

---
<img width="2940" height="2250" alt="catbase" src="https://github.com/user-attachments/assets/dd8c9d2a-7286-4cc2-b490-5096f18bbd4a" />
---

## Run locally

```bash
git clone https://github.com/KacperKotecki/CatBase
cd catbase
dotnet run
```

---

## Configuration

Edit `appsettings.json`:

```json
{
  "CatFactApi": {
    "FactUrl": "https://catfact.ninja/fact",
    "OutputFileName": "output/catfacts.txt"
  }
}
```

