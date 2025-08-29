export interface Workflow {
  workflowId: number;
  workflowName: string;
  isActive: boolean;
  multiExecBehavior: string;
}

export interface SyncResult {
  message: string;
  inserted: number;
  deleted: number;
  updated: number;
}

export interface RunWorkflowResult {
  message: string;
}
