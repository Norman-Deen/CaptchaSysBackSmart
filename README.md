# AI-based CAPTCHA System (Backend)

Backend API for a smart CAPTCHA system that detects and blocks bots by analyzing mouse and touch behavior using a custom AI model.

> ⚠️ **Private Project** – Developed as part of a graduation thesis at EC Utbildning. Not open-source.

---

## 🎯 Project Overview

This backend complements the frontend CAPTCHA system that avoids traditional image-based verification. Instead, it collects user interaction data (mouse and touch), then applies a trained ML model to determine whether the user is human or a bot.

It is built with .NET and ML.NET and uses a lightweight structure that logs interaction data for ongoing model training and evaluation.

---

## 🔧 Features

- 🧠 **AI Detection**: Human vs. bot classification using ML.NET  
- 📩 **Data Endpoints**: Separate routes for checkbox (mouse) and slider (touch) data  
- 📁 **CSV Logging**: All interaction data is stored in `/Logs/access-log.csv`  
- 🔄 **REST API**: To view and manage stored logs  
- 🤝 **CORS Support**: For full integration with the frontend  
- ⚙️ **Ping Endpoint**: For uptime and health checks

---

## 📡 API Endpoints

| Method | Endpoint             | Description                          |
|--------|----------------------|--------------------------------------|
| POST   | `/api/box`           | Submit mouse interaction data        |
| POST   | `/api/slider`        | Submit touch (slider) data           |
| GET    | `/api/log`           | Retrieve all logged interaction data |
| DELETE | `/api/log/{index}`   | Delete a specific log entry          |
| GET    | `/api/ping`          | Health check / keep-alive            |

---

## 🚀 How to Run

1. Make sure you have [.NET 6 or later](https://dotnet.microsoft.com/en-us/download)
2. Run the backend locally with:
   ```bash
   dotnet run
````

3. Default behavior:

   * Logs saved to: `Logs/access-log.csv`
   * AI model file path: `ML/score-model.zip`

> The API is currently hosted via [Render.com](https://captchasysbacksmart.onrender.com/api/ping) for public testing.

---

## 🌐 Related Projects & Links

* 🔹 **Frontend Demo**: [CAPTCHA Frontend (GitHub Pages)](https://norman-deen.github.io/CaptchaSysFrontSmart/)
* 🔹 **Backend Health Check**: [Ping Endpoint](https://captchasysbacksmart.onrender.com/api/ping)
* 🔹 **Portfolio Website**: [pure-art.co](https://www.pure-art.co)

---

## 📚 Technologies Used

* .NET 8 (C#)
* ML.NET (Randomized PCA)
* RESTful API Design
* CSV-based data storage
* Hosted on Render (free plan with wake-up workaround)

---

## 📬 Contact

**👨‍💻 Norman Deen (Nour Altinawi)**
📧 [Deen80@live.com](mailto:Deen80@live.com)
🌍 [pure-art.co](https://www.pure-art.co)
🔗 [LinkedIn](https://www.linkedin.com/in/nour-tinawi)

---

## 🧠 Disclaimer

This backend is part of an academic proof-of-concept developed for a final project. It is not intended for reuse or production environments.
