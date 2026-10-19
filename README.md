# AI Motion CAPTCHA API

Backend API for an experimental CAPTCHA system that analyzes user interaction behavior to help distinguish human activity from automated input.

> Academic proof-of-concept developed during my .NET Fullstack studies at EC Utbildning.

---

## Overview

This backend powers an AI-assisted CAPTCHA concept built around behavioral analysis instead of traditional image-based verification.

The system receives mouse and touch interaction data from the frontend and evaluates movement patterns using an ML.NET-based model.

The project explores how motion characteristics such as timing, speed variation, and interaction flow can be used as an alternative verification method.

---

## Core Features

* AI-based behavior analysis with ML.NET
* Separate interaction pipelines for desktop and mobile
* REST API architecture
* CSV-based interaction logging for training and evaluation
* Frontend communication via CORS-enabled endpoints
* Lightweight deployment structure
* Health-check endpoint for uptime monitoring

---

## API Endpoints

| Method | Endpoint           | Description                           |
| ------ | ------------------ | ------------------------------------- |
| POST   | `/api/box`         | Submit desktop mouse interaction data |
| POST   | `/api/slider`      | Submit mobile touch interaction data  |
| GET    | `/api/log`         | Retrieve stored interaction logs      |
| DELETE | `/api/log/{index}` | Delete a specific log entry           |
| GET    | `/api/ping`        | API health check                      |

---

## Technologies Used

* .NET 8 (C#)
* ML.NET
* REST API Design
* CSV-based logging system
* Render.com deployment
* Frontend integration with Vanilla JavaScript

---

## Project Purpose

The goal of this project was to explore practical integration between:

* AI-assisted verification systems
* Behavioral analytics
* Frontend and backend communication
* ML.NET model integration
* Real-world web interaction tracking

Rather than focusing on production deployment, the project was designed as a technical and research-oriented prototype.

---

## Related Links

* Frontend Demo:
  [GitHub Pages Demo](https://norman-deen.github.io/CaptchaSysFrontSmart/)

* Backend API:
  [Backend Health Check](https://captchasysbacksmart.onrender.com/api/ping)

* Portfolio Website:
  [Nourtin](https://nourtin.com)

---

## Contact

**Nour Tinawi (Norman Deen)**

* LinkedIn:
  [LinkedIn Profile](https://www.linkedin.com/in/nour-tinawi)

* Website:
  [Nourtin](https://nourtin.com)

---

## Notes

This project was developed as part of a graduation thesis focused on AI-assisted behavioral analysis and web-based verification systems. It serves as a research and demonstration project showcasing frontend, backend, and machine learning integration. 

## Local development and repository contents

Requires the .NET 8 SDK.

```bash
git clone https://github.com/Norman-Deen/CaptchaSysBackSmart.git
cd CaptchaSysBackSmart
dotnet restore CaptchaApi.csproj
dotnet run --project CaptchaApi.csproj
```

The application uses the `PORT` environment variable, with port `8080` as its default. The pretrained runtime model is retained at `ML/score-model.zip`; build outputs can be regenerated from source. Training is disabled at startup by default. The CSV log and the educational log-viewing workflow are retained as part of the graduation demonstration.

This research prototype does not establish production-grade bot detection or validated classification accuracy.
