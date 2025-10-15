import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Server } from '../../constants/server';
import { DataCountDTO } from '../../interfaces/data-count-dto';

@Injectable({
  providedIn: 'root'
})
export class DatabaseApiService {
  private readonly baseUrl = Server.apiUrl + 'database';

  constructor(private http: HttpClient) {}

  getTablesCountForDatabases(): Observable<DataCountDTO> {
    return this.http.get<DataCountDTO>(`${this.baseUrl}/tables-count`);
  }
}