# Prompt de inicio para Claude — Protocolo Anochecer

Trabajá sobre el repositorio privado https://github.com/IsMiguel/Protocolo-Anochecer. Si tu sesión no tiene acceso, pedí que conecten tu cuenta a GitHub; no solicites ni aceptes que peguen tokens o contraseñas en el chat.

Antes de tocar archivos, leé `CLAUDE.md` y `Docs/AI/PROJECT_GUIDE.md`. El documento de guía resume el canon, las ideas provisionales, la dirección de arte, el estado real del repo y las tareas recomendadas.

## Reparto de trabajo

- Tu responsabilidad: scripts C#, gameplay y documentación técnica.
- Codex se ocupa de modelos 3D, materiales, texturas y su integración visual.
- El creador conserva la decisión final sobre canon, historia y arte.
- Evitá editar los mismos archivos que Codex y usá ramas/PRs para integrar.

## Tu primera tarea

Implementar el **Hito 1: jugador FPS mínimo, funcional y fácil de probar**, respetando Unity 6.6.0f1, URP 17.6.0 y el Input System 1.20.0 que ya aparecen en el proyecto.

1. Revisá la rama y el estado actual del repo; inspeccioná paquetes, escena de muestra, Input Actions, Player Settings y referencias relevantes.
2. Conservá `Assets/Scenes/SampleScene.unity`; creá una escena separada para el prototipo y agrégala a Build Settings solo si es necesario para abrir/probarla sin desplazar otras escenas.
3. Antes de implementar, informá en pocas líneas el plan y los archivos previstos. Luego seguí con el cambio mínimo sin esperar otra aprobación.
4. Añadí únicamente lo necesario para caminar, mirar con mouse, correr si es sencillo y probar colisiones en un bloqueout básico. Interacción puede quedar para el siguiente hito.
5. No inventes el argumento, no escribas diálogos/canon, no implementes el arma completa, enemigos, rebobinado ni la cinemática en esta tarea.
6. No cambies settings globales ni agregues paquetes sin una necesidad comprobada. Usá el Input System que ya está instalado y evitá mezclarlo accidentalmente con el Input Manager legacy.
7. Dejá pasos concretos para abrir la escena, entrar en Play Mode y probar los controles. Si no pudiste ejecutar Unity, indicá claramente qué quedó sin verificar.
8. Entregá el trabajo en la rama `claude/fps-foundation` y prepará un PR contra `main`, sin fusionarlo. Resumí archivos cambiados, controles, riesgos y verificación.

La prioridad es una base jugable pequeña, legible y mantenible que respete el proyecto. Leé la guía antes de tomar cualquier decisión narrativa o de arquitectura.
