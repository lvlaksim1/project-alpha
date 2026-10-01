param(
    [string]$InstallDir = (Join-Path $env:LOCALAPPDATA "Programs\Project Alpha"),
    [switch]$NoLaunch,
    [string]$ErrorPath
)

$ErrorActionPreference = "Stop"

$packageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$basesRoot = Join-Path $packageRoot "bases"
$repairRoot = Join-Path $packageRoot "repair"
$targetManifestPath = Join-Path $packageRoot "target-manifest.json"
$appExe = Join-Path $InstallDir "Baraban.exe"
$processName = "Baraban"
$userDataDir = Join-Path $env:LOCALAPPDATA "Baraban"
$userDrumsDir = Join-Path $userDataDir "Drums"
$installedDrumsDir = Join-Path $InstallDir "Drums"

function Write-UpdateError([string]$Message) {
    if (-not [string]::IsNullOrWhiteSpace($ErrorPath)) {
        try {
            $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
            [System.IO.File]::WriteAllText($ErrorPath, $Message, $encoding)
        }
        catch {
        }
    }
}

function Clear-UpdateError {
    if (-not [string]::IsNullOrWhiteSpace($ErrorPath)) {
        Remove-Item -LiteralPath $ErrorPath -Force -ErrorAction SilentlyContinue
    }
}

function Normalize-RelativePath([string]$Path) {
    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "Manifest contains an empty path."
    }

    $normalized = $Path.Replace("/", "\")
    if ([System.IO.Path]::IsPathRooted($normalized) -or
        $normalized -eq ".." -or
        $normalized.StartsWith("..\") -or
        $normalized.Contains("\..\")) {
        throw "Unsafe relative path in update manifest: $Path"
    }

    return $normalized
}

function Get-Sha256([string]$Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Test-Baseline([string]$Root, $Manifest) {
    foreach ($entry in @($Manifest.baseline)) {
        $relative = Normalize-RelativePath ([string]$entry.path)
        $fullPath = Join-Path $Root $relative

        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            return $false
        }

        $actual = Get-Sha256 $fullPath
        $expected = ([string]$entry.sha256).ToLowerInvariant()
        if ($actual -ne $expected) {
            return $false
        }
    }

    return $true
}

function Get-InstalledVersion {
    if (-not (Test-Path -LiteralPath $appExe -PathType Leaf)) {
        return $null
    }

    $info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($appExe)
    foreach ($candidate in @([string]$info.ProductVersion, [string]$info.FileVersion)) {
        if ($candidate -match '(\d+\.\d+\.\d+)') {
            return $Matches[1]
        }
    }

    return $null
}

function Migrate-EditableDrums {
    if (-not (Test-Path -LiteralPath $installedDrumsDir -PathType Container)) {
        return
    }

    New-Item -ItemType Directory -Path $userDrumsDir -Force | Out-Null
    Get-ChildItem -LiteralPath $installedDrumsDir -Filter "*.json" -File | ForEach-Object {
        $destination = Join-Path $userDrumsDir $_.Name
        if (-not (Test-Path -LiteralPath $destination)) {
            Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
        }
    }
}

function Find-ExactDelta {
    if (-not (Test-Path -LiteralPath $basesRoot -PathType Container)) {
        throw "В пакете обновления отсутствует каталог поддерживаемых базовых версий."
    }

    foreach ($dir in @(Get-ChildItem -LiteralPath $basesRoot -Directory | Sort-Object Name -Descending)) {
        $manifestPath = Join-Path $dir.FullName "update-manifest.json"
        if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
            continue
        }

        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.schema -ne "project-alpha-delta-v1") {
            continue
        }

        if (Test-Baseline $InstallDir $manifest) {
            return [pscustomobject]@{
                Root = $dir.FullName
                Manifest = $manifest
                Mode = "exact"
            }
        }
    }

    return $null
}

function Get-RepairPlan {
    $manifestPath = Join-Path $repairRoot "repair-manifest.json"
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "Точная сигнатура установленной версии не совпала, а безопасный repair-пакет отсутствует."
    }

    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    if ($manifest.schema -ne "project-alpha-repair-v1") {
        throw "Пакет восстановления имеет неизвестный формат."
    }

    $installedVersion = Get-InstalledVersion
    if ([string]::IsNullOrWhiteSpace($installedVersion)) {
        throw "Не удалось определить версию установленного Baraban.exe после несовпадения SHA-256."
    }

    $supported = @($manifest.supportedVersions | ForEach-Object { [string]$_ })
    if ($supported -notcontains $installedVersion) {
        throw "Установлена версия $installedVersion, которая не поддерживается этим обновлением. Поддерживаются: $($supported -join ', ')."
    }

    return [pscustomobject]@{
        Root = $repairRoot
        Manifest = $manifest
        Mode = "repair"
        InstalledVersion = $installedVersion
    }
}

function Assert-TargetState($TargetManifest) {
    foreach ($entry in @($TargetManifest.files)) {
        if ([bool]$entry.mutable) {
            continue
        }

        $relative = Normalize-RelativePath ([string]$entry.path)
        $fullPath = Join-Path $InstallDir $relative
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            throw "Финальная проверка не пройдена: отсутствует '$relative'."
        }

        $actual = Get-Sha256 $fullPath
        $expected = ([string]$entry.sha256).ToLowerInvariant()
        if ($actual -ne $expected) {
            throw "Финальная проверка SHA-256 не пройдена для '$relative'."
        }
    }
}

