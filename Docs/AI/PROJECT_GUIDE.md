# Protocolo Anochecer — guía maestra del proyecto

**Estudio:** PCGG Studios  
**Género:** FPS de horror, acción, misterio e historia alternativa con horror analógico.  
**Motor:** Unity 6.6.0f1, C#.  
**Plataforma inicial:** PC.  
**Repositorio:** https://github.com/IsMiguel/Protocolo-Anochecer  
**Estado de este documento:** contexto consolidado a partir del documento maestro del creador y del contenido de `main` revisado el 2 de noviembre de 2026.

> Este documento orienta el desarrollo, pero no reemplaza las decisiones explícitas del creador. Si hay un conflicto, no conviertas una idea provisional o una implementación técnica en canon.

## 1. Identidad y visión

Protocolo Anochecer busca combinar una operación militar aparentemente convencional con un misterio que se revela poco a poco. El jugador debe descubrir el fenómeno junto con el protagonista. La historia empieza contenida y verosímil; aumenta la inquietud mediante señales, documentos, transmisiones y situaciones que no encajan.

La identidad buscada es **FPS de mediados de los 2000 (aprox. 2006–2007) + horror moderno + Segunda Guerra Mundial + Argentina + anomalías + decisiones y consecuencias**. Las referencias de escala y ritmo son Call of Duty 3 y Call of Duty 4, sin copiar personajes, mapas, armas o escenas. No se busca imitar gráficos AAA contemporáneos ni convertirlo en “Call of Duty con monstruos”.

Argentina debe tener peso real en el misterio: fuerzas, autoridades, lenguaje, documentos, radio, lugares y contexto histórico. Los sucesos alternativos son ficción; nunca presentarlos como hechos históricos reales.

## 2. Canon y niveles de certeza

Usá estas etiquetas en documentos, escenas, assets y conversaciones:

- **CANON:** decisión explícita del creador o del documento maestro.
- **PROVISIONAL:** idea aceptada para prototipar, pero modificable.
- **EXPERIMENTAL:** prueba técnica que no define la historia.

### Confirmado por el documento maestro

- El protagonista forma parte de fuerzas argentinas y empieza sin conocer la verdad completa.
- La primera historia se relaciona con Europa y la Segunda Guerra Mundial, alrededor de la década de 1940.
- Roberto Marcelino Ortiz aparece dentro de una representación ficticia de los acontecimientos. No atribuirle delitos o hechos ficticios como si fueran históricos.
- Edelmiro Julián Farrell pertenece a una etapa posterior del contexto narrativo; su papel y escenas exactas siguen por definir.
- La saga trata secretos, manipulación, sacrificio, responsabilidad y consecuencias.
- Hay una idea para una secuela cuyo protagonista está relacionado con el protagonista original: su hijo, ligado a fuerzas de seguridad argentinas. La elección entre Ejército y Gendarmería está abierta.
- Una posible continuación o expansión, **Protocolo Atardecer**, se relaciona con el Chaco. El concepto de clímax incluye una decisión extrema para contener una amenaza en la región; detalles, responsabilidad y consecuencias deben desarrollarse con cuidado, sin tratarlos como canon más allá de lo indicado.

### Apertura propuesta por el creador — tratar como guion provisional hasta su aprobación final

La intención compartida en conversación es una presentación militar de apariencia oficial; una transmisión o material de archivo con una anomalía; conversación entre el protagonista y un compañero durante el viaje; llegada y aterrizaje en Europa; y entrega del control al jugador al desembarcar. La primera tarea podría ser encontrar al resto de la unidad, pero el texto y objetivo exactos son provisionales. La transmisión con Ortiz y los efectos de VHS deben construirse con sobriedad.

**Choque de época que debe quedar visible:** el Bell UH‑1H solicitado para modelado es de posguerra, mientras la apertura principal se plantea en los años 40. El asset UH‑1H existe como concepto de arte fuera de la rama de GitHub revisada; no introducirlo en la misión de 1940 sin una decisión explícita de historia alternativa o de cronología. También queda por decidir qué transporte utiliza la apertura. El campus chaqueño es un concepto ficticio inspirado en la región; no es una réplica confirmada de una institución ni se debe insertar en la campaña sin acordarlo.

### No definido (no inventar)

Nombre del protagonista y del compañero; fechas exactas; nombres y orden de misiones; explicación última del Protocolo (científica, sobrenatural, biológica, psicológica o mixta); tipos definitivos de enemigo; duración y reglas completas del rebobinado; final de cada entrega; cronología política detallada; vehículo de la apertura; precio y fecha de lanzamiento; elección de fuerza de seguridad del hijo.

## 3. Premisa y progresión narrativa

La operación empieza como una misión militar. Poco a poco aparecen indicios de información clasificada, experimentación, actividad secreta y anomalías. La progresión deseada es:

