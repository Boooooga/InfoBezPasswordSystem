# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем файл проекта и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем остальные файлы и собираем приложение
COPY . ./
RUN dotnet publish -c Release -o out

# Этап исполнения
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Копируем результаты сборки
COPY --from=build /app/out .

# Открываем порт
EXPOSE 80

# Запуск приложения
ENTRYPOINT ["dotnet", "InfoBezPasswordSystem.dll"]