
# CAPTCHA Backend

Backend API for a smart CAPTCHA system that detects and blocks bots based on mouse and touch behavior analytics.

> ⚠️ Private project – not open-source.

## Features

- Receives and logs mouse & touch interaction data  
- Classifies behavior as **human** or **robot**  
- Uses ML model (Randomized PCA via ML.NET)  
- Logs data in `.csv` format with metadata  
- Provides log access and deletion via REST API  
- CORS support for frontend integration  

## Endpoints

- `POST /api/box` – Mouse data submission  
- `POST /api/slider` – Touch data submission  
- `GET /api/log` – Get log entries  
- `DELETE /api/log/{index}` – Delete specific log entry  

## Usage

1. Run with:
   ```bash
   dotnet run
````

2. Logs are saved in: `Logs/access-log.csv`

3. ML model file path: `ML/score-model.zip`

## Requirements

* .NET 6 or later
* ML.NET
* Hosting: Local or Render/Vercel compatible

## Contact

**Norman Deen**
📧 [Deen80@live.com](mailto:Deen80@live.com)
🌍 [LinkedIn](https://www.linkedin.com/in/nour-tinawi)

**hasan Sero**
