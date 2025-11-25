# **MemeIt Game \- Complete Requirements & Workflow Documentation**

This document serves as the single source of truth for the requirements and workflow of the MemeIt online party game, covering Authentication, Real-Time Communication, Lobby Management, the Creative Phase, and the Scoring Phase. It details the synchronous, real-time nature of the game and the technical contracts between client and server services.

## **1\. Core Entities & Data Structure**

| Entity | Fields (Required) | Fields (Optional) | Description |
| :---- | :---- | :---- | :---- |
| **Game** | gameCode (8-char unique string), status (LOBBY/IN\_PROGRESS/ENDED), adminPlayerId, maxPlayers, currentRound (1-5) | passwordHash | The main container for the game instance, tracking global state and progression across the 5 rounds. maxPlayers should be configurable, ideally between 4 and 10 players, to ensure balanced rating pools. |
| **Player** | playerId (unique user ID from JWT), name (user-defined name), gameCode, isReady (Boolean), gameTotalScore | isAdmin | Represents a participant. The gameTotalScore persists across rounds and determines the final winner. The playerId is the immutable link to the player's identity throughout the session. |
| **Meme Template** | templateId, imageUrl (base image), textAreas (Array of objects) |  | A template defining the base image and relative coordinates for text placement, ensuring responsive and consistent text overlay across various screen sizes. |
| **Submitted Meme** | memeId, gameCode, round, creatorId, memeData (user text/style) |  | An entry containing the final user text, linked to the round and creator for scoring. This is the artifact generated during the Creative Phase. |
| **Rating** | ratingId, memeId, raterId, score (0-5) |  | A discrete score (0 to 5\) given by one player to another's creation, used for calculating round points. Zero (0) is explicitly allowed for negative feedback. |

## **1.5. Users Service and Authentication Requirements**

The Users Service acts as the Identity Provider, handling player identification and session management via JSON Web Tokens (JWT). This establishes a critical security boundary, ensuring the Game Service trusts only verified player identities (playerId).

### **A. Authentication Flow**

1. **Identity Request:** When a player attempts to create or join a game, the client initiates the process by sending the desired **Player Name** to the Users Service. This name is primarily for cosmetic display in the game lobby.  
2. **JWT Generation:** The Users Service executes the core authentication logic:  
   * **Player ID Generation:** Generates a unique, cryptographically secure playerId for the session. This acts as the canonical identifier used in all game logic.  
   * **Token Creation:** Creates a time-limited JWT. For security, the token should be relatively short-lived (e.g., 60 minutes) to minimize the window for token hijacking. The payload **MUST** include the playerId and should include an issued at (iat) and expiration (exp) timestamp.  
   * **Token Delivery:** Returns the signed JWT to the client.  
3. **Session Storage:** The client **MUST** save the received JWT token in the browser's session storage (sessionStorage). This ensures the player remains authenticated and avoids re-authentication on minor navigation or page refresh. Upon token expiration, the client must be redirected back to the initial entry screen with an error message indicating session invalidation.

### **B. Request Authorization**

1. **Header Requirement:** All subsequent HTTP requests from the client to the Game Service (for game state changes, submission, etc.) **MUST** include the JWT in the Authorization header as a Bearer token (Authorization: Bearer \<JWT\_TOKEN\>).  
2. **Service Validation:** The Game Service is responsible for strict token validation upon every request:  
   * **Security Check:** Validating the JWT's signature (using a shared secret or public key) and verifying the exp timestamp to prevent unauthorized or expired requests.  
   * **Identity Extraction:** Extracting the validated **playerId** from the token payload. This ID is the only trusted source of identity and ownership for all lobby and game logic operations.  
   * **Error Handling:** Requests failing validation (e.g., expired, bad signature, missing token) should be rejected with a 401 Unauthorized status, triggering the client-side session cleanup mentioned above.

## **2\. Lobby Workflow (Creation & Joining)**

The lobby acts as the crucial pre-game coordination space, managed with persistent, real-time data from Firestore.

### **Game Creation Workflow**

1. The client successfully obtains and validates a JWT, extracting the **playerId**.  
2. **Game Data Persistence:** The game is created in the Game Service (Firestore). The service generates a unique, human-readable 8-character gameCode (e.g., using alphanumeric characters). If a password was provided, a strong hashing algorithm (e.g., bcrypt) is used for secure storage before the game document is created.  
3. **Player Initialization:** The creator is marked as isAdmin: true, and their initial state is isReady: false.  
4. The Admin player is immediately redirected to the **Game Lobby View**, where the unique, shareable gameCode is prominently displayed alongside a copy-to-clipboard function.

