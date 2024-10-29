# ADR 001: Elección de la Estrategia de Predicción de Duración de Tareas

## Estado

**Propuesta**

## Contexto

El objetivo de la aplicación es permitir a los usuarios predecir la duración de tareas repetitivas basándose en datos históricos. La solución debe ser efectiva, eficiente y fácil de usar. Para lograr esto, es crucial seleccionar una estrategia de predicción que maximice la precisión de las estimaciones iniciales sin recalcular durante el progreso de la tarea.

## Decisión

Se ha decidido utilizar un enfoque de predicción basado en datos históricos, en el que la aplicación:

- Almacena un historial de ejecuciones de tareas y sus respectivas duraciones.
- Calcula la duración estimada de una nueva ejecución de tarea utilizando el promedio de las duraciones de ejecuciones anteriores de la misma tarea, así como una proyección de la duración de la ejecución actual en base al grado de avance de la misma.
- Proporciona actualizaciones en tiempo real sobre el progreso de la tarea y el tiempo restante estimado.

## Consecuencias

### Ventajas:

- **Precisión en las Predicciones**: Al utilizar datos históricos, se espera que las estimaciones sean más precisas, lo que genera confianza en los usuarios.
- **Interacción Dinámica**: La capacidad de proporcionar actualizaciones en tiempo real sobre el progreso mejora la experiencia del usuario.
- **Simplicidad**: La estrategia es fácil de implementar y no requiere modelos de predicción complejos.

### Desventajas:

- **Dependencia de Datos Históricos**: Si no se dispone de un historial de tareas, la aplicación no podrá proporcionar una estimación precisa.
- **Variabilidad en las Tareas**: La duración de tareas similares puede variar, lo que puede impactar la precisión de las estimaciones.

## Alternativas Consideradas

- **Uso de Modelos de Aprendizaje Automático**: Aunque podrían ofrecer predicciones más avanzadas, esta opción fue desechada debido a la complejidad de implementación y la necesidad de un mayor conjunto de datos para entrenar los modelos.
- **Estimaciones basadas en factores externos**, como la carga de uso del sistema: Esta opción fue considerada, pero se optó por la estrategia de datos históricos para garantizar una implementación más simple y fácil de mantener dada la baja criticidad de las predicciones.
