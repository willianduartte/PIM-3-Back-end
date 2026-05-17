param(
    [Parameter(Mandatory = $true)]
    [string]$OldName,

    [Parameter(Mandatory = $true)]
    [string]$NewName,

    [string]$Root = ".",

    [switch]$Preview
)

$ErrorActionPreference = "Stop"

$rootPath = (Resolve-Path $Root).Path
Write-Host "Root: $rootPath"
Write-Host "OldName: $OldName"
Write-Host "NewName: $NewName"
Write-Host "Preview: $Preview"

# Folders/files to ignore
$excludeDirs = @(".git", ".vs", "bin", "obj", "node_modules")
$excludeFilePatterns = @("*.dll", "*.exe", "*.pdb", "*.so", "*.dylib", "*.png", "*.jpg", "*.jpeg", "*.gif", "*.ico", "*.pdf", "*.zip")

function Is-ExcludedPath {
    param([string]$Path)
    $parts = $Path -split '[\\/]'
    foreach ($d in $excludeDirs) {
        if ($parts -contains $d) { return $true }
    }
    return $false
}

function Is-ExcludedFile {
    param([System.IO.FileInfo]$File)
    foreach ($pattern in $excludeFilePatterns) {
        if ($File.Name -like $pattern) { return $true }
    }
    return $false
}

# 1) Replace content in files
$files = Get-ChildItem -Path $rootPath -Recurse -File | Where-Object {
    -not (Is-ExcludedPath $_.FullName) -and -not (Is-ExcludedFile $_)
}

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { continue }

    if ($content.Contains($OldName)) {
        $newContent = $content.Replace($OldName, $NewName)
        if ($Preview) {
            Write-Host "[PREVIEW] Update content: $($file.FullName)"
        } else {
            Set-Content -Path $file.FullName -Value $newContent -NoNewline
            Write-Host "Updated content: $($file.FullName)"
        }
    }
}

# 2) Rename files and directories (deepest first)
$items = Get-ChildItem -Path $rootPath -Recurse -Force | Where-Object {
    -not (Is-ExcludedPath $_.FullName)
} | Sort-Object { $_.FullName.Length } -Descending

foreach ($item in $items) {
    if ($item.Name -like "*$OldName*") {
        $newItemName = $item.Name.Replace($OldName, $NewName)
        $parent = Split-Path -Path $item.FullName -Parent
        $newPath = Join-Path $parent $newItemName

        if ($Preview) {
            Write-Host "[PREVIEW] Rename: $($item.FullName) -> $newPath"
        } else {
            Rename-Item -Path $item.FullName -NewName $newItemName
            Write-Host "Renamed: $($item.FullName) -> $newPath"
        }
    }
}

Write-Host "Done."