1. misión y briefing;
2. exploración y llegada;
3. primera contradicción entre lo prometido y lo que se encuentra;
4. descubrimiento e investigación;
5. evidencia de actividad secreta;
6. anomalía y horror;
7. revelación parcial, decisión y consecuencias.

No abrir con una explicación total ni con combate constante. Las áreas deben servir a exploración, narrativa, tensión, combate, puzzle, descanso o transición; el entorno debe contar parte de la historia.

La secuela conserva las consecuencias de la primera historia. El hijo del protagonista no es “otro soldado”: hereda secretos e información incompleta. El Chaco debe sentirse como un lugar, no como un decorado intercambiable. La idea de una decisión extrema en Chaco es un tema a desarrollar, no una escena que deba escribirse unilateralmente.

## 4. Pilares de gameplay

El juego es un FPS con movimiento, cámara, interacción, armas, munición, enemigos, daño, muerte y checkpoints. La base técnica debe preceder a sistemas de gran escala.

### Loop de misión

Recibir información → explorar → avanzar → descubrir algo → combatir o evitar peligro → investigar → experimentar anomalía → tomar una decisión → avanzar. No todas las misiones deben repetir el loop igual.

### Horror

Priorizar tensión, silencio, sonido ambiental, espacios vacíos, iluminación, radios, documentos, transmisiones y señales incompletas. Jumpscares y enemigos son herramientas puntuales. No llenar cada segundo de combate ni de glitches.

### Rebobinado

Es una mecánica distintiva y parte del misterio, no un botón genérico de reinicio. La idea inicial es que ciertas acciones dañinas o moralmente cuestionables hacia NPC vulnerables puedan provocar un retroceso breve de la realidad. Puede sugerir que el fenómeno responde, observa o intenta corregir algo. Reglas jugables, disparadores, duración, coste y cronología son decisiones abiertas: construir primero una prueba aislada y etiquetarla como experimental. No castigar al jugador con una regla oculta sin feedback.

### Decisiones y NPC

Compatriotas afectados pueden crear dilemas; “enemigo = matar” no es la única respuesta. Evitar pantallas de moralidad “bueno/malo”. Los NPC deben tener propósito, comportamiento creíble y relación clara con la misión.

## 5. Narrativa audiovisual y dirección de arte

- Las grabaciones pueden funcionar como briefing, propaganda, informe, evidencia o material encontrado. La cinta puede degradarse cuando el fenómeno aparece.
- VHS es una herramienta estética/narrativa; no afirmar que existía como tecnología literal en 1940. La forma exacta en que se presenta el material archivado sigue abierta.
- Radio: órdenes, noticias, voces, interferencia y mensajes incompletos.
- Glitches deben tener significado (presencia, fallo, censura, transición o peligro) y aparecer con moderación.
- Apagones deben cambiar algo: iluminación, comunicación, conducta, acceso o información.
- HUD limpio, legible y de sabor 2000s: salud, munición, objetivo e interacción cuando correspondan.
- Estética: geometría moderada, materiales creíbles, texturas cuidadas, iluminación atmosférica, niebla/lluvia/polvo cuando sirvan al plano. El aspecto antiguo es una elección de arte; no justificar texturas borrosas, repetidas o sin escala.
- Mantener consistencia de época en armas, uniformes, arquitectura y accesorios. Cuando una referencia histórica real no esté clara, señalarla como referencia por verificar.

## 6. Estado técnico verificado de GitHub

Revisado en la rama `main` del repositorio privado el 2026-11-02:

- Unity **6.6.0f1**.
- Universal Render Pipeline **17.6.0**.
- Input System **1.20.0**; también existe `Assets/InputSystem_Actions.inputactions`. No asumir que el mapa de acciones ya está conectado a un jugador.
- AI Navigation **2.0.14**, Timeline **6.6.0**, Unity Test Framework **1.8.0**, uGUI **2.6.0** y Visual Scripting **1.9.12** están en el manifest.
- Única escena listada en Build Settings: `Assets/Scenes/SampleScene.unity`.
- Assets vistos: escena de plantilla, settings de URP (incluye assets PC y Mobile) y TutorialInfo. No hay aún código de gameplay propio, escena narrativa ni carpeta de arte de Protocolo Anochecer en la rama revisada.
- No se detectaron CLAUDE.md, README ni documentos de diseño en la raíz durante esa revisión.
- `.gitignore` cubre cachés típicas de Unity y excluye `*.blend1`; `.gitattributes` asigna modelos e imágenes a Git LFS. Esto configura reglas en el repo, pero no confirma que Git LFS esté instalado en cada clon.
- Input legacy también está presente en ProjectSettings; validar el modo activo antes de escribir bindings nuevos.
- No hay evidencia revisada de tests de juego, asmdef de gameplay, CI de build o arquitectura de código propia. Estado: desconocido / todavía sin gameplay.

