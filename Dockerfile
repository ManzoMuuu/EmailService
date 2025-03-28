# Costruzione dell'applicazione in un container di build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia i file .csproj e ripristina le dipendenze
COPY ["src/EmailService.API/EmailService.API.csproj", "src/EmailService.API/"]
COPY ["src/EmailService.Core/EmailService.Core.csproj", "src/EmailService.Core/"]
COPY ["src/EmailService.Infrastructure/EmailService.Infrastructure.csproj", "src/EmailService.Infrastructure/"]
RUN dotnet restore "src/EmailService.API/EmailService.API.csproj"

# Copia il resto dei file e compila
COPY . .
WORKDIR "/src/src/EmailService.API"
RUN dotnet build "EmailService.API.csproj" -c Release -o /app/build

# Pubblicazione
FROM build AS publish
RUN dotnet publish "EmailService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Creare cartella per i log
RUN mkdir -p /app/logs && \
    chmod 777 /app/logs

# Copia l'app pubblicata e configura l'utente non-root
COPY --from=publish /app/publish .

# Definisci le variabili d'ambiente per la configurazione
ENV ASPNETCORE_ENVIRONMENT=Production \
    EMAILAPI_EmailSettings__SmtpServer=smtp.gmail.com \
    EMAILAPI_EmailSettings__SmtpPort=587 \
    EMAILAPI_EmailSettings__UseSsl=true \
    EMAILAPI_EmailSettings__SenderName="Email Service" \
    EMAILAPI_EmailSettings__Username="" \
    EMAILAPI_EmailSettings__Password="" \
    EMAILAPI_EmailSettings__SenderEmail=""

# Imposta l'entrypoint
ENTRYPOINT ["dotnet", "EmailService.API.dll"]

# Uso:
# 1. Costruisci l'immagine:
# docker build -t emailservice:latest .
#
# 2. Esegui il container:
# docker run -d -p 5001:80 \
#   -e EMAILAPI_EmailSettings__Username="tua-email@gmail.com" \
#   -e EMAILAPI_EmailSettings__Password="tua-password-app" \
#   -e EMAILAPI_EmailSettings__SenderEmail="tua-email@gmail.com" \
#   --name emailservice \
#   emailservice:latest