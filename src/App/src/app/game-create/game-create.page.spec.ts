import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GameCreatePage } from './game-create.page';

describe('GameCreatePage', () => {
  let component: GameCreatePage;
  let fixture: ComponentFixture<GameCreatePage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(GameCreatePage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
