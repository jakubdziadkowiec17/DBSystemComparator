import { TablesCountDTO } from "./tables-count-dto";

export interface DataCountDTO {
  postgreSQL: TablesCountDTO;
  sqlServer: TablesCountDTO;
  mongoDB: TablesCountDTO;
  cassandra: TablesCountDTO;
}