# Използваме официалния .NET 8 SDK образ за билдване
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Копираме csproj и възстановяваме зависимостите
COPY *.csproj ./
RUN dotnet restore

# Копираме останалия код и билдваме
COPY . ./
RUN dotnet publish -c Release -o out

# Използваме по-лек .NET 8 runtime образ за стартиране
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Изпълняваме приложението
CMD ["dotnet", "BuditelPhonebook.dll"]
