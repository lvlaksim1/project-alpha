param(
    [Parameter(Mandatory = $true)]
    [string]$CurrentPublishDir,

    [Parameter(Mandatory = $true)]
    [string]$TargetManifestPath,

    [Parameter(Mandatory = $true)]
    [string]$DeltaBasesRoot,

    [Parameter(Mandatory = $true)]
    [string]$BasesJsonPath,

    [Parameter(Mandatory = $true)]
    [string]$TargetTag,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"

function Normalize-Path([string]$Path) {
    return $Path.Replace("\", "/")
}

$targetManifest = Get-Content -LiteralPath $TargetManifestPath -Raw | ConvertFrom-Json
if ($targetManifest.schema -ne "project-alpha-publish-v1") {
    throw "Unsupported target publish manifest schema."
}
if ([string]$targetManifest.tag -ne $TargetTag) {
    throw "Target manifest tag '$($targetManifest.tag)' does not match '$TargetTag'."
}

$target = @{}
foreach ($entry in @($targetManifest.files)) {
    if ([bool]$entry.mutable) { continue }
    $path = Normalize-Path ([string]$entry.path)
    $target[$path] = [pscustomobject]@{
        path = $path
        sha256 = ([string]$entry.sha256).ToLowerInvariant()
        size = [long]$entry.size
        mutable = $false
    }
}

$bases = @(Get-Content -LiteralPath $BasesJsonPath -Raw | ConvertFrom-Json)
if ($bases.Count -eq 0) {
    throw "No supported base versions were supplied."
}

$baseMaps = @{}
$deleteSet = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)

foreach ($base in $bases) {
    $safe = ([string]$base.tag) -replace '[^A-Za-z0-9._-]', '_'
    $manifestPath = Join-Path (Join-Path $DeltaBasesRoot $safe) "update-manifest.json"
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "Delta manifest missing for $($base.tag)."
    }

    $delta = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $map = @{}
    foreach ($entry in @($delta.baseline)) {
        $path = Normalize-Path ([string]$entry.path)
        $map[$path] = ([string]$entry.sha256).ToLowerInvariant()
        if (-not $target.ContainsKey($path)) {
            [void]$deleteSet.Add($path)
        }
    }
    $baseMaps[[string]$base.version] = $map
}

$repairPaths = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
foreach ($path in $target.Keys) {
    foreach ($version in $baseMaps.Keys) {
        $map = $baseMaps[$version]
        if (-not $map.ContainsKey($path) -or $map[$path] -ne $target[$path].sha256) {
            [void]$repairPaths.Add($path)
            break
        }
    }
}

$repairRoot = Join-Path $OutputDirectory "repair"
if (Test-Path -LiteralPath $repairRoot) {
    Remove-Item -LiteralPath $repairRoot -Recurse -Force
}
$payloadRoot = Join-Path $repairRoot "payload"
New-Item -ItemType Directory -Path $payloadRoot -Force | Out-Null

$repairFiles = @()
foreach ($path in @($repairPaths | Sort-Object)) {
    $entry = $target[$path]
    $source = Join-Path $CurrentPublishDir ($path.Replace("/", "\"))
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
        throw "Target publish file missing: $path"
    }
    $destination = Join-Path $payloadRoot ($path.Replace("/", "\"))
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination -Force
    $repairFiles += $entry
}

$repairManifest = [ordered]@{
    schema = "project-alpha-repair-v1"
    targetTag = $TargetTag
    supportedVersions = @($bases | ForEach-Object { [string]$_.version })
    files = $repairFiles
    delete = @($deleteSet | Sort-Object)
}

$repairManifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $repairRoot "repair-manifest.json") -Encoding UTF8
Copy-Item -LiteralPath $TargetManifestPath -Destination (Join-Path $OutputDirectory "target-manifest.json") -Force

$payloadBytes = ($repairFiles | Measure-Object -Property size -Sum).Sum
if ($null -eq $payloadBytes) { $payloadBytes = 0 }

Write-Host "Safe repair bridge for $TargetTag"
Write-Host "Supported versions: $((@($repairManifest.supportedVersions)) -join ', ')"
Write-Host "Repair files: $($repairFiles.Count)"
Write-Host "Repair delete paths: $($deleteSet.Count)"
Write-Host "Repair payload bytes: $payloadBytes"
