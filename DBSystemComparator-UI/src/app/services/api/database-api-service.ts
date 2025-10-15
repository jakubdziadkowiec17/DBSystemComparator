import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Server } from '../../constants/server';
import { DataCountDTO } from '../../interfaces/data-count-dto';
import { GenerateDataDTO } from '../../interfaces/generate-data-dto';
import { ResponseDTO } from '../../interfaces/response-dto';

@Injectable({
  providedIn: 'root'
})
export class DatabaseApiService {
  private readonly baseUrl = Server.apiUrl + 'database';

  constructor(private http: HttpClient) {}

  generateData(generateDataDTO: GenerateDataDTO): Observable<ResponseDTO> {
      return this.http.post<ResponseDTO>(`${this.baseUrl}/generate-data`, generateDataDTO);
  }
  
  getTablesCountForDatabases(): Observable<DataCountDTO> {
    return this.http.get<DataCountDTO>(`${this.baseUrl}/tables-count`);
  }
}