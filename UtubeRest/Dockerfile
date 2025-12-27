FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER root

RUN apt-get update
RUN apt-get install -y --no-install-recommends nodejs
RUN apt-get install -y --no-install-recommends python3
RUN apt-get install -y --no-install-recommends python3-pip
RUN apt-get install -y --no-install-recommends python3-venv
RUN apt-get install -y --no-install-recommends pipx

RUN apt-get install -y --no-install-recommends ffmpeg
RUN rm -rf /var/lib/apt/lists/*

USER $APP_UID

RUN pipx install "yt-dlp[default]"
RUN pipx ensurepath 

ENV PATH="$PATH:/home/app/.local/bin"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UtubeRest/UtubeRest.csproj", "UtubeRest/"]
RUN dotnet restore "UtubeRest/UtubeRest.csproj"
COPY UtubeRest/. UtubeRest/
WORKDIR /src/UtubeRest
RUN dotnet build "UtubeRest.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UtubeRest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#RUN mkdir -p /home/app/cookies

#ENV ASPNETCORE_HTTPS_PORT 8081
#ENV ASPNETCORE_HTTP_PORT 8080

ENTRYPOINT ["dotnet", "UtubeRest.dll"]