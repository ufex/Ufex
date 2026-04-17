param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Files
)

$repoRoot = Split-Path -Path $PSScriptRoot -Parent
$schemaPath = Join-Path $repoRoot "src/Ufex.FileType/config.xsd"

if (-not (Test-Path -LiteralPath $schemaPath -PathType Leaf)) {
    Write-Host "Schema file not found: $schemaPath"
    exit 1
}

if (-not $Files -or $Files.Count -eq 0) {
    $configDir = Join-Path $repoRoot "config"
    $targetFiles = Get-ChildItem -Path $configDir -Filter "*.xml" -File | ForEach-Object { $_.FullName }
}
else {
    $targetFiles = @()
    foreach ($file in $Files) {
        $resolvedPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($file)
        $targetFiles += $resolvedPath
    }
}

if (-not $targetFiles -or $targetFiles.Count -eq 0) {
    Write-Host "No files to validate."
    exit 0
}

$hasFailures = $false

foreach ($filePath in $targetFiles) {
    if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
        Write-Host "Invalid: $filePath (file not found)"
        $hasFailures = $true
        continue
    }

    $schemaStream = [System.IO.File]::OpenRead($schemaPath)
    try {
        $schema = [System.Xml.Schema.XmlSchema]::Read($schemaStream, $null)
    }
    finally {
        $schemaStream.Close()
    }
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.Schemas.Add($schema) | Out-Null
    $settings.ValidationType = [System.Xml.ValidationType]::Schema

    $validationErrors = New-Object System.Collections.Generic.HashSet[string]
    $validationHandler = [System.Xml.Schema.ValidationEventHandler]{
        param($sender, $e)
        $null = $validationErrors.Add("$($e.Severity): $($e.Message)")
    }
    $settings.add_ValidationEventHandler($validationHandler)

    $reader = [System.Xml.XmlReader]::Create($filePath, $settings)
    try {
        while ($reader.Read()) {}
        if ($validationErrors.Count -eq 0) {
            Write-Host "Valid: $filePath"
        }
        else {
            Write-Host "Invalid: $filePath"
            foreach ($err in $validationErrors) {
                Write-Host "  - $err"
            }
            $hasFailures = $true
        }
    }
    catch {
        Write-Host "Invalid: $filePath"
        if ($validationErrors.Count -eq 0) {
            Write-Host "  - $($_.Exception.Message)"
        }
        else {
            foreach ($err in $validationErrors) {
                Write-Host "  - $err"
            }
        }
        $hasFailures = $true
    }
    finally {
        $reader.Close()
    }
}

if ($hasFailures) {
    exit 1
}

exit 0