### **Game Joining Workflow**

1. The client successfully obtains and validates a JWT, extracting the **playerId**.  
2. The client sends the **Game Code** and (if applicable) the **Password** to the Game Service.  
3. **Validation & Security Gates:** The Game Service performs layered checks:  
   * **Code Check:** Does the gameCode exist?  
   * **Password Check:** If a passwordHash exists, does the provided password match the hash?  
   * **State Check:** Is the game status strictly LOBBY? (Prevents joining games already IN\_PROGRESS or ENDED).  
   * **Capacity Check:** Is the game full (currentPlayers \< maxPlayers)?  
4. **Error Feedback:** Failure in validation results in descriptive errors (e.g., "Invalid Game Code," "Incorrect Password," "Game is Full"). These messages must be displayed via custom UI components for a seamless user experience.  
5. If valid, a new Player document is created using the JWT-provided playerId, and the player is redirected to the **Game Lobby View**.

### **Game Start Conditions (Admin Action)**

The "Start Game" button is a critical control for the admin, enabled only when the session is viable for play.

1. **Minimum Players:** The game has a minimum of 3 players joined (the admin plus two others).  
2. **Readiness Consensus:** **All** players currently in the lobby must have their isReady status set to true. The UI must provide clear, live visual feedback (e.g., toggling icons or status text) to the admin about who is pending.  
3. **Action:** Upon clicking the enabled button, the admin triggers a server event that updates the game's status to IN\_PROGRESS in Firestore, and immediately triggers a **SignalR broadcast** to all connected players, forcing a simultaneous redirection to the game's first round.

## **3\. Real-time Communication (SignalR)**

SignalR is the backbone for delivering synchronous game updates and coordinating phase transitions with low latency.

### **SignalR Requirements:**

1. **Connection Lifecycle:** The client establishes a connection to the dedicated hub (/gameHub) upon entering the lobby. The client must implement robust logic to handle connection states:  
   * **Connecting:** Display a loading indicator.  
   * **Connected:** The client joins a specific group identified by the gameCode.  
   * **Disconnected/Reconnecting:** Automatic reconnection logic must be implemented. If reconnection fails after several attempts, the user must be prompted to refresh or warned that the game session may be compromised.  
2. **Server-to-Client Messages:** The server broadcasts critical, synchronous updates with defined data payloads:  
   * **Game State Changes:** Messages like PlayerJoined, PlayerLeft, or ReadyStatusUpdated containing the specific playerId and new status.  
   * **Phase Transitions:** Explicit commands that carry critical initialization data: e.g., CreativePhaseStart (payload: { round: 2, templateId: 'ABC123' }), ScoringPhaseStart (payload: { round: 2, memeQueue: \['memeId1', 'memeId2', ...\] }).  
   * **Timer Updates:** Frequent broadcasts (e.g., every 500ms or 1s) of the TimeRemaining for the active phase.  
   * **Game Start Command:** A single GameStarted message used to force the initial transition from the lobby.

## **4\. Game Structure and Progression**

The game is structured for competitive balance and replayability, consisting of **5 rounds**. This design ensures a satisfying match length and sufficient opportunity for scoring variance.

Each round has two strictly sequential, timed phases:

1. **Creative Phase** (30 seconds): Content generation under high-pressure time constraint.  
2. **Scoring Phase** (Up to 30 seconds per submitted meme): Peer review, dictating the progression rate based on player interaction speed.

## **5\. Creative Phase (30 Seconds)**

This phase demands quick input and seamless visual rendering, ensuring the player can accurately preview their submission before the timer expires.

### **A. Memes Service Interaction & Template Structure**

1. **Template Selection:** The Game Service selects a random Meme Template ID for the current round and broadcasts this ID via SignalR. This selection process ensures variety and fairness across all players.  
2. **Template Structure:** The textAreas array provides the necessary coordinates and styling. The implementation must guarantee that the text overlay is rendered responsively regardless of the client's screen size.  
   interface TextArea {  
     location: { x: number, y: number }; // Relative position on image (0-100) for responsive layout  
     dimensions: { width: number, height: number }; // Relative size (0-100)  
     fontSize: string; // e.g., '48px' or '10vw'. Should use a highly visible font like Impact or similar (e.g., 'Impact').  
     fontBorderWidth: string; // e.g., '2px' (for classic meme outline)  
     textColor: string; // Hex code (typically white)  
     textBorderColor: string; // Hex code (typically black)  
     isBold: boolean; // Text weight  
   }

