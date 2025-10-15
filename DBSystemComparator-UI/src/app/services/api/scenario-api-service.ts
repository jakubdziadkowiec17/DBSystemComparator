import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Server } from '../../constants/server';
import { MetricsDTO } from '../../interfaces/metrics-dto';
import { SelectedScenarioDTO } from '../../interfaces/selected-scenario-dto';
import { ScenarioDTO } from '../../interfaces/scenario-dto';

@Injectable({
  providedIn: 'root'
})
export class ScenarioApiService {
  private readonly baseUrl = Server.apiUrl + 'scenario';

  constructor(private http: HttpClient) {}

  testScenario(selectedScenarioDTO: SelectedScenarioDTO): Observable<MetricsDTO> {
    return this.http.post<MetricsDTO>(this.baseUrl, selectedScenarioDTO);
  }

  getScenarios(): Observable<ScenarioDTO[]> {
    return this.http.get<ScenarioDTO[]>(this.baseUrl);
  }
}