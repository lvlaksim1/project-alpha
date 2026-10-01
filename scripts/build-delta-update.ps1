param(
    [string]$BasePublishDir,

    [string]$BaseManifestPath,

    [Parameter(Mandatory = $true)]
    [string]$CurrentPublishDir,

    [Parameter(Mandatory = $true)]
    [string]$BaseTag,

    [Parameter(Mandatory = $true)]
    [string]$TargetTag,

    [Parameter(Mandatory = $true)]
    [string]$TargetCommit,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($BasePublishDir) -eq [string]::IsNullOrWhiteSpace($BaseManifestPath)) {
    throw "Specify exactly one of BasePublishDir or BaseManifestPath."
}

function Test-MutablePath([string]$Path) {
    return $Path.Replace("\", "/").StartsWith("Drums/", [StringComparison]::OrdinalIgnoreCase)
}

function Get-PublishMap([string]$Root) {
    $rootPath = (Resolve-Path $Root).Path.TrimEnd("\")
    $map = @{}

    Get-ChildItem -LiteralPath $rootPath -File -Recurse | ForEach-Object {
        $relative = $_.FullName.Substring($rootPath.Length).TrimStart("\").Replace("\", "/")
        $map[$relative] = [pscustomobject]@{
            path = $relative
            sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            size = $_.Length
            mutable = (Test-MutablePath $relative)
        }
    }

    return $map
}

function Get-ManifestMap([string]$Path) {
    $manifest = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    if ($manifest.schema -ne "project-alpha-publish-v1") {
        throw "Unsupported base publish manifest schema in '$Path'."
    }

    if ([string]$manifest.tag -ne $BaseTag) {
        throw "Base manifest tag '$($manifest.tag)' does not match expected '$BaseTag'."
    }

    $map = @{}
    foreach ($entry in @($manifest.files)) {
        $relative = ([string]$entry.path).Replace("\", "/")
        $map[$relative] = [pscustomobject]@{
            path = $relative
            sha256 = ([string]$entry.sha256).ToLowerInvariant()
            size = [long]$entry.size
            mutable = [bool]$entry.mutable
        }
    }

    return $map
}

$base = if (-not [string]::IsNullOrWhiteSpace($BaseManifestPath)) {
    Get-ManifestMap $BaseManifestPath
}
else {
    Get-PublishMap $BasePublishDir
}

$current = Get-PublishMap $CurrentPublishDir

$changed = @()
foreach ($path in ($current.Keys | Sort-Object)) {
    if (-not $base.ContainsKey($path) -or $base[$path].sha256 -ne $current[$path].sha256) {
        $changed += $current[$path]
    }
}

$deleted = @($base.Keys | Where-Object { -not $current.ContainsKey($_) } | Sort-Object)
$baseline = @($base.Values | Where-Object { -not $_.mutable } | Sort-Object path)

if (Test-Path -LiteralPath $OutputDirectory) {
    Remove-Item -LiteralPath $OutputDirectory -Recurse -Force
}

$payload = Join-Path $OutputDirectory "payload"
New-Item -ItemType Directory -Path $payload -Force | Out-Null

foreach ($entry in $changed) {
    $source = Join-Path $CurrentPublishDir ($entry.path.Replace("/", "\"))
    $destination = Join-Path $payload ($entry.path.Replace("/", "\"))
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination -Force
}

$manifest = [ordered]@{
    schema = "project-alpha-delta-v1"
    fromTag = $BaseTag
    toTag = $TargetTag
    targetCommit = $TargetCommit
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("o")
    baseline = $baseline
    files = $changed
    delete = $deleted
}

$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutputDirectory "update-manifest.json") -Encoding UTF8

$payloadBytes = ($changed | Measure-Object -Property size -Sum).Sum
if ($null -eq $payloadBytes) { $payloadBytes = 0 }

$baseSource = if (-not [string]::IsNullOrWhiteSpace($BaseManifestPath)) { "exact manifest" } else { "reconstructed publish" }
Write-Host "Delta $BaseTag -> $TargetTag ($baseSource)"
Write-Host "Changed files: $($changed.Count)"
Write-Host "Deleted files: $($deleted.Count)"
Write-Host "Payload bytes: $payloadBytes"
Write-Host "Staging: $OutputDirectory"