### **B. Client-Side Rendering & Submission**

1. **Image Display:** The base imageUrl is fetched and displayed. The client must utilize either an HTML Canvas or a complex, layered DOM structure (e.g., absolute positioning using Tailwind CSS) to display the meme image with the text overlay.  
2. **Real-time Overlay:** User input in the corresponding text fields is dynamically rendered in an overlay layer. This must apply the precise style parameters (font family, border thickness, color, and position) from the template to give the player an accurate, high-fidelity preview of their final submission.  
3. **Submission Lock & Validation:** The "Submit Meme" button is disabled initially. It becomes enabled only after a two-step validation: (1) The client confirms **ALL** text areas contain at least one non-whitespace character, and (2) The client enforces a character limit (e.g., 80 characters per field) to maintain meme readability.  
4. **Submission:** The client sends the Meme Template ID and the final, sanitized text content to the Game Service via a secure, authenticated API endpoint. The server should perform a final check for offensive or prohibited content.  
5. **Wait State:** After submission, the player's UI transitions to a "Waiting for the Timer to Expire" state. The input fields are locked, and they observe the countdown timer until the phase ends.  
6. **Phase End:** When the 30-second timer elapses, the submission window closes. Any player who failed to submit before the timer runs out has their contribution for that round discarded. The server then sends the ScoringPhaseStart SignalR message to all players.

## **6\. Scoring Phase (Max 30 Seconds per Meme)**

The scoring phase is a synchronized, timed event focused on impartial rating and rapid score tabulation.

### **A. Meme Presentation & Rating**

1. **Meme Queue:** The server presents all validly submitted memes for the current round sequentially. The order is determined server-side (e.g., shuffled randomly) and broadcast to ensure consistency.  
2. **Presentation:** The complete, rendered meme is displayed. **Crucially, the creator's identity must be masked/hidden** during the rating process to enforce unbiased peer scoring.  
3. **Self-Exclusion:** The meme creator's UI automatically disables the rating mechanism for their own submitted meme.  
4. **Rating UI:** Players use intuitive controls (0 to 5, e.g., five star icons, where 0 is explicitly an option) to submit their rating. The score is submitted via an authenticated endpoint.  
5. **Rating Submission:** Once submitted, the player's UI transitions to "Waiting for next meme..." and they cannot change their score for that specific meme.

### **B. Progression and Scoreboard**

1. **Meme Progression Logic:** The server progresses to the next meme immediately when either: (a) All eligible players have submitted their rating (consensus achieved), or (b) the 30-second timer for that specific meme expires (time-out condition). This mechanism guarantees the game moves forward efficiently, even if a player is inactive.  
2. **Round Scoreboard Calculation:** After the last meme is rated, the server performs the scoring:  
   * **Meme Score:** Each meme's score is calculated as the average of all received ratings (excluding the creator's non-rating).  
   * **Player Points:** The creator of the meme receives points equal to their meme's final average score, rounded to the nearest tenth.  
   * **Total Update:** The round points are added to the player's persistent gameTotalScore.  
3. **Display:** A "Round Scoreboard" view is presented. It must be dynamically ordered by gameTotalScore (highest first), showing: (1) The player's name, (2) the points earned in the current round, and (3) their overall total score. A highlight should be placed on the "Top Meme of the Round" (highest average score).  
4. **Next Round:** The server initiates a brief (e.g., 5-second) pause to allow players to review the scores before sending the CreativePhaseStart message to begin the subsequent round.

## **7\. Game End**

1. **Condition:** The game officially ends after the completion of the 5th scoring phase and the subsequent calculation of the final scoreboard.  
2. **Final Scoreboard:** The final "End Scoreboard" is displayed. This celebratory screen shows the permanent ranking of all players based on their final gameTotalScore. The "Meme Lord" champion (player with the highest score) must be clearly celebrated with unique visual effects. The application should offer post-game options, such as viewing all submitted memes, initiating a new game, or sharing the final result link. The application should also provide a method to save this final result to a historical game record database.