La rama de contexto `codex/project-context` contiene la documentación de colaboración. Los modelos Blender hechos en la sesión anterior aún no están en GitHub; antes de integrarlos hay que llevarlos al proyecto y conservar los `.meta` de Unity. No asumir que el proyecto local fuera de GitHub y este repositorio remoto están sincronizados.

## 7. Plan recomendado de desarrollo

### Hito 0 — base compartida (documentación actual)

Acordar contexto, separar decisiones firmes de prototipos y establecer colaboración sin editar el mismo asset desde dos chats.

### Hito 1 — jugador FPS controlable

En una escena nueva de prototipo, sin destruir `SampleScene`: cámara y movimiento con Input System ya instalado, colisiones, correr y una interacción simple con feedback. No añadir combate completo todavía. Que sea fácil de probar de forma aislada.

### Hito 2 — pequeño bloque narrativo

Secuencia de briefing/archivo, conversación ambiental sencilla, transición a una zona jugable corta y objetivo de llegada. Mantener diálogo, vehículo y cronología en placeholders provisionales mientras la discrepancia del UH‑1H siga abierta.

### Hito 3 — interacción y misión

Objetivos, objetos inspeccionables/documentos, radio básica, checkpoints y un encuentro diseñado. Medir ritmo: exploración, información, tensión y descanso.

### Hito 4 — sistemas distintivos

Pruebas de anomalía/energía y rebobinado en escenas pequeñas, con eventos y reglas visibles. No integrarlas antes de que movimiento, interacción y escena estén estables.

### Hito 5 — producción de arte

Crear modelos y materiales propios, importar con escala y orientación consistentes, mantener fuente Blender y FBX/texturas de Unity, nombrar assets y conservar GUID/meta. Crear texturas con intención por superficie y época; configurar mapas y materiales URP explícitamente.

**Criterio de alcance:** una secuencia corta, completa y coherente vale más que muchas mecánicas a medio hacer.

## 8. Guía de colaboración Claude + Codex + creador

- Claude: código de juego, scripts, arquitectura ligera, input, interacción, herramientas de editor y documentación técnica.
- Codex (este chat): modelos 3D, UV y texturas, materiales/shaders URP, props, composición visual y preparación/importación de assets.
- Creador: autoridad final sobre canon, arte final, historia, referencias y cambios que afecten la visión.
- Trabajar en ramas distintas o rutas de archivos distintas. No editar simultáneamente la misma escena, prefab, script, material o modelo.
- No enviar tokens, passwords ni archivos con secretos en chats. Usar los permisos/conectores de GitHub de cada entorno.
- Abrir PR o compartir cambios y revisar el diff antes de integrar. No empujar directamente a `main`.
- Para cada entrega: propósito, archivos, decisiones provisionales, pasos para abrir/probar y problemas conocidos.
- Integrar arte: fuente editable fuera de `Assets` si conviene; FBX y mapas que use Unity dentro de `Assets/ProtocoloAnochecer/Art/`. Mantener `.meta` si ya existe y confirmar que el proyecto reconoce los GUID.
- Los cambios grandes de narrativa, controls o arquitectura se anuncian primero. Los cambios pequeños y reversibles pueden hacerse directamente en la rama de trabajo.

## 9. Reglas obligatorias para todo asistente

1. Leer esta guía y las instrucciones locales antes de tocar el proyecto.
2. Inspeccionar código, escenas, prefabs, paquetes y referencias antes de implementar; evitar duplicar sistemas.
3. Clasificar una idea nueva como canon, provisional o experimental. No inventar respuestas a misterios.
4. Mantener el código simple, modular, legible, mantenible y compatible con Unity 6/URP; evitar scripts gigantes, variables mágicas, dependencias y complejidad prematura.
5. No borrar/reorganizar assets, renombrar APIs públicas, instalar paquetes o modificar escenas ajenas sin razón documentada.
6. Para bugs: reproducir, localizar causa, aplicar el menor cambio que lo corrija y verificar.
7. No afirmar que una escena corre, compila o un build sirve sin evidencia del Editor.
8. Explicar qué cambió, por qué, archivos afectados y pasos de verificación.
9. Si falta información, inspeccionar el repo; luego preguntar o implementar con placeholders explícitamente provisionales.
10. Preservar la identidad: Argentina + historia alternativa + horror histórico + misterio + consecuencias.

## 10. Siguiente tarea recomendada para Claude

Abrir el repo `IsMiguel/Protocolo-Anochecer`, leer `CLAUDE.md` y esta guía, revisar el estado actual de Unity y diseñar el **Hito 1: jugador FPS controlable** en una escena nueva. Presentar brevemente el plan y archivos a modificar antes de implementarlo. Usar la rama `claude/fps-foundation` desde `main` si está disponible; preservar `SampleScene` y evitar cambiar settings de proyecto sin necesidad. Implementar y entregar un prototipo mínimo con pasos de verificación. Las decisiones de input de escena deben usar el paquete existente. No integrar modelos, texturas ni narrativa nueva.
