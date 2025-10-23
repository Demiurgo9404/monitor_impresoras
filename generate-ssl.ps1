# 🔐 Generar certificados SSL auto-firmados para desarrollo
# Este script crea certificados SSL para usar con Nginx

Write-Host "🔐 Generando certificados SSL auto-firmados para QOPIQ..." -ForegroundColor Green

# Crear directorio SSL si no existe
if (!(Test-Path "ssl")) {
    New-Item -ItemType Directory -Path "ssl"
}

# Generar clave privada
Write-Host "📝 Generando clave privada..." -ForegroundColor Yellow
openssl genrsa -out ssl/key.pem 2048

# Generar certificado auto-firmado
Write-Host "📜 Generando certificado auto-firmado..." -ForegroundColor Yellow
openssl req -new -x509 -key ssl/key.pem -out ssl/cert.pem -days 365 -subj "/C=US/ST=State/L=City/O=QOPIQ/OU=IT/CN=localhost"

# Verificar archivos generados
if ((Test-Path "ssl/key.pem") -and (Test-Path "ssl/cert.pem")) {
    Write-Host "✅ Certificados SSL generados exitosamente:" -ForegroundColor Green
    Write-Host "   - ssl/key.pem (clave privada)" -ForegroundColor Cyan
    Write-Host "   - ssl/cert.pem (certificado)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "⚠️  NOTA: Estos son certificados auto-firmados para desarrollo." -ForegroundColor Yellow
    Write-Host "   Para producción, use certificados de Let's Encrypt o una CA válida." -ForegroundColor Yellow
} else {
    Write-Host "❌ Error generando certificados SSL" -ForegroundColor Red
    Write-Host "   Asegúrese de tener OpenSSL instalado y en el PATH" -ForegroundColor Red
}

Write-Host ""
Write-Host "🚀 Para usar HTTPS en desarrollo:" -ForegroundColor Green
Write-Host "   docker-compose -f docker-compose.prod.yml up -d" -ForegroundColor Cyan
