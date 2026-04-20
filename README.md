# 🐱 CatBase

You are a secret agent. Your mission: collect classified intel on cats.

The **red button** connects to a covert feline database and extracts one random cat fact. Every fact is automatically logged to a classified file on the server.

The **blue button** destroys all evidence. The file is wiped. No trace remains.

The terminal shows everything. The cat watches.

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

