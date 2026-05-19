FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NotificationWorker/NotificationWorker.csproj", "NotificationWorker/"]
RUN dotnet restore "NotificationWorker/NotificationWorker.csproj"
COPY . .
WORKDIR "/src/NotificationWorker"
RUN dotnet publish "NotificationWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationWorker.dll"]
