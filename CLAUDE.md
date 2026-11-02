# CLAUDE.md — Protocolo Anochecer

Antes de modificar el juego, lee Docs/AI/PROJECT_GUIDE.md y Docs/AI/CLAUDE_HANDOFF_PROMPT.md. Inspecciona la rama, el diff, Unity y los assets existentes antes de editar.

- Codex lleva ahora el desarrollo del juego y sus assets; Claude es relevo para tareas acotadas de programación e integración cuando el creador lo indique.
- No crees un tercer controlador o escena FPS sin comparar los dos prototipos existentes.
- Respeta los niveles CANON / PROVISIONAL / EXPERIMENTAL. No inventes lore ni cierres misterios.
- Mantén compatibilidad con Unity 6.6.0f1, URP 17.6.0 e Input System 1.20.0.
- No reescribas Assets/Scenes/SampleScene.unity, cambies Player Settings, agregues paquetes o borres assets sin necesidad justificada.
- Deja arte, texturas, shaders y modelos existentes intactos salvo que la tarea incluya explícitamente esos archivos.
- Trabaja desde una rama propia o la rama que el creador asigne. No trabajes directamente en main; usa PR.
- No incluyas los cambios locales no relacionados en ProjectSettings/ ni Assets/TutorialInfo/Icons/URP.png.
- No afirmes que compiló o funcionó si no se verificó en Unity. Entrega archivos, decisiones provisionales, resultados de verificación y pasos para probar.

La próxima tarea y el estado detallado están en Docs/AI/CLAUDE_HANDOFF_PROMPT.md.