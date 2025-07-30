import { Component, ElementRef, ViewChild, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';

interface Ball {
  x: number;
  y: number;
  dx: number;
  dy: number;
  radius: number;
}

interface Paddle {
  x: number;
  y: number;
  width: number;
  height: number;
}

interface Brick {
  x: number;
  y: number;
  width: number;
  height: number;
  color: string;
  destroyed: boolean;
}

interface PowerUp {
  x: number;
  y: number;
  width: number;
  height: number;
  type: 'multiball' | 'bigpaddle' | 'slowball';
  dy: number;
}

@Component({
  selector: 'app-breakout-game',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  templateUrl: './breakout-game.component.html',
  styleUrl: './breakout-game.component.scss'
})
export class BreakoutGameComponent implements OnInit, OnDestroy {
  @ViewChild('gameCanvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('canvasContainer', { static: true }) containerRef!: ElementRef<HTMLDivElement>;

  private ctx!: CanvasRenderingContext2D;
  private animationId: number = 0;
  
  // Game state
  gameRunning = false;
  gameStarted = false;
  gameOver = false;
  score = 0;
  lives = 3;
  maxLives = 3;

  // Canvas dimensions
  canvasWidth = 560;
  canvasHeight = 400;

  // Game objects
  ball!: Ball;
  paddle!: Paddle;
  bricks: Brick[] = [];
  powerUps: PowerUp[] = [];

  // Game settings
  private readonly brickRows = 5;
  private readonly brickCols = 10;
  private readonly brickColors = ['#ef4444', '#f97316', '#eab308', '#22c55e', '#3b82f6'];
  private readonly ballSpeed = 4;
  private readonly paddleSpeed = 8;
  private readonly powerUpChance = 0.15; // 15% chance for power-up

  // Visual effects
  private particles: Array<{x: number, y: number, vx: number, vy: number, life: number, color: string}> = [];
  scoreAnimation = false;

  // Input handling
  private mouseX = 0;
  private touchX = 0;
  private isTouch = false;

  ngOnInit() {
    this.setupCanvas();
    this.initializeGame();
    this.updateCanvasSize();
  }

  ngOnDestroy() {
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
    }
  }

  @HostListener('window:resize')
  onResize() {
    this.updateCanvasSize();
  }

  private setupCanvas() {
    this.ctx = this.canvasRef.nativeElement.getContext('2d')!;
    this.ctx.imageSmoothingEnabled = true;
  }

  private updateCanvasSize() {
    const container = this.containerRef.nativeElement;
    const containerWidth = container.offsetWidth - 4; // Account for border
    const aspectRatio = this.canvasHeight / this.canvasWidth;
    
    if (containerWidth < this.canvasWidth) {
      this.canvasWidth = containerWidth;
      this.canvasHeight = containerWidth * aspectRatio;
    } else {
      this.canvasWidth = 560;
      this.canvasHeight = 400;
    }
  }

  private initializeGame() {
    // Initialize ball
    this.ball = {
      x: this.canvasWidth / 2,
      y: this.canvasHeight - 60,
      dx: this.ballSpeed * (Math.random() > 0.5 ? 1 : -1),
      dy: -this.ballSpeed,
      radius: 8
    };

    // Initialize paddle
    this.paddle = {
      x: this.canvasWidth / 2 - 50,
      y: this.canvasHeight - 20,
      width: 100,
      height: 10
    };

    // Clear effects
    this.particles = [];
    this.powerUps = [];

    // Initialize bricks
    this.createBricks();
  }

  private createBricks() {
    this.bricks = [];
    const brickWidth = (this.canvasWidth - 20) / this.brickCols;
    const brickHeight = 20;
    
    for (let row = 0; row < this.brickRows; row++) {
      for (let col = 0; col < this.brickCols; col++) {
        this.bricks.push({
          x: col * brickWidth + 10,
          y: row * brickHeight + 30,
          width: brickWidth - 2,
          height: brickHeight - 2,
          color: this.brickColors[row],
          destroyed: false
        });
      }
    }
  }

  startGame() {
    this.gameRunning = true;
    this.gameStarted = true;
    this.gameOver = false;
    this.score = 0;
    this.lives = this.maxLives;
    this.initializeGame();
    this.gameLoop();
  }

  private gameLoop() {
    if (!this.gameRunning) return;

    this.update();
    this.draw();
    
    this.animationId = requestAnimationFrame(() => this.gameLoop());
  }

  private update() {
    // Update ball position
    this.ball.x += this.ball.dx;
    this.ball.y += this.ball.dy;

    // Update particles
    this.updateParticles();

    // Ball collision with walls
    if (this.ball.x + this.ball.radius > this.canvasWidth || this.ball.x - this.ball.radius < 0) {
      this.ball.dx = -this.ball.dx;
    }
    
    if (this.ball.y - this.ball.radius < 0) {
      this.ball.dy = -this.ball.dy;
    }

    // Ball collision with bottom (lose life)
    if (this.ball.y + this.ball.radius > this.canvasHeight) {
      this.lives--;
      if (this.lives <= 0) {
        this.endGame();
        return;
      } else {
        this.resetBall();
      }
    }

    // Update paddle position based on input
    const targetX = (this.isTouch ? this.touchX : this.mouseX) - this.paddle.width / 2;
    this.paddle.x = Math.max(0, Math.min(this.canvasWidth - this.paddle.width, targetX));

    // Ball collision with paddle
    if (this.ball.y + this.ball.radius > this.paddle.y &&
        this.ball.x > this.paddle.x &&
        this.ball.x < this.paddle.x + this.paddle.width &&
        this.ball.dy > 0) {
      
      // Calculate hit position for angle variation
      const hitPos = (this.ball.x - this.paddle.x) / this.paddle.width;
      const angle = (hitPos - 0.5) * Math.PI / 3; // Max 60 degrees
      
      this.ball.dx = this.ballSpeed * Math.sin(angle);
      this.ball.dy = -Math.abs(this.ballSpeed * Math.cos(angle));
    }

    // Ball collision with bricks
    for (const brick of this.bricks) {
      if (!brick.destroyed && this.checkBallBrickCollision(brick)) {
        brick.destroyed = true;
        this.ball.dy = -this.ball.dy;
        const points = 10 * (this.brickRows - Math.floor((brick.y - 30) / 20) + 1);
        this.score += points;
        this.createParticles(brick.x + brick.width / 2, brick.y + brick.height / 2, brick.color);
        this.animateScore();
        
        // Chance to spawn power-up
        if (Math.random() < this.powerUpChance) {
          this.createPowerUp(brick.x + brick.width / 2, brick.y + brick.height / 2);
        }
        break;
      }
    }

    // Update power-ups
    this.updatePowerUps();

    // Check win condition
    if (this.bricks.every(brick => brick.destroyed)) {
      this.endGame();
    }
  }

  private checkBallBrickCollision(brick: Brick): boolean {
    return this.ball.x + this.ball.radius > brick.x &&
           this.ball.x - this.ball.radius < brick.x + brick.width &&
           this.ball.y + this.ball.radius > brick.y &&
           this.ball.y - this.ball.radius < brick.y + brick.height;
  }

  private resetBall() {
    this.ball.x = this.canvasWidth / 2;
    this.ball.y = this.canvasHeight - 60;
    this.ball.dx = this.ballSpeed * (Math.random() > 0.5 ? 1 : -1);
    this.ball.dy = -this.ballSpeed;
  }

  private endGame() {
    this.gameRunning = false;
    this.gameOver = true;
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
    }
  }

  private draw() {
    // Clear canvas
    this.ctx.fillStyle = '#000';
    this.ctx.fillRect(0, 0, this.canvasWidth, this.canvasHeight);

    // Draw ball with glow effect
    this.ctx.save();
    this.ctx.shadowColor = '#ffffff';
    this.ctx.shadowBlur = 10;
    this.ctx.fillStyle = '#ffffff';
    this.ctx.beginPath();
    this.ctx.arc(this.ball.x, this.ball.y, this.ball.radius, 0, Math.PI * 2);
    this.ctx.fill();
    this.ctx.restore();

    // Draw paddle with gradient
    const paddleGradient = this.ctx.createLinearGradient(0, this.paddle.y, 0, this.paddle.y + this.paddle.height);
    paddleGradient.addColorStop(0, '#60a5fa');
    paddleGradient.addColorStop(1, '#3b82f6');
    this.ctx.fillStyle = paddleGradient;
    this.ctx.fillRect(this.paddle.x, this.paddle.y, this.paddle.width, this.paddle.height);

    // Draw bricks
    for (const brick of this.bricks) {
      if (!brick.destroyed) {
        this.ctx.fillStyle = brick.color;
        this.ctx.fillRect(brick.x, brick.y, brick.width, brick.height);
        
        // Add highlight
        this.ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
        this.ctx.fillRect(brick.x, brick.y, brick.width, 3);
      }
    }

    // Draw particles
    this.drawParticles();

    // Draw power-ups
    this.drawPowerUps();
  }

  private animateScore() {
    this.scoreAnimation = true;
    setTimeout(() => this.scoreAnimation = false, 300);
  }

  private createParticles(x: number, y: number, color: string) {
    for (let i = 0; i < 8; i++) {
      this.particles.push({
        x: x,
        y: y,
        vx: (Math.random() - 0.5) * 8,
        vy: (Math.random() - 0.5) * 8,
        life: 30,
        color: color
      });
    }
  }

  private updateParticles() {
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const particle = this.particles[i];
      particle.x += particle.vx;
      particle.y += particle.vy;
      particle.life--;
      
      if (particle.life <= 0) {
        this.particles.splice(i, 1);
      }
    }
  }

  private drawParticles() {
    for (const particle of this.particles) {
      this.ctx.save();
      this.ctx.globalAlpha = particle.life / 30;
      this.ctx.fillStyle = particle.color;
      this.ctx.fillRect(particle.x - 2, particle.y - 2, 4, 4);
      this.ctx.restore();
    }
  }

  private createPowerUp(x: number, y: number) {
    const powerUpTypes: PowerUp['type'][] = ['multiball', 'bigpaddle', 'slowball'];
    const type = powerUpTypes[Math.floor(Math.random() * powerUpTypes.length)];
    
    this.powerUps.push({
      x: x - 15,
      y: y,
      width: 30,
      height: 15,
      type: type,
      dy: 2
    });
  }

  private updatePowerUps() {
    for (let i = this.powerUps.length - 1; i >= 0; i--) {
      const powerUp = this.powerUps[i];
      powerUp.y += powerUp.dy;
      
      // Check collision with paddle
      if (powerUp.y + powerUp.height > this.paddle.y &&
          powerUp.x + powerUp.width > this.paddle.x &&
          powerUp.x < this.paddle.x + this.paddle.width) {
        this.activatePowerUp(powerUp.type);
        this.powerUps.splice(i, 1);
      }
      // Remove if off screen
      else if (powerUp.y > this.canvasHeight) {
        this.powerUps.splice(i, 1);
      }
    }
  }

  private activatePowerUp(type: PowerUp['type']) {
    // For simplicity, we'll just add score bonus for now
    // In a full implementation, these would have visual effects
    switch (type) {
      case 'multiball':
        this.score += 50;
        break;
      case 'bigpaddle':
        this.score += 30;
        break;
      case 'slowball':
        this.score += 20;
        break;
    }
    this.animateScore();
  }

  private drawPowerUps() {
    for (const powerUp of this.powerUps) {
      this.ctx.save();
      
      // Set color based on type
      switch (powerUp.type) {
        case 'multiball':
          this.ctx.fillStyle = '#10b981';
          break;
        case 'bigpaddle':
          this.ctx.fillStyle = '#3b82f6';
          break;
        case 'slowball':
          this.ctx.fillStyle = '#f59e0b';
          break;
      }
      
      this.ctx.fillRect(powerUp.x, powerUp.y, powerUp.width, powerUp.height);
      
      // Add glow effect
      this.ctx.shadowColor = this.ctx.fillStyle as string;
      this.ctx.shadowBlur = 8;
      this.ctx.fillRect(powerUp.x + 2, powerUp.y + 2, powerUp.width - 4, powerUp.height - 4);
      
      this.ctx.restore();
    }
  }

  // Event handlers
  onMouseMove(event: MouseEvent) {
    this.isTouch = false;
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    this.mouseX = (event.clientX - rect.left) * (this.canvasWidth / rect.width);
  }

  onTouchMove(event: TouchEvent) {
    event.preventDefault();
    this.isTouch = true;
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    this.touchX = (event.touches[0].clientX - rect.left) * (this.canvasWidth / rect.width);
  }

  onTouchStart(event: TouchEvent) {
    event.preventDefault();
    this.onTouchMove(event);
  }

  onCanvasClick() {
    if (!this.gameRunning && !this.gameStarted) {
      this.startGame();
    }
  }

  // Helper methods for template
  getLivesArray() {
    return Array(this.lives).fill(0);
  }

  getEmptyLivesArray() {
    return Array(this.maxLives - this.lives).fill(0);
  }
}
