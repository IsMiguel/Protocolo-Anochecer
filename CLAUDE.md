# CLAUDE.md — Protocolo Anochecer

Antes de modificar código, escenas, prefabs o assets:

1. Lee `Docs/AI/PROJECT_GUIDE.md` completo.
2. Inspecciona el repositorio y confirma la rama, Unity version, packages, escenas y archivos existentes.
3. Respeta los niveles CANON / PROVISIONAL / EXPERIMENTAL descritos en la guía.
4. No inventes lore ni cierres misterios abiertos. Si falta una decisión narrativa, usa un placeholder marcado o pregunta.
5. Mantén los cambios pequeños, reversibles y compatibles con Unity 6.6.0f1 + URP 17.6.0.
6. No reescribas `SampleScene` ni toques arte, texturas, shaders o modelos asignados a Codex salvo coordinación explícita.
7. No añadas paquetes, no cambies settings globales, no borres assets ni trabajes directamente en `main` sin autorización específica.
8. Al corregir errores, reproduce y localiza primero; aplica la solución mínima y verifica.
9. No afirmes que una escena compila o funciona si no lo verificaste en Unity.
10. Entrega un resumen de archivos, cambios, decisiones provisionales, verificación y pasos para abrir/probar.

## División de tareas acordada

- Claude: C# y gameplay, sistemas de input/interacción, herramientas y documentación técnica.
- Codex: modelos 3D, materiales, texturas, props y su integración visual en URP.
- Creador: decisiones de canon y aprobación artística/narrativa.

Usa una rama propia y PR para integrar. Evita editar el mismo archivo que Codex. No compartas tokens o credenciales en conversaciones.

## Primera tarea autorizada

Seguir “Siguiente tarea recomendada para Claude” en `Docs/AI/PROJECT_GUIDE.md`: inspeccionar el proyecto y luego proponer/implementar el Hito 1, un jugador FPS mínimo en escena separada, preservando la escena de plantilla. Mantener cualquier decisión temporal identificada como experimental.
