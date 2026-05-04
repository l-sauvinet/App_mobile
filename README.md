# Classyn — Application mobile de gestion des absences

> Projet BTS SIO option SLAM — Application mobile cross-platform destinée aux professeurs pour la gestion des présences et des cours.

---

## Sommaire

1. [Présentation du projet](#présentation-du-projet)
2. [Contexte et objectifs](#contexte-et-objectifs)
3. [Fonctionnalités](#fonctionnalités)
4. [Architecture technique](#architecture-technique)
5. [Stack technique](#stack-technique)
6. [Structure du projet](#structure-du-projet)
7. [Base de données](#base-de-données)
8. [API REST](#api-rest)
9. [Installation et lancement](#installation-et-lancement)
10. [Compétences BTS SIO SLAM](#compétences-bts-sio-slam)

---

## Présentation du projet

**Classyn** est une application mobile professionnelle développée en **.NET MAUI** permettant aux professeurs de gérer en temps réel les absences et retards de leurs élèves, et de créer des cours directement depuis leur smartphone ou tablette.

L'application communique avec un **serveur API REST** développé en **Node.js / Express** qui interroge une base de données **MySQL** hébergée sur AlwaysData.

---

## Contexte et objectifs

Dans un établissement scolaire, le suivi des absences est souvent effectué sur papier ou via des outils lourds peu adaptés à une utilisation mobile. Ce projet répond au besoin suivant :

- Permettre à un professeur de **prendre les présences directement sur son téléphone** pendant le cours
- Lui donner la possibilité de **créer un cours exceptionnel** (cours de rattrapage, sortie scolaire, etc.) en associant une salle et une classe
- Centraliser les données dans une **base commune** accessible depuis n'importe quel appareil

---

## Fonctionnalités

### Authentification
- Connexion sécurisée par identifiant et mot de passe (hashé en SHA-256 en base)
- Session persistante via header `X-Teacher-Id`

### Calendrier des cours
- Vue hebdomadaire avec navigation semaine par semaine
- Affichage des cours et des cours créés pour la journée sélectionnée
- Résumé du jour (nombre de cours, absences)

### Gestion des présences
- Liste des élèves d'un cours avec leur état (présent / absent / en retard)
- Marquage absent ou en retard en un tap
- Saisie de la durée de retard en minutes
- Modification ou suppression d'une absence existante
- **Restriction** : la saisie n'est possible qu'en dehors des plages horaires du cours

### Création de cours
- Sélection d'une salle parmi les salles actives
- Association à une classe (toutes les classes disponibles)
- Choix de la date, de l'heure de début et de fin
- Vérification en temps réel de la disponibilité de la salle
- Motif libre (cours de rattrapage, etc.)
- Suivi des absences sur ce cours exactement comme un cours classique

### Gestion des cours créés
- Liste des cours à venir avec salle, date, horaires et motif
- Suppression d'un cours (et de ses absences associées)

---

## Architecture technique

L'application suit le pattern **MVVM** (Model – View – ViewModel) avec une séparation stricte des responsabilités en 4 couches :

```
┌─────────────────────────────────────────────────┐
│                    VIEWS (XAML)                  │
│   LoginPage · CoursesPage · AttendancePage       │
│   RoomPage · ReservationAttendancePage           │
└───────────────────┬─────────────────────────────┘
                    │ Binding
┌───────────────────▼─────────────────────────────┐
│                 VIEWMODELS (C#)                  │
│  LoginVM · CoursesVM · AttendanceVM              │
│  RoomVM · ReservationAttendanceVM                │
└───────────────────┬─────────────────────────────┘
                    │ Injection de dépendances
┌───────────────────▼─────────────────────────────┐
│                  SERVICES (C#)                   │
│  ApiService · AuthService · CourseService        │
│  AbsenceService · RoomService                    │
│  ReservationAbsenceService                       │
└───────────────────┬─────────────────────────────┘
                    │ HTTP (JSON)
┌───────────────────▼─────────────────────────────┐
│             SERVEUR API (Node.js)                │
│              Express · mysql2                    │
└───────────────────┬─────────────────────────────┘
                    │ SQL
┌───────────────────▼─────────────────────────────┐
│           BASE DE DONNÉES (MySQL)                │
│         Hébergée sur AlwaysData                  │
└─────────────────────────────────────────────────┘
```

### Injection de dépendances

Tous les services et ViewModels sont enregistrés dans le conteneur DI de .NET via `MauiProgram.cs` :

- **Singleton** : services partagés entre toutes les pages (ApiService, AuthService, RoomService…)
- **Transient** : ViewModels recréés à chaque navigation (AttendanceViewModel, ReservationAttendanceViewModel)

### Navigation

```
LoginPage
└── Shell (TabBar)
    ├── CoursesPage
    │   └── [Modal] AttendancePage
    └── RoomPage
        └── [Modal] ReservationAttendancePage
```

---

## Stack technique

| Couche | Technologie | Version |
|--------|-------------|---------|
| Application mobile | .NET MAUI | 10.0 |
| Langage client | C# + XAML | .NET 10 |
| Pattern UI | MVVM — CommunityToolkit.Mvvm | 8.4.2 |
| Plateformes cibles | Android (API 21+), iOS 15+, macOS 15+, Windows 10+ | — |
| Serveur API | Node.js + Express | Express 4 |
| Langage serveur | JavaScript (CommonJS) | Node 18+ |
| Base de données | MySQL | 8.x |
| Hébergement DB | AlwaysData | — |
| Authentification | SHA-256 + session X-Teacher-Id | — |
| Culture / Langue | fr-FR (heure 24h, dates françaises) | — |

### Dépendances .NET

| Package | Rôle |
|---------|------|
| CommunityToolkit.Mvvm 8.4.2 | Générateurs de code MVVM (ObservableProperty, RelayCommand) |
| BCrypt.Net-Next 4.1.0 | (référence, hachage géré côté serveur) |
| MySqlConnector 2.5.0 | Driver MySQL asynchrone |

### Dépendances Node.js

| Package | Rôle |
|---------|------|
| express | Framework web HTTP |
| mysql2 | Driver MySQL pour Node.js |
| bcrypt | Comparaison de mots de passe hashés |
| express-session | Gestion des sessions |
| cors | Autorisation cross-origin |
| dotenv | Variables d'environnement |

---

## Structure du projet

```
App_mobile/
├── Models/
│   ├── User.cs                  # Professeur connecté
│   ├── Course.cs                # Cours avec matière, classe, salle
│   ├── Student.cs               # Élève avec état d'absence (MVVM)
│   ├── ReservationStudent.cs    # Élève avec durée de retard (MVVM)
│   ├── Room.cs                  # Salle (nom, capacité, type)
│   ├── RoomReservation.cs       # Cours créé manuellement
│   ├── SchoolClass.cs           # Classe scolaire
│   └── CalendarDay.cs           # Jour du calendrier (MVVM)
│
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── CoursesViewModel.cs
│   ├── AttendanceViewModel.cs
│   ├── RoomViewModel.cs
│   └── ReservationAttendanceViewModel.cs
│
├── Views/
│   ├── LoginPage.xaml
│   ├── CoursesPage.xaml
│   ├── AttendancePage.xaml
│   ├── RoomPage.xaml
│   └── ReservationAttendancePage.xaml
│
├── Services/
│   ├── ApiService.cs            # HttpClient centralisé
│   ├── AuthService.cs           # Login / logout
│   ├── CourseService.cs         # Récupération des cours
│   ├── AbsenceService.cs        # Absences sur cours classiques
│   ├── RoomService.cs           # Salles et cours créés
│   └── ReservationAbsenceService.cs  # Absences sur cours créés
│
├── Converters/
│   └── AppConverters.cs         # IsNotEmpty, InverseBool, couleurs absences
│
├── MauiProgram.cs               # Configuration DI + culture fr-FR
├── AppShell.xaml                # Navigation tabs + routes
├── App.xaml.cs                  # Initialisation culture UI
│
└── server/
    ├── index.js                 # 16 endpoints API REST
    ├── package.json
    └── .env                     # Secrets (non versionné)
```

---

## Base de données

Base hébergée sur **AlwaysData** (`absencemanagement_bdd`).

### Tables principales

| Table | Rôle |
|-------|------|
| `user` | Comptes utilisateurs (login, password SHA-256, role_id) |
| `teacher` | Liaison user → professeur |
| `class` | Classes scolaires |
| `subject` | Matières (dont "Réservation de salle") |
| `student` | Élèves, rattachés à une classe |
| `room` | Salles (nom, capacité, type, is_active) |
| `course` | Cours (matière, classe, professeur, salle, horaires, notes) |
| `absence` | Absences/retards (student_id, course_id, is_late, delay_minutes, recorded_by) |

### Points notables

- Un **cours créé manuellement** depuis l'app est inséré dans la table `course` avec le sujet `"Réservation de salle"` — pas de table intermédiaire
- Les absences des cours créés et des cours classiques partagent **la même table `absence`**, reliée par `course_id`
- Les migrations (ajout de colonnes, sujets par défaut) s'exécutent **automatiquement au démarrage du serveur**

---

## API REST

Base URL : `http://localhost:3001`

Les routes protégées nécessitent le header `X-Teacher-Id: <teacher_id>`.

### Authentification

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/api/teacher/login` | Connexion (login + password → SHA-256) |
| POST | `/api/teacher/logout` | Déconnexion |

### Cours

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/teacher/courses` | Cours du professeur (50 derniers) |
| GET | `/api/teacher/courses/:id/students` | Élèves du cours avec état d'absence |
| GET | `/api/teacher/classes` | Classes du professeur |
| GET | `/api/teacher/all-classes` | Toutes les classes de l'établissement |

### Absences

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/api/teacher/absences` | Enregistrer absence/retard (vérif. plage horaire) |
| DELETE | `/api/teacher/absences/:id` | Supprimer une absence |

### Cours créés (anciennement réservations)

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/teacher/rooms` | Salles actives |
| GET | `/api/teacher/rooms/availability` | Vérifier disponibilité d'un créneau |
| GET | `/api/teacher/reservations` | Cours créés à venir |
| POST | `/api/teacher/reservations` | Créer un cours (vérif. conflit salle) |
| DELETE | `/api/teacher/reservations/:id` | Supprimer un cours + ses absences |
| GET | `/api/teacher/reservations/:id/students` | Élèves du cours créé |
| POST | `/api/teacher/reservation-absences` | Absence sur cours créé |
| DELETE | `/api/teacher/reservation-absences/:id` | Supprimer cette absence |

---

## Installation et lancement

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) avec workload MAUI :
  ```
  dotnet workload install maui
  ```
- [Node.js 18+](https://nodejs.org/)
- Accès à la base MySQL (fichier `.env`)

### 1. Démarrer le serveur

```
cd server
npm install
npm start
```

Serveur disponible sur `http://localhost:3001`.

Fichier `server/.env` à créer :
```env
DB_HOST=mysql-absencemanagement.alwaysdata.net
DB_PORT=3306
DB_NAME=absencemanagement_bdd
DB_USER=absencemanagement
DB_PASSWORD=...
SESSION_SECRET=...
PORT=3001
```

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

**Via Visual Studio 2022** : ouvrir `App_mobile.sln`, choisir la plateforme, lancer avec F5.

---

## Compétences BTS SIO SLAM

| Bloc | Compétence | Mise en oeuvre |
|------|-----------|----------------|
| B1 — Support | Gestion d'un patrimoine applicatif | Maintenance évolutive : ajout de fonctionnalités, refactoring de la couche base de données |
| B2 — Développement | Conception et développement d'une application | Application .NET MAUI complète en architecture MVVM, 5 pages, 6 services, 8 modèles |
| B2 — Développement | Développement de composants d'accès aux données | API REST Node.js / Express avec 16 endpoints, accès MySQL via mysql2 |
| B2 — Développement | Travail en mode collaboratif | Gestion de version Git, branches, commits conventionnels |
| B2 — Développement | Conception d'une base de données | Modélisation relationnelle MySQL, migrations automatiques, gestion des FK |
| B3 — Cybersécurité | Sécurisation des données | Mots de passe hashés SHA-256, header d'authentification, validation serveur des plages horaires |

### Points techniques valorisables à l'oral

- **Pattern MVVM** avec générateurs de code source (CommunityToolkit.Mvvm, `[ObservableProperty]`, `[RelayCommand]`)
- **Injection de dépendances** via le conteneur .NET (Singleton vs Transient selon le cycle de vie)
- **Architecture multi-couches** : séparation nette Models / ViewModels / Views / Services
- **Application cross-platform** : un seul code C# déployé sur Android, iOS, macOS et Windows
- **API REST** avec middleware d'authentification, gestion des erreurs et migrations automatiques
- **Gestion de la culture** `fr-FR` appliquée globalement (format 24h, noms de jours/mois en français)
- **Validation métier côté serveur** : vérification de la disponibilité des salles, restriction de saisie d'absences à la plage horaire du cours

---

*Projet développé dans le cadre du BTS SIO option SLAM.*
