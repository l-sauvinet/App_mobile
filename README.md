# Classyn — Application mobile de gestion des absences

Application mobile cross-platform développée en **.NET MAUI** permettant aux professeurs de gérer les présences et les absences de leurs élèves en temps réel, et de créer des cours directement depuis leur smartphone ou tablette.

L'application communique avec un serveur **API REST Node.js / Express** connecté à une base de données **MySQL** hébergée sur AlwaysData.

---

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) avec le workload MAUI :
  ```
  dotnet workload install maui
  ```
- [Node.js 18+](https://nodejs.org/)
- Pour Android : [Android Studio](https://developer.android.com/studio) (émulateur) ou un appareil physique Android
- Pour iOS : Xcode avec un simulateur iOS (macOS uniquement) ou un appareil physique iOS
- Un fichier `server/.env` configuré (voir ci-dessous)

---

## Installation

### 1. Démarrer le serveur

```
cd server
npm install
npm start
```

Le serveur sera disponible sur `http://localhost:3001`.

### 2. Lancer l'application

**Windows**
```
dotnet run -f net10.0-windows10.0.19041.0
```

**Android (émulateur)**
```
dotnet build -f net10.0-android -t:Run
```

> Sur émulateur Android, le serveur est accessible via `http://10.0.2.2:3001` (géré automatiquement dans `ApiService.cs`).

**Via Visual Studio 2022** : ouvrir `App_mobile.sln`, choisir la plateforme cible, puis lancer avec F5.
