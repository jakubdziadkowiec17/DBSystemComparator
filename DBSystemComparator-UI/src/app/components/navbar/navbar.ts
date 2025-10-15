import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Menubar } from 'primeng/menubar';
import { BadgeModule } from 'primeng/badge';
import { InputTextModule } from 'primeng/inputtext';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [Menubar, BadgeModule, InputTextModule, CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class Navbar implements OnInit {
  items: MenuItem[] = [];

  constructor() {}

  ngOnInit() {
    this.buildNavbar();
  }

  buildNavbar() {
    this.items = [
          {
            label: 'Home',
            icon: 'pi pi-home',
            routerLink: ['/']
          },
          {
            label: 'Databases',
            icon: 'pi pi-database',
            routerLink: ['/databases']
          }
        ];
  }
}