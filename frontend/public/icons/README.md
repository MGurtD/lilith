# PWA Icons para Temges

## Iconos requeridos

Para que la PWA funcione correctamente, necesitas estos iconos:

- `icon-192x192.png` - Icono estándar Android
- `icon-512x512.png` - Icono de alta resolución
- `apple-touch-icon.png` - Icono para iOS (180x180px)
- `favicon.ico` - Favicon clásico (opcional, ya existe en root)

## Cómo generar los iconos

### Opción 1: Usar herramienta online (Recomendado)

1. Ve a https://realfavicongenerator.net/
2. Sube el logo de Temges (`../logo.png`)
3. Configura:
   - Android Chrome: Sí, con fondo blanco o transparente
   - iOS Safari: Sí, con margen de seguridad
   - Nombre: "Temges"
4. Descarga el paquete y copia los archivos aquí

### Opción 2: Usar ImageMagick (CLI)

Si tienes ImageMagick instalado:

```bash
# Desde el directorio public/
magick logo.png -resize 192x192 icons/icon-192x192.png
magick logo.png -resize 512x512 icons/icon-512x512.png
magick logo.png -resize 180x180 icons/apple-touch-icon.png
```

### Opción 3: Usar Photoshop/GIMP

1. Abre `../logo.png`
2. Redimensiona a 192x192px (mantener ratio, añadir padding si es necesario)
3. Exporta como PNG
4. Repite para 512x512 y 180x180

## Notas importantes

- **Fondo**: El logo actual tiene fondo blanco. Para PWA es recomendable:
  - Fondo blanco para iconos estándar
  - Versión con padding para que no se corte en esquinas redondeadas
- **Maskable icons**: Android puede recortar los iconos. Considera crear una versión "maskable" con área segura en el centro (80% del canvas).

## Temporal

Mientras generas los iconos finales, el plugin PWA usará el logo.png existente como fallback.
