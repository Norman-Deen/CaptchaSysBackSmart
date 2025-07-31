# AI-based CAPTCHA System (Backend)

Backend API for a smart CAPTCHA system that detects and blocks bots by analyzing mouse and touch behavior using a custom AI model.

⚠️ Private Project – Developed as part of a graduation thesis at EC Utbildning. Not open-source.

---

## 🎯 Project Overview

This backend complements the frontend CAPTCHA system that avoids traditional image-based verification. Instead, it collects user interaction data (mouse and touch), then applies a trained ML model to determine whether the user is human or a bot.

It is built with .NET and ML.NET and uses a lightweight structure that logs interaction data for ongoing model training and evaluation.

---

## 🔧 Features

- AI Detection using ML.NET
- Separate endpoints for mouse (checkbox) and touch (slider) data
- Logs data to: `/Logs/access-log.csv`
- REST API to manage logs
- CORS enabled for frontend communication
- Ping endpoint for uptime/health checks

---

## 📡 API Endpoints

| Method | Endpoint             | Description                          |
|--------|----------------------|--------------------------------------|
| POST   | /api/box             | Submit mouse interaction data        |
| POST   | /api/slider          | Submit touch (slider) data           |
| GET    | /api/log             | Retrieve all logged interaction data |
| DELETE | /api/log/{index}     | Delete a specific log entry          |
| GET    | /api/ping            | Health check / keep-alive            |

---

## 🚀 How to Run

1. Make sure you have [.NET 6 or later](https://dotnet.microsoft.com/en-us/download)
2. Run the backend locally with:

   ```bash
   dotnet run
   ```

3. Default behavior:
   - Logs saved to: `Logs/access-log.csv`
   - AI model file path: `ML/score-model.zip`

> The API is currently hosted here for testing:  
https://captchasysbacksmart.onrender.com/api/ping

---

## 🌐 Related Projects & Links

- Frontend Demo: https://norman-deen.github.io/CaptchaSysFrontSmart/
- Backend Health Check: https://captchasysbacksmart.onrender.com/api/ping
- Portfolio Website: https://www.pure-art.co

---

## 📚 Technologies Used

- .NET 8 (C#)
- ML.NET (Randomized PCA)
- RESTful API Design
- CSV-based data storage
- Hosted on Render (free plan with wake-up workaround)

---

## 📬 Contact

Norman Deen (Nour Altinawi)  
📧 Deen80@live.com  
🌍 https://www.pure-art.co  
🔗 https://www.linkedin.com/in/nour-tinawi

---

## 🧠 Disclaimer

This backend is part of an academic proof-of-concept developed for a final project.  
It is not intended for reuse or production environments.
