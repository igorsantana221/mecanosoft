import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuoteList } from './quote-list';

describe('QuoteList', () => {
  let component: QuoteList;
  let fixture: ComponentFixture<QuoteList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuoteList],
    }).compileComponents();

    fixture = TestBed.createComponent(QuoteList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
