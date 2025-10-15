import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TabsModule } from 'primeng/tabs';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DatabaseApiService } from '../../services/api/database-api-service';
import { finalize } from 'rxjs';
import { DataCountDTO } from '../../interfaces/data-count-dto';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-databases',
  templateUrl: './databases.html',
  styleUrl: './databases.css',
  imports: [CardModule, NgxChartsModule, TooltipModule, ProgressSpinnerModule, NgIf, CardModule, TabsModule, ProgressSpinnerModule]
})
export class DatabasesComponent implements OnInit {
  dataCount: DataCountDTO = {
    postgreSQL: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
    sqlServer: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
    mongoDB: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
    cassandra: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0}
  };
  loadingDatabases = false;
  activeTabIndex = 0;

  constructor(private cdr: ChangeDetectorRef, private databaseApiService: DatabaseApiService) {}

  ngOnInit(): void {
    this.getTablesCountForDatabases();
  }

  onTabChange(index: any): void {
    this.activeTabIndex = index;
  }

  getTablesCountForDatabases(): void {
    this.loadingDatabases = true;
    this.databaseApiService.getTablesCountForDatabases().pipe(
      finalize(() => {
        this.loadingDatabases = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (data) => {
        this.dataCount = data;
      },
      error: () => {
        this.dataCount = {
          postgreSQL: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
          sqlServer: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
          mongoDB: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
          cassandra: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0}
        };
      }
    });
  }
}