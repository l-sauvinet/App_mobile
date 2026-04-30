# Classyn (AppMobile) — Gestion des absences et réservations

Application mobile cross-platform pour les professeurs, permettant de gérer les présences en cours et de réserver des salles de classe.

---

## Fonctionnalités

- **Authentification** — Connexion sécurisée par identifiant/mot de passe
- **Calendrier des cours** — Vue hebdomadaire des cours avec navigation semaine par semaine
- **Gestion des présences** — Marquer les élèves absents ou en retard pour chaque cours
- **Salles de classe** — Consulter la liste des salles disponibles
- **Réservations** — Réserver une salle pour un créneau horaire, consulter et annuler ses réservations

---

## Stack technique

| Couche | Technologie |
|--------|-------------|
| Application mobile | .NET MAUI 10 (C# / XAML) |
| Pattern UI | MVVM — CommunityToolkit.Mvvm 8.4.2 |
| Plateformes cibles | Android (API 21+), iOS 15+, macOS Catalyst 15+, Windows 10+ |
| Serveur API | Node.js + Express 4 |
| Base de données | MySQL (hébergé sur AlwaysData) |

---

## Prérequis

### Application mobile

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Workload MAUI installé :
  ```bash
  dotnet workload install maui
  ```
- Pour Android : Android SDK (via Visual Studio ou en standalone)
- Pour iOS/macOS : macOS avec Xcode installé
- Recommandé : [Visual Studio 2022](https://visualstudio.microsoft.com/) (v17.12+) avec la charge de travail **.NET Multi-platform App UI**

### Serveur

- [Node.js 18+](https://nodejs.org/)
- Accès à la base de données MySQL (credentials dans `server/.env`)

---

## Structure du projet

```
App_mobile/
├── Models/               # Modèles de données (Course, Student, Room, ...)
├── ViewModels/           # ViewModels MVVM (Login, Courses, Attendance, Room)
├── Views/                # Pages XAML (LoginPage, CoursesPage, ...)
├── Services/             # Services métier et appels API
│   ├── ApiService.cs     # Client HTTP vers le serveur
│   ├── AuthService.cs    # Authentification et session
│   ├── CourseService.cs  # Récupération des cours
│   ├── AbsenceService.cs # Gestion des absences
│   └── RoomService.cs    # Salles et réservations
├── Converters/           # Convertisseurs de valeurs XAML
├── Platforms/            # Code spécifique par plateforme
├── Resources/            # Icônes, polices, styles
├── MauiProgram.cs        # Configuration DI (injection de dépendances)
├── AppShell.xaml         # Navigation (onglets + routes)
└── server/               # Serveur Node.js Express
    ├── index.js          # Point d'entrée — routes API
    ├── package.json
    └── .env              # Variables d'environnement (DB, session)
```

---

## Installation et lancement

### 1. Serveur API

```bash
cd server
npm install
npm start
```

Le serveur démarre sur `http://localhost:3001`.

Pour le mode développement avec rechargement automatique :
```bash
npm run dev
```

Le fichier `server/.env` doit contenir :
```env
DB_HOST=...
DB_PORT=3306
DB_NAME=...
DB_USER=...
DB_PASSWORD=...
SESSION_SECRET=...
PORT=3001
```

---

### 2. Application mobile

#### Windows

```bash
dotnet run -f net10.0-windows10.0.19041.0
```

#### Android (émulateur ou appareil)

```bash
dotnet build -f net10.0-android -t:Run
```

> Sur l'émulateur Android, l'application communique avec le serveur via `http://10.0.2.2:3001` (l'adresse du PC hôte vue depuis l'émulateur). Cette redirection est gérée automatiquement dans `Services/ApiService.cs`.

#### iOS (macOS uniquement)

```bash
dotnet build -f net10.0-ios -t:Run
```

#### Via Visual Studio 2022

Ouvrir `App_mobile.sln`, sélectionner la plateforme cible dans la barre d'outils, puis lancer avec **F5**.

---

## API — Endpoints

Base URL : `http://localhost:3001`

Les routes protégées nécessitent le header `X-Teacher-Id: <id>`.

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/api/teacher/login` | Connexion professeur |
| POST | `/api/teacher/logout` | Déconnexion |
| GET | `/api/teacher/courses` | Liste des cours du professeur |
| GET | `/api/teacher/courses/:id/students` | Élèves d'un cours avec état d'absence |
| POST | `/api/teacher/absences` | Enregistrer une absence / un retard |
| DELETE | `/api/teacher/absences/:id` | Supprimer une absence |
| GET | `/api/teacher/rooms` | Liste des salles disponibles |
| GET | `/api/teacher/reservations` | Réservations actuelles et futures |
| POST | `/api/teacher/reservations` | Créer une réservation |
| DELETE | `/api/teacher/reservations/:id` | Annuler une réservation |

---

## Schéma de navigation

```
LoginPage
└── TabBar
    ├── CoursesPage (Mes Cours)
    │   └── AttendancePage (sélection d'un cours)
    └── RoomPage (Réservations)
```
