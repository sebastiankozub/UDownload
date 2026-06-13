# Docker Volume Mount Troubleshooting Script

Write-Host "=== Docker Volume Mount Verification ===" -ForegroundColor Cyan
Write-Host ""

# 1. Check if cookies directory exists on host
Write-Host "1. Checking host cookies directory..." -ForegroundColor Yellow
$cookiesPath = "D:\repos\udownload\cookies"
if (Test-Path $cookiesPath) {
    Write-Host "   ? Cookies directory exists: $cookiesPath" -ForegroundColor Green
    Get-ChildItem $cookiesPath | ForEach-Object {
        Write-Host "     - $($_.Name) ($($_.Length) bytes)" -ForegroundColor Gray
    }
} else {
    Write-Host "   ? Cookies directory NOT found: $cookiesPath" -ForegroundColor Red
    Write-Host "   Creating directory..." -ForegroundColor Yellow
    New-Item -Path $cookiesPath -ItemType Directory -Force
}
Write-Host ""

# 2. Check if youtube.txt exists
Write-Host "2. Checking youtube.txt file..." -ForegroundColor Yellow
$cookieFile = Join-Path $cookiesPath "youtube.txt"
if (Test-Path $cookieFile) {
    Write-Host "   ? Cookie file exists: $cookieFile" -ForegroundColor Green
    $fileInfo = Get-Item $cookieFile
    Write-Host "     Size: $($fileInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "     Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "   ? Cookie file NOT found: $cookieFile" -ForegroundColor Red
    Write-Host "   ? You need to export YouTube cookies to this file" -ForegroundColor Yellow
}
Write-Host ""

# 3. Check running containers
Write-Host "3. Checking running Docker containers..." -ForegroundColor Yellow
$containers = docker ps --filter "name=utuberest" --format "{{.ID}}|{{.Names}}|{{.Status}}"
if ($containers) {
    foreach ($container in $containers) {
        $parts = $container -split '\|'
        $containerId = $parts[0]
        $containerName = $parts[1]
        $status = $parts[2]
        
        Write-Host "   ? Container found: $containerName ($containerId)" -ForegroundColor Green
        Write-Host "     Status: $status" -ForegroundColor Gray
        
        # Check volume mounts
        Write-Host "   Checking volume mounts..." -ForegroundColor Yellow
        $inspect = docker inspect $containerId | ConvertFrom-Json
        $mounts = $inspect[0].Mounts
        
        $cookieMount = $mounts | Where-Object { $_.Destination -eq "/app/cookies" }
        if ($cookieMount) {
            Write-Host "   ? Cookie volume is mounted!" -ForegroundColor Green
            Write-Host "     Host: $($cookieMount.Source)" -ForegroundColor Gray
            Write-Host "     Container: $($cookieMount.Destination)" -ForegroundColor Gray
            Write-Host "     Mode: $($cookieMount.Mode)" -ForegroundColor Gray
        } else {
            Write-Host "   ? Cookie volume NOT mounted!" -ForegroundColor Red
            Write-Host "   Check launchSettings.json dockerRunArgs" -ForegroundColor Yellow
        }
        
        # Check if cookies exist inside container
        Write-Host "   Checking files inside container..." -ForegroundColor Yellow
        try {
            $containerFiles = docker exec $containerId ls -la /app/cookies 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   Files in /app/cookies:" -ForegroundColor Gray
                $containerFiles | ForEach-Object { Write-Host "     $_" -ForegroundColor Gray }
            } else {
                Write-Host "   ? Cannot access /app/cookies in container" -ForegroundColor Red
                Write-Host "     $containerFiles" -ForegroundColor Red
            }
        } catch {
            Write-Host "   ? Error checking container files: $_" -ForegroundColor Red
        }
    }
} else {
    Write-Host "   ? No running UtubeRest containers found" -ForegroundColor Red
    Write-Host "   Start the container from Visual Studio (F5)" -ForegroundColor Yellow
}
Write-Host ""

# 4. Check launchSettings.json
Write-Host "4. Checking launchSettings.json..." -ForegroundColor Yellow
$launchSettingsPath = "D:\repos\udownload\UtubeRest\Properties\launchSettings.json"
if (Test-Path $launchSettingsPath) {
    $launchSettings = Get-Content $launchSettingsPath -Raw | ConvertFrom-Json
    $dockerProfile = $launchSettings.profiles.'Container (Dockerfile)'
    
    if ($dockerProfile.dockerRunArgs) {
        Write-Host "   ? dockerRunArgs found:" -ForegroundColor Green
        Write-Host "     $($dockerProfile.dockerRunArgs)" -ForegroundColor Gray
        
        if ($dockerProfile.dockerRunArgs -match "cookies") {
            Write-Host "   ? Contains cookies volume mount" -ForegroundColor Green
        } else {
            Write-Host "   ? Does NOT contain cookies volume mount" -ForegroundColor Red
        }
    } else {
        Write-Host "   ? dockerRunArgs NOT found in Container (Dockerfile) profile" -ForegroundColor Red
    }
} else {
    Write-Host "   ? launchSettings.json not found" -ForegroundColor Red
}
Write-Host ""

# 5. Recommendations
Write-Host "=== Recommendations ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "To fix volume mounting issues:" -ForegroundColor Yellow
Write-Host "1. Stop any running containers (Shift+F5 in Visual Studio)" -ForegroundColor White
Write-Host "2. Clean Docker cache: docker system prune -f" -ForegroundColor White
Write-Host "3. Rebuild in Visual Studio: Build > Rebuild Solution" -ForegroundColor White
Write-Host "4. Start with F5 (ensure 'Container (Dockerfile)' is selected)" -ForegroundColor White
Write-Host "5. Check Docker Desktop > Containers > UtubeRest > Bind mounts tab" -ForegroundColor White
Write-Host ""
Write-Host "If still not working:" -ForegroundColor Yellow
Write-Host "• Check Docker Desktop file sharing: Settings > Resources > File Sharing" -ForegroundColor White
Write-Host "• Ensure D:\ drive is shared with Docker" -ForegroundColor White
Write-Host "• Try running as administrator" -ForegroundColor White
Write-Host ""
