export interface Flight{
	carrier:string;
	routes:Route[];
	flightNumber:number;
	agentId:number;
} 
export interface Route{
	origin:string;
	destination:string;
	flightDuration:number;
}
