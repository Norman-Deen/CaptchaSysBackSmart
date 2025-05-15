# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and publish (restore + build)
COPY . ./
RUN dotnet publish -c Release -o out

# Use a runtime-only image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port (default for Kestrel)
EXPOSE 80

# Start the app
ENTRYPOINT ["dotnet", "CaptchaApi.dll"]
