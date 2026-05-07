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
  [GitHub Pages Demo](https://norman-deen.github.io/CaptchaSysFrontSmart/?utm_source=chatgpt.com)

* Backend API:
  [Backend Health Check](https://captchasysbacksmart.onrender.com/api/ping?utm_source=chatgpt.com)

* Portfolio Website:
  [Pure-Art.co](https://www.pure-art.co?utm_source=chatgpt.com)

---

## Contact

**Nour Tinawi (Norman Deen)**

* LinkedIn:
  [LinkedIn Profile](https://www.linkedin.com/in/nour-tinawi?utm_source=chatgpt.com)

* Website:
  [Pure-Art.co](https://www.pure-art.co?utm_source=chatgpt.com)

---

## Notes

This project was developed as part of a graduation thesis focused on AI-assisted behavioral analysis and web-based verification systems. It serves as a research and demonstration project showcasing frontend, backend, and machine learning integration. 
