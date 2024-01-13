FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY *.sln ./

# Add any new projects to the build here
COPY GE_Chatapp/GE_Chatapp/*.csproj ./GE_Chatapp/GE_Chatapp/
COPY GE_Chatapp/GE_Chatapp.Client/*.csproj ./GE_Chatapp/GE_Chatapp.Client/
COPY Chatapp.Shared/*.csproj ./Chatapp.Shared/
COPY Chat.Unit_Test/*.csproj ./Chat.Unit_Test/
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet build -c Release -o out
