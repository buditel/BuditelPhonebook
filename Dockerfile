# Step 1: Use .NET SDK for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Step 2: Copy the solution and all project files
COPY *.sln ./
COPY BuditelPhonebook.Common/*.csproj ./BuditelPhonebook.Common/
COPY BuditelPhonebook.Core/*.csproj ./BuditelPhonebook.Core/
COPY BuditelPhonebook.Infrastructure/*.csproj ./BuditelPhonebook.Infrastructure/
COPY BuditelPhonebook.IntegrationTests/*.csproj ./BuditelPhonebook.IntegrationTests/
COPY BuditelPhonebook.Tests/*.csproj ./BuditelPhonebook.Tests/
COPY BuditelPhonebook.Utilities/*.csproj ./BuditelPhonebook.Utilities/
COPY BuditelPhonebook.Web.ViewModels/*.csproj ./BuditelPhonebook.Web.ViewModels/
COPY BuditelPhonebook/*.csproj ./BuditelPhonebook/

# Step 3: Restore dependencies
RUN dotnet restore BuditelPhonebook/BuditelPhonebook.Web.csproj

# Step 4: Copy all source files
COPY . ./

# Step 5: Build and publish the app
WORKDIR /app/BuditelPhonebook
RUN dotnet publish -c Release -o out

# Step 6: Use a smaller runtime image for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Step 7: Copy built files from the build stage
COPY --from=build /app/BuditelPhonebook/out ./

# Step 8: Expose the web port (Render uses 8080)
EXPOSE 8080

# Step 9: Start the application
CMD ["dotnet", "BuditelPhonebook.dll"]
