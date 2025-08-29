import axios from 'axios';
import { Workflow, SyncResult, RunWorkflowResult } from '../types/Workflow';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7041/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const workflowApi = {
  getWorkflows: async (): Promise<Workflow[]> => {
    const response = await api.get<Workflow[]>('/workflows');
    return response.data;
  },

  runWorkflow: async (workflowId: number): Promise<RunWorkflowResult> => {
    const response = await api.post<RunWorkflowResult>(`/workflows/${workflowId}/run`);
    return response.data;
  },

  syncWorkflows: async (): Promise<SyncResult> => {
    const response = await api.post<SyncResult>('/workflows/sync');
    return response.data;
  },
};
