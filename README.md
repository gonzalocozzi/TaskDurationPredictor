# Task Duration Predictor

## Descripción

**Task Duration Predictor** es una aplicación de consola desarrollada en C# utilizando .NET que permite predecir la duración de tareas repetitivas basándose en datos históricos. La aplicación carga el historial de tareas desde un archivo JSON, calcula la duración promedio de las tareas y ofrece una estimación de tiempo a medida que se simula la ejecución de la tarea.

### Funcionalidades

- **Cálculo de duración promedio**: Calcula la duración promedio de tareas previas y la utiliza para estimar el tiempo restante durante la ejecución.
- **Carga y almacenamiento de historial de tareas**: Guarda el historial de duración de tareas en un archivo JSON, permitiendo que la información persista entre ejecuciones.
- **Simulación de tareas**: Simula la ejecución de tareas, proporcionando un progreso visual y estimaciones de tiempo.

## Requisitos

- .NET SDK (versión 6.0 o superior)

## Instalación

1. Clona este repositorio:

   ```bash
   git clone https://github.com/gonzalocozzi/TaskDurationPredictor.git
   cd TaskDurationPredictor
   ```

2. Abre la terminal o línea de comandos y navega hasta la carpeta del proyecto.

## Ejecución

### En Windows

1. Abre la terminal (cmd o PowerShell).
2. Navega hasta la carpeta del proyecto.
3. Ejecuta el siguiente comando:

   ```bash
   dotnet run
   ```

### En macOS

1. Abre la terminal.
2. Navega hasta la carpeta del proyecto.
3. Ejecuta el siguiente comando:

   ```bash
   dotnet run
   ```

### En GNU/Linux

1. Abre la terminal.
2. Navega hasta la carpeta del proyecto.
3. Ejecuta el siguiente comando:

   ```bash
   dotnet run
   ```

## Uso

- Al ejecutar la aplicación, se te pedirá que ingreses el nombre de la tarea que deseas simular. Si ya existen datos históricos para esa tarea, se mostrará una estimación de la duración.
- Si no hay datos históricos, la aplicación simulará la tarea sin estimación de duración.

## Pruebas

Para ejecutar las pruebas unitarias, asegúrate de tener el .NET SDK instalado y sigue estos pasos:

1. Navega hasta la carpeta del proyecto de pruebas.

   ```bash
   cd TaskDurationPredictor.Tests
   ```

2. Ejecuta el siguiente comando:

   ```bash
   dotnet test
   ```

## Contribuciones

Las contribuciones son bienvenidas. Si deseas contribuir, por favor crea un fork del repositorio, realiza tus cambios y envía un pull request.

## Licencia

Este proyecto está bajo la Licencia MIT. Para más detalles, consulta el archivo [LICENSE](https://github.com/gonzalocozzi/TaskDurationPredictor/blob/main/LICENSE).