function Apply-Plan($selected, $TargetManifest) {
    $manifest = $selected.Manifest
    $payloadRoot = Join-Path $selected.Root "payload"

    if ($selected.Mode -eq "exact") {
        Write-Host "Detected exact installed base: $($manifest.fromTag)"
        Write-Host "Target version: $($manifest.toTag)"
    }
    else {
        Write-Host "Exact base hash did not match; using safe repair mode for v$($selected.InstalledVersion)."
        Write-Host "Target version: $($manifest.targetTag)"
    }

    $running = @(Get-Process -Name $processName -ErrorAction SilentlyContinue)
    foreach ($process in $running) {
        try {
            if ($process.MainWindowHandle -ne 0) {
                [void]$process.CloseMainWindow()
            }
        }
        catch {
        }
    }

    $deadline = [DateTime]::UtcNow.AddSeconds(8)
    while ((Get-Process -Name $processName -ErrorAction SilentlyContinue) -and [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 250
    }

    Get-Process -Name $processName -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

    $backupRoot = Join-Path $env:TEMP ("ProjectAlpha-update-" + [Guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null
    $hadOriginal = @{}

    try {
        foreach ($entry in @($manifest.files)) {
            $relative = Normalize-RelativePath ([string]$entry.path)
            $destination = Join-Path $InstallDir $relative
            $source = Join-Path $payloadRoot $relative

            if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
                throw "Update payload is missing '$relative'."
            }

            $payloadHash = Get-Sha256 $source
            if ($payloadHash -ne ([string]$entry.sha256).ToLowerInvariant()) {
                throw "Update payload hash mismatch for '$relative'."
            }

            $exists = Test-Path -LiteralPath $destination -PathType Leaf
            $hadOriginal[$relative] = $exists

            if ($exists) {
                $backupPath = Join-Path $backupRoot $relative
                New-Item -ItemType Directory -Path (Split-Path -Parent $backupPath) -Force | Out-Null
                Copy-Item -LiteralPath $destination -Destination $backupPath -Force
            }
        }

        foreach ($relativeRaw in @($manifest.delete)) {
            $relative = Normalize-RelativePath ([string]$relativeRaw)
            $destination = Join-Path $InstallDir $relative
            $exists = Test-Path -LiteralPath $destination -PathType Leaf
            $hadOriginal[$relative] = $exists

            if ($exists) {
                $backupPath = Join-Path $backupRoot $relative
                New-Item -ItemType Directory -Path (Split-Path -Parent $backupPath) -Force | Out-Null
                Copy-Item -LiteralPath $destination -Destination $backupPath -Force
            }
        }

        foreach ($entry in @($manifest.files)) {
            $relative = Normalize-RelativePath ([string]$entry.path)
            $destination = Join-Path $InstallDir $relative
            $source = Join-Path $payloadRoot $relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
            Copy-Item -LiteralPath $source -Destination $destination -Force
        }

        foreach ($relativeRaw in @($manifest.delete)) {
            $relative = Normalize-RelativePath ([string]$relativeRaw)
            $destination = Join-Path $InstallDir $relative
            if (Test-Path -LiteralPath $destination) {
                Remove-Item -LiteralPath $destination -Force
            }
        }

        Assert-TargetState $TargetManifest
        Write-Host "Update complete in $($selected.Mode) mode."
    }
    catch {
        Write-Warning "Update failed. Rolling back changed files."

        foreach ($entry in @($manifest.files)) {
            $relative = Normalize-RelativePath ([string]$entry.path)
            $destination = Join-Path $InstallDir $relative
            $backupPath = Join-Path $backupRoot $relative

            if ($hadOriginal[$relative]) {
                if (Test-Path -LiteralPath $backupPath -PathType Leaf) {
                    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
                    Copy-Item -LiteralPath $backupPath -Destination $destination -Force
                }
            }
            elseif (Test-Path -LiteralPath $destination) {
                Remove-Item -LiteralPath $destination -Force
            }
        }

        foreach ($relativeRaw in @($manifest.delete)) {
            $relative = Normalize-RelativePath ([string]$relativeRaw)
            $destination = Join-Path $InstallDir $relative
            $backupPath = Join-Path $backupRoot $relative

            if ($hadOriginal[$relative] -and (Test-Path -LiteralPath $backupPath -PathType Leaf)) {
                New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
                Copy-Item -LiteralPath $backupPath -Destination $destination -Force
            }
        }

        throw
    }
    finally {
        Remove-Item -LiteralPath $backupRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

try {
    Clear-UpdateError

    if (-not (Test-Path -LiteralPath $appExe -PathType Leaf)) {
        throw "Project Alpha не найден в '$InstallDir'. Для первой установки используйте полный Setup."
    }

    if (-not (Test-Path -LiteralPath $targetManifestPath -PathType Leaf)) {
        throw "В пакете обновления отсутствует финальный SHA-256 манифест."
    }

    $targetManifest = Get-Content -LiteralPath $targetManifestPath -Raw | ConvertFrom-Json
    if ($targetManifest.schema -ne "project-alpha-publish-v1") {
        throw "Финальный SHA-256 манифест имеет неизвестный формат."
    }

    Migrate-EditableDrums

    $selected = Find-ExactDelta
    if ($null -eq $selected) {
        $selected = Get-RepairPlan
    }

    Apply-Plan $selected $targetManifest

    if (-not $NoLaunch) {
        Start-Process -FilePath $appExe
    }

    Clear-UpdateError
    exit 0
}
catch {
    $message = $_.Exception.Message
    Write-UpdateError $message
    [Console]::Error.WriteLine($message)
    exit 1
}
