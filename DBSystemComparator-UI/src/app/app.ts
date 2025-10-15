import { Component, OnDestroy, OnInit} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { Navbar } from "./components/navbar/navbar";
import { Footer } from "./components/footer/footer";
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastModule, Navbar, Footer],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  private accessTokenSub?: Subscription;

  constructor() {}

  ngOnInit() {
    
  }

  ngOnDestroy() {
    this.accessTokenSub?.unsubscribe();
  }
}