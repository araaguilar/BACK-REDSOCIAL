# Deuda conocida

## Validación de nombres ofensivos

Pendiente implementar una capa de moderación para `NombrePerfil` y `NombreUsuario`.

La validación actual cubre formato, longitud y unicidad del `@usuario`, pero todavía no filtra lenguaje ofensivo, palabras reservadas, intentos de suplantación o variantes con caracteres similares.

Propuesta futura:

- Mantener una lista de palabras bloqueadas normalizada.
- Normalizar acentos, espacios, símbolos y sustituciones comunes.
- Agregar revisión manual para casos ambiguos.
- Registrar intentos rechazados para auditoría.
