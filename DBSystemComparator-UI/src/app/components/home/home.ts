import { ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, NgIf } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { StepperModule } from 'primeng/stepper';
import { InputTextModule } from 'primeng/inputtext';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { MessageModule } from 'primeng/message';
import { SelectedScenarioDTO } from '../../interfaces/selected-scenario-dto';
import { ScenarioApiService } from '../../services/api/scenario-api-service';
import { MetricsDTO } from '../../interfaces/metrics-dto';
import { finalize } from 'rxjs';
import { ScenarioDTO } from '../../interfaces/scenario-dto';
import { AccordionModule } from 'primeng/accordion';
import { Operation } from '../../constants/operation';
import { DB } from '../../constants/db';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NgIf,CommonModule, NgxChartsModule, ReactiveFormsModule, ProgressSpinnerModule, FormsModule, CardModule, ButtonModule, StepperModule, InputTextModule, ToggleSwitchModule, MessageModule, AccordionModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class HomeComponent implements OnInit, OnDestroy {
  @ViewChild('timeContainer') timeContainer!: ElementRef;
  @ViewChild('ramUsageContainer') ramUsageContainer!: ElementRef;
  @ViewChild('cpuUsageContainer') cpuUsageContainer!: ElementRef;
  timeData: { name: string, value: number }[] = [];
  ramUsageData: { name: string, value: number }[] = [];
  cpuUsageData: { name: string, value: number }[] = [];
  view: [number, number] = [800, 300];
  activeStep: number = 1;
  selectedScenario: ScenarioDTO | undefined;
  scenarios: ScenarioDTO[] = [];
  loadingScenarios = false;
  loadingTest = false;
  dbs = [
      { id: DB.POSTGRESQL, name: 'PostgreSQL' },
      { id: DB.SQLSERVER, name: 'SQL Server' },
      { id: DB.MONGODB, name: 'MongoDB' },
      { id: DB.CASSANDRA, name: 'Cassandra' }
  ];

  constructor(private cdr: ChangeDetectorRef, private scenarioApiService: ScenarioApiService) {}

  ngOnInit() {
    window.addEventListener('resize', this.updateChartSize.bind(this));
    this.getScenarios();
  }

  ngOnDestroy(): void {
    window.removeEventListener('resize', this.updateChartSize.bind(this));
  }

  get createScenarios(): ScenarioDTO[] {
    return this.scenarios.filter(a => a.operationId === Operation.CREATE);
  }

  get readScenarios(): ScenarioDTO[] {
    return this.scenarios.filter(a => a.operationId === Operation.READ);
  }

  get updateScenarios(): ScenarioDTO[] {
    return this.scenarios.filter(a => a.operationId === Operation.UPDATE);
  }

  get deleteScenarios(): ScenarioDTO[] {
    return this.scenarios.filter(a => a.operationId === Operation.DELETE);
  }

  getMetricsByDatabase(): { dbName: string, time?: number, ram?: number, cpu?: number }[] {
    return this.dbs.map(db => {
      const time = this.timeData.find(t => t.name === db.name)?.value;
      const ram = this.ramUsageData.find(r => r.name === db.name)?.value;
      const cpu = this.cpuUsageData.find(c => c.name === db.name)?.value;

      return { dbName: db.name, time, ram, cpu };
    });
  }

  updateChartSize(): void {
    if (this.timeContainer && this.timeContainer.nativeElement) {
      const width = this.timeContainer.nativeElement.offsetWidth;
      this.view = [width, 400];
    }
    if (this.ramUsageContainer && this.ramUsageContainer.nativeElement) {
      const width = this.ramUsageContainer.nativeElement.offsetWidth;
      this.view = [width, 400];
    }
    if (this.cpuUsageContainer && this.cpuUsageContainer.nativeElement) {
      const width = this.cpuUsageContainer.nativeElement.offsetWidth;
      this.view = [width, 400];
    }
  }

  setScenario(scenario: ScenarioDTO) {
    const isAlreadySelected = scenario.selected;

    this.scenarios = this.scenarios.map(s => ({
      ...s,
      selected: s.id === scenario.id ? isAlreadySelected : false
    }));

    this.selectedScenario = isAlreadySelected ? scenario : undefined;
    this.resetMetrics();
    this.cdr.detectChanges();
  }

  resetMetrics() {
    this.timeData = [];
    this.ramUsageData = [];
    this.cpuUsageData = [];
  }

  goToStep1() {
    this.activeStep = 1;
  }
  
  goToStep2() {
    if (!this.selectedScenario) return;
    this.activeStep = 2;
  }

  getScenarios() {
    this.loadingScenarios = true;
    this.scenarioApiService.getScenarios().pipe(
      finalize(() => {
        this.loadingScenarios = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (data) => {
        this.scenarios = data;
        this.cdr.detectChanges();
      },
      error: () => {
        this.scenarios = [];
      }
    });
  }

  testScenario() {
    if(!this.selectedScenario) return;
    const scenario: SelectedScenarioDTO = { id: this.selectedScenario.id };

    this.loadingTest = true;
    this.scenarioApiService.testScenario(scenario).subscribe({
      next: (data) => {
        this.updateChartData(data);
        this.loadingTest = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.resetMetrics();
        this.loadingTest = false;
        this.cdr.detectChanges();
      }
    });
  }

  updateChartData(metricsDTO: MetricsDTO): void {
    this.timeData = metricsDTO.time.map(item => ({
      name: this.dbs.find(db => db.id === item.database)?.name || 'Unknown',
      value: item.result
    }));

    this.ramUsageData = metricsDTO.ramUsage.map(item => ({
      name: this.dbs.find(db => db.id === item.database)?.name || 'Unknown',
      value: item.result
    }));

    this.cpuUsageData = metricsDTO.cpuUsage.map(item => ({
      name: this.dbs.find(db => db.id === item.database)?.name || 'Unknown',
      value: item.result
    }));

    this.cdr.detectChanges();
  }
}