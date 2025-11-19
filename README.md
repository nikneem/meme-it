# Meme-It 🎭

A multiplayer party game where creativity meets comedy! Players compete to create the funniest memes and earn points based on votes from other players.

## 🎮 Game Overview

Meme-It is an interactive party game that brings people together through humor and creativity. One player creates a game lobby, others join in, and when everyone's ready, the competition begins!

### How to Play

1. **Lobby Setup**
   - One user creates a game and enters the lobby
   - Other players join using the game code
   - Players set their status to "ready"
   - Game starts when all players are ready

2. **Game Structure**
   - Each game consists of **5 rounds**
   - Each round has two phases: **Creative Phase** and **Scoring Phase**

3. **Creative Phase**
   - All players receive a random meme template (base image without text)
   - Players add text to their meme template in designated text areas
   - Phase ends when all players finish or time runs out

4. **Scoring Phase**
   - All created memes are presented to all players
   - Players vote on memes created by others (cannot vote for their own)
   - Points are awarded based on voting results
   - Leaderboard is updated and displayed

5. **Game End**
   - After round 5, the final leaderboard is shown
   - Winner is announced with a festive celebration
   - Players can start a new game immediately from the results screen

## 🏗️ Architecture

### Backend
- **Language**: C# (.NET)
- **Orchestration**: Microsoft Aspire
- **Architecture**: Microservices with service-specific storage solutions
- Each service uses storage appropriate to its needs (MongoDB, SQL, etc.)

### Frontend
- **Framework**: Angular
- Modern, responsive UI for seamless gameplay across devices

## 📁 Project Structure

```
meme-it/
├── docs/              # Documentation files
├── src/               # Source code
│   ├── Aspire/        # Microsoft Aspire orchestration
│   ├── Games/         # Game service modules
│   └── ...            # Additional service modules
└── README.md          # This file
```

Each backend module is isolated in its own folder to maintain clear separation of concerns and independent development.

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 or later
- Node.js and npm (for Angular frontend)
- Microsoft Aspire workload
- Docker (for containerized services)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/nikneem/meme-it.git
cd meme-it
```

2. Navigate to the source directory:
```bash
cd src
```

3. Restore .NET dependencies:
```bash
dotnet restore
```

4. Run the application using Aspire:
```bash
cd Aspire/HexMaster.MemeIt.Aspire/HexMaster.MemeIt.Aspire.AppHost
dotnet run
```

## 🛠️ Development

The project follows a modular architecture where each service is self-contained:

- **Games Service**: Handles game creation, lobby management, and game state
- **Aspire AppHost**: Orchestrates all services and manages dependencies
- **Service Defaults**: Shared configuration and utilities

Each service can be developed and tested independently while Aspire handles the orchestration and inter-service communication.

## 📝 License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Contact

For questions or feedback, please open an issue on GitHub.

---

**Have fun and may the funniest meme win!** 🏆
