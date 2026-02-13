# PowerShell script to execute the SQL script
$env:PGPASSWORD = "LarLaw6emmDmezaV"

# Try to find psql in common locations
$psqlPaths = @(
    "C:\Program Files\PostgreSQL\16\bin\psql.exe",
    "C:\Program Files\PostgreSQL\15\bin\psql.exe",
    "C:\Program Files\PostgreSQL\14\bin\psql.exe",
    "C:\Program Files (x86)\PostgreSQL\16\bin\psql.exe",
    "C:\Program Files (x86)\PostgreSQL\15\bin\psql.exe"
)

$psqlExe = $null
foreach ($path in $psqlPaths) {
    if (Test-Path $path) {
        $psqlExe = $path
        break
    }
}

if ($null -eq $psqlExe) {
    Write-Host "psql.exe not found in common locations. Trying system PATH..."
    $psqlExe = "psql"
}

Write-Host "Using psql: $psqlExe"

& $psqlExe -h localhost -p 5432 -U postgres -d oui_system -f assign-admin-role.sql

Write-Host "`nDone! Check output above for results."
