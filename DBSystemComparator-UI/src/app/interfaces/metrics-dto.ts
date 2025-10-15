import { ChartDTO } from "./chart-dto";

export interface MetricsDTO {
  time: ChartDTO[];
  ramUsage: ChartDTO[];
  cpuUsage: ChartDTO[];
}