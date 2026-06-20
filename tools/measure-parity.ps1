param(
    [string]$MarkdownFile = "docs/design.md"
)

if (-not (Test-Path $MarkdownFile)) {
    Write-Error "File not found: $MarkdownFile"
    exit 1
}

$lines = Get-Content $MarkdownFile -ErrorAction Stop
$headerLine = $lines | Select-String -Pattern '^\|\s*Area\s*\|\s*Win2D API\s*\|\s*Uno2D API/Type\s*\|\s*Status\s*\|\s*Priority\s*\|' | Select-Object -First 1
if (-not $headerLine) {
    Write-Error "Could not find the parity matrix header in $MarkdownFile"
    exit 1
}

$startIndex = $headerLine.LineNumber
$rows = @()
for ($i = $startIndex; $i -lt $lines.Count; $i++) {
    $line = $lines[$i].Trim()
    if (-not $line) { break }
    if ($line -match '^\|\s*-') { continue }
    if ($line -notmatch '^\|') { break }

    $cells = $line -split '\|' | ForEach-Object { $_.Trim() }
    if ($cells.Count -lt 6) { continue }

    $rows += [PSCustomObject]@{
        Area = $cells[1]
        Win2DApi = $cells[2]
        Uno2DApi = $cells[3]
        Status = $cells[4]
        Priority = $cells[5]
    }
}

if ($rows.Count -eq 0) {
    Write-Error "No matrix rows found in $MarkdownFile"
    exit 1
}

$summary = [ordered]@{}
$statuses = @('Supported','Partial','Not Planned')
foreach ($status in $statuses) {
    $summary[$status] = ($rows | Where-Object { $_.Status -ieq $status }).Count
}

$total = $rows.Count
$supported = $summary['Supported']
$partial = $summary['Partial']
$notPlanned = $summary['Not Planned']

function FormatPercent($value, $total) {
    if ($total -eq 0) { return '0.0%' }
    return "{0:N1}%" -f (($value / $total) * 100)
}

$completed = $supported + $partial

Write-Host "Parity matrix summary for: $MarkdownFile" -ForegroundColor Cyan
Write-Host "Rows tracked: $total"
Write-Host "Supported: $supported ($(FormatPercent $supported $total))"
Write-Host "Partial:   $partial ($(FormatPercent $partial $total))"
Write-Host "Not Planned: $notPlanned ($(FormatPercent $notPlanned $total))"
Write-Host "Supported or Partial: $completed ($(FormatPercent $completed $total))"
Write-Host ''

$priorityGroups = $rows | Group-Object -Property Priority | Sort-Object Name
Write-Host "Priority breakdown:" -ForegroundColor Cyan
foreach ($group in $priorityGroups) {
    $name = $group.Name
    $count = $group.Count
    $supportedCount = ($group.Group | Where-Object { $_.Status -ieq 'Supported' }).Count
    $partialCount = ($group.Group | Where-Object { $_.Status -ieq 'Partial' }).Count
    $notPlannedCount = ($group.Group | Where-Object { $_.Status -ieq 'Not Planned' }).Count
    Write-Host ("  {0}: {1} total, Supported={2}, Partial={3}, NotPlanned={4}" -f $name, $count, $supportedCount, $partialCount, $notPlannedCount)
}

Write-Host ''
Write-Host "Detailed row statuses:" -ForegroundColor Cyan
$rows | ForEach-Object {
    Write-Host ("- [{0}] {1} -> {2} ({3})" -f $_.Status, $_.Area, $_.Win2DApi, $_.Priority)
}
