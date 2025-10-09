# Simple HTTP Server for Frontend
$port = 3000
$path = Get-Location

Write-Host "Iniciando servidor HTTP en puerto $port..."
Write-Host "Directorio: $path"
Write-Host "URL: http://localhost:$port/simple-frontend.html"
Write-Host "Presiona Ctrl+C para detener el servidor"

# Create HTTP listener
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$port/")
$listener.Start()

Write-Host "Servidor iniciado exitosamente!"

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response
        
        $requestedFile = $request.Url.LocalPath.TrimStart('/')
        if ($requestedFile -eq "" -or $requestedFile -eq "/") {
            $requestedFile = "simple-frontend.html"
        }
        
        $filePath = Join-Path $path $requestedFile
        
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - $($request.HttpMethod) $($request.Url.LocalPath)"
        
        if (Test-Path $filePath) {
            $content = Get-Content $filePath -Raw -Encoding UTF8
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($content)
            
            # Set content type based on file extension
            $extension = [System.IO.Path]::GetExtension($filePath).ToLower()
            switch ($extension) {
                ".html" { $response.ContentType = "text/html; charset=utf-8" }
                ".css" { $response.ContentType = "text/css" }
                ".js" { $response.ContentType = "application/javascript" }
                ".json" { $response.ContentType = "application/json" }
                default { $response.ContentType = "text/plain" }
            }
            
            $response.StatusCode = 200
            $response.ContentLength64 = $bytes.Length
            $response.OutputStream.Write($bytes, 0, $bytes.Length)
        } else {
            $response.StatusCode = 404
            $errorBytes = [System.Text.Encoding]::UTF8.GetBytes("404 - File Not Found")
            $response.ContentLength64 = $errorBytes.Length
            $response.OutputStream.Write($errorBytes, 0, $errorBytes.Length)
        }
        
        $response.OutputStream.Close()
    }
} finally {
    $listener.Stop()
}

