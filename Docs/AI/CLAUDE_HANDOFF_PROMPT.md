# Relevo para Claude — Protocolo Anochecer

Trabaja en https://github.com/IsMiguel/Protocolo-Anochecer, rama codex/fps-foundation y PR borrador #1. Lee CLAUDE.md y Docs/AI/PROJECT_GUIDE.md completos antes de tocar archivos. Si la rama no está sincronizada, inspecciona primero git status, git log y el diff; conserva cualquier cambio local y no pidas que peguen tokens.

## Dirección del trabajo

Codex lleva actualmente el desarrollo completo del juego. Claude queda como relevo para resolver tareas de C#, integración y verificación que el creador le asigne. No cambies canon, diálogos definitivos, modelos, materiales ni texturas sin una tarea explícita. El creador decide historia y arte.

## Estado del proyecto

- Unity 6.6.0f1, URP 17.6.0, Input System 1.20.0. Proyecto PC y solo Input System nuevo.
- SampleScene sigue siendo la única escena en Build Settings; debe conservarse.
- Assets/ProtocoloAnochecer/Scenes/OpeningPrototype.unity: presentación militar/VHS, charla de dos personajes en el helicóptero, vuelo a Europa, aterrizaje y entrega del control al jugador.
- Assets/ProtocoloAnochecer/Scenes/BlockoutPrototype.unity: terreno, edificio y obstáculos para recorrer con el jugador.
- Menús para generarlas: Protocolo Anochecer/Crear introducción y primer mapa y Protocolo Anochecer/Crear prototipo FPS.
- Control de la apertura: Scripts/Player/FirstPersonController.cs (WASD, mouse, gravedad, Shift para correr, Esc/clic para cursor).
- Secuencia: Scripts/Narrative/OpeningSequenceController.cs.
- Generadores: Scripts/Editor/OpeningSequenceBuilder.cs y BlockoutSceneBuilder.cs.
- También existe un prototipo FPS aislado en Scripts/Prototype/FPS/ que utiliza InputSystem_Actions.inputactions. No agregues otro controlador. Primero compara qué escena usa cada sistema y propone una unificación pequeña solo si hace falta.
- Los generadores guardan meshes de terreno como assets, confirman antes de reemplazar escenas y conservan cada mesh generado con nombre único. Ambos terrenos usan UVs con un albedo y normal map procedural tileables de campo europeo; las estructuras siguen en blockout blanco. La apertura aplana el área de aterrizaje, usa escalones bajos y tiene luz direccional.
- ArtSource/Blender/ contiene prototipos .blend y vistas previas de un Bell UH-1H, un campus chaqueño ficticio y armas de los años 40. No son modelos finales, ni están importados como assets de Unity; texturizado e integración aún pendientes.
- El UH-1H es posterior a la Segunda Guerra Mundial, mientras que la apertura lo sitúa en Europa en los años 40. Mantén el helicóptero de la escena como blockout; no uses el UH-1H en esa escena sin una decisión del creador.
- Validación parcial el 2026-11-02 en Unity 6.6.0f1: Console mostró 0 mensajes, 0 advertencias y 0 errores tras importar; `OpeningPrototype` llegó a Play Mode hasta mostrar el objetivo «Encontrá al resto de la unidad». Esto no valida aún los controles, la regeneración de escenas, `BlockoutPrototype`, colisiones, materiales ni un build. Completá esas comprobaciones y reporta solo los resultados observados.

## Primera tarea de relevo: validar e integrar lo existente

1. Abre el proyecto Unity con la versión indicada y espera a que termine la importación. No cambies paquetes ni settings globales.
2. Revisa Console y el diff; confirma que no hay referencias rotas.
3. Ejecuta Protocolo Anochecer/Crear introducción y primer mapa. Si ya existe, la herramienta pide confirmación antes de reemplazarla. Revisa que Unity genere el material European_Field.mat, los meshes y sus .meta; ajusta la escala de tiling solo después de ver el terreno en Play Mode.
4. Ejecuta Protocolo Anochecer/Crear prototipo FPS y verifica también que su terreno quede persistente y no produzca otra copia numerada de la escena.
5. En OpeningPrototype, comprueba el orden de briefing, conversación, vuelo, aterrizaje y control. En Play, verifica WASD, mirada con mouse, Shift, Esc, cursor bloqueado al tomar control y colisiones en cada escalón.
6. En BlockoutPrototype, recorre rampa, escalones y obstáculos con el CharacterController.
7. Si encuentras un error, reproduce y corrige la causa más pequeña. No reescribas la arquitectura. Mantén los cambios técnicos marcados como experimentales.
8. Actualiza PROJECT_GUIDE.md y el PR con resultados reales: archivos, qué probaste, errores encontrados y qué quedó pendiente.

## Lo que queda después de validar

- Revisar en Unity el albedo/normal del terreno y corregir tiling o intensidad si se notan repetidos. Después crear texturas/materiales para concreto y modelos de equipo; mantener paletas sobrias y conservar las estructuras blancas como referencia de blockout.
- Mejorar modelos de Blender e importarlos a Unity solo cuando se revise su escala, orientación, malla, pivotes y colisiones. Las vistas previas no prueban que el modelo sea final.
- Acordar la cronología y transporte de la apertura antes de incorporar modelos históricos.
- Continuar interacción y objetivos cuando el creador lo pida. No implementar armas completas, enemigos, rebobinado ni nuevas escenas por suposición.

No incluyas en el PR los cambios locales ajenos al juego en ProjectSettings/ ni Assets/TutorialInfo/Icons/URP.png. No fusiones el PR; deja que lo revise el creador.