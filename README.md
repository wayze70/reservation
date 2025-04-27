# 🗓️ Rezervario

**Rezervario** je webová aplikace určená pro správu rezervací. Umožňuje vytváření, správu a přehledné zobrazení rezervací pro různé typy služeb a akcí.

Aplikace běží online na adrese:  
🔗 [https://www.rezervario.cz](https://www.rezervario.cz)

---

## 🚀 Lokální spuštění

Pro správné spuštění projektu lokálně je potřeba:
1. Naklonovat repozitář.
2. Nakonfigurovat soubor `appsettings.json` v projektu `Reservation.Api` podle níže uvedeného příkladu.
3. Spustit migrační skripty databáze.
4. Spustit backend a frontend. (BE je možné spustit v dockeru)

### Příklad `appsettings.json`

```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=db;Username=username;Password=secretpassword"
  },
  "Jwt": {
    "Key": "your256BitSecretKeyYour256BitSecretKeyYour256BitSecretKey",
    "Issuer": "yourIssuer",
    "Audience": "yourAudience"
  },
  "FrontendUrl": "https://localhost:port",
  "BackendUrl": "https://localhost:port",
  "EmailSettings": {
    "SenderEmail": "example@domain.com",
    "SmtpPassword": "yourSmtpPassword"
  }
}
```

> ⚠️ **Poznámka:**  
> Hodnota `Jwt:Key` musí mít minimálně 256 bitů (tj. 32 bytů).

---

## 🧩 Požadavky

- .NET 8 SDK
- PostgreSQL
- SMTP server pro odesílání e-mailových notifikací  

---

## ⚙️ Použité technologie

- ASP.NET Core Web API (.NET 8)  
- Entity Framework Core  
- PostgreSQL  
- JWT (JSON Web Token)  
- SMTP pro e-mailové notifikace  
- Docker (volitelné)  
- Blazor WebAssembly

---

## 📌 Autor

Vyvinuto jako součást bakalářské práce  
Autor: Michal Syřiště
Vedoucí práce: Mgr. Vojtěch Vorel, Ph.D.  
Fakulta informatiky a managementu, Univerzita Hradec Králové  
