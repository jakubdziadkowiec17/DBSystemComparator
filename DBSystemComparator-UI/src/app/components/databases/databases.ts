import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TabsModule } from 'primeng/tabs';
import { DialogModule } from 'primeng/dialog';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DatabaseApiService } from '../../services/api/database-api-service';
import { finalize } from 'rxjs';
import { DataCountDTO } from '../../interfaces/data-count-dto';
import { NgIf } from '@angular/common';
import { GenerateDataDTO } from '../../interfaces/generate-data-dto';
import { NotificationService } from '../../services/common/notification-service';
import { ButtonModule } from 'primeng/button';
import { Select } from 'primeng/select';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-databases',
  templateUrl: './databases.html',
  styleUrl: './databases.css',
  imports: [CardModule, NgxChartsModule, TooltipModule, FormsModule, Select, DialogModule, ButtonModule, ProgressSpinnerModule, NgIf, CardModule, TabsModule, ProgressSpinnerModule]
})
export class DatabasesComponent implements OnInit {
  dataCount: DataCountDTO = {
    postgreSQL: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
    sqlServer: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
    mongoDB: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0},
    cassandra: {clientsCount: 0, paymentsCount: 0, reservationsCount: 0, reservationsServicesCount: 0, roomsCount: 0, servicesCount: 0}
  };
  loadingDatabases = false;
  loadingGenerateData = false;
  dialogVisible = false;
  activeTabIndex = 0;
  selectedCount: number = 10000;

  constructor(private cdr: ChangeDetectorRef, private databaseApiService: DatabaseApiService, private notificationService: NotificationService) {}

  ngOnInit() {
    this.getTablesCountForDatabases();
  }

  onTabChange(index: any) {
    this.activeTabIndex = index;
  }

  showDialog() {
    this.dialogVisible = true;
  }

  hideDialog() {
    this.dialogVisible = false;
  }

  getTablesCountForDatabases() {
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

  generateData(countForDTO: number) {
    if(!countForDTO) return;
    const generateDataDTO: GenerateDataDTO = { count: countForDTO };
  
    this.loadingGenerateData = true;
    this.databaseApiService.generateData(generateDataDTO).subscribe({
      next: (response) => {
        this.notificationService.showSuccessToast(response.message);
        this.loadingGenerateData = false;
        this.getTablesCountForDatabases();
        this.hideDialog();
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingGenerateData = false;
        this.cdr.detectChanges();
      }
    });
  }
}