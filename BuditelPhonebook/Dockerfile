# Използваме .NET 8 SDK за билдване
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Копираме .sln и всички проекти
COPY . ./
WORKDIR /app/BuditelPhonebook/BuditelPhonebook  # Отиваме в главния уеб проект

# Възстановяваме зависимостите и билдваме проекта
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Използваме .NET runtime за изпълнение
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/BuditelPhonebook/BuditelPhonebook/out .

# Стартиране на приложението
CMD ["dotnet", "BuditelPhonebook.dll"]
