# Script para limpiar carpetas bin/ y obj/ de todos los proyectos
$basePath = Get-Location
$folders = @("bin", "obj")

# Buscar y eliminar carpetas bin/ y obj/ recursivamente
Get-ChildItem -Path $basePath -Recurse -Directory | 
    Where-Object { $_.Name -in $folders } | 
    ForEach-Object {
        $folderPath = $_.FullName
        try {
            Remove-Item -Path $folderPath -Recurse -Force -ErrorAction Stop
            Write-Host "Eliminada carpeta: $folderPath" -ForegroundColor Green
        } catch {
            Write-Host "Error al eliminar $folderPath : $_" -ForegroundColor Red
        }
    }

Write-Host "\nLimpieza completada." -ForegroundColor Cyan
