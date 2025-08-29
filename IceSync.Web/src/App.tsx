import React, { useState, useEffect } from 'react';
import { Workflow } from './types/Workflow';
import { workflowApi } from './services/api';
import toastr from 'toastr';
import 'toastr/build/toastr.min.css';
import './App.css';

toastr.options = {
  closeButton: true,
  progressBar: true,
  positionClass: 'toast-top-right',
  timeOut: 5000,
  extendedTimeOut: 2000,
  preventDuplicates: true,
  newestOnTop: true
};

function App() {
  const [workflows, setWorkflows] = useState<Workflow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [syncLoading, setSyncLoading] = useState(false);
  const [syncResult, setSyncResult] = useState<string | null>(null);
  const [runningWorkflows, setRunningWorkflows] = useState<Set<number>>(new Set());

  useEffect(() => {
    loadWorkflows();
  }, []);

  const loadWorkflows = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await workflowApi.getWorkflows();
      setWorkflows(data);
      if (data.length === 0) {
        toastr.info('No workflows found. Try synchronizing with the Universal Loader API.', 'No Workflows');
      }
    } catch (err) {
      const errorMessage = 'Failed to load workflows. Please try again.';
      setError(errorMessage);
      toastr.error(errorMessage, 'Loading Error');
      console.error('Error loading workflows:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSync = async () => {
    try {
      setSyncLoading(true);
      setError(null);
      setSyncResult(null);
      const result = await workflowApi.syncWorkflows();
      const syncMessage = `${result.message} - Inserted: ${result.inserted}, Deleted: ${result.deleted}, Updated: ${result.updated}`;
      setSyncResult(syncMessage);
      toastr.success(syncMessage, 'Synchronization Complete');
      await loadWorkflows();
    } catch (err) {
      const errorMessage = 'Failed to synchronize workflows. Please try again.';
      setError(errorMessage);
      toastr.error(errorMessage, 'Synchronization Error');
      console.error('Error syncing workflows:', err);
    } finally {
      setSyncLoading(false);
    }
  };

  const handleRunWorkflow = async (workflowId: number) => {
    try {
      setRunningWorkflows(prev => new Set(prev).add(workflowId));
      const result = await workflowApi.runWorkflow(workflowId);
      toastr.success(result.message, 'Workflow Execution');
    } catch (err) {
      toastr.error('Failed to run workflow. Please try again.', 'Workflow Execution Error');
      console.error('Error running workflow:', err);
    } finally {
      setRunningWorkflows(prev => {
        const newSet = new Set(prev);
        newSet.delete(workflowId);
        return newSet;
      });
    }
  };

  const activeWorkflows = workflows.filter(w => w.isActive).length;
  const inactiveWorkflows = workflows.filter(w => !w.isActive).length;

  return (
    <div className="App">
      <div className="header">
        <div className="container">
          <h1>🍦 IceSync - Workflow Management</h1>
        </div>
      </div>

      <div className="container">
        {error && <div className="error">{error}</div>}
        {syncResult && <div className="success">{syncResult}</div>}

        <div className="stats">
          <div className="stat-card">
            <div className="stat-number">{workflows.length}</div>
            <div className="stat-label">Total Workflows</div>
          </div>
          <div className="stat-card">
            <div className="stat-number">{activeWorkflows}</div>
            <div className="stat-label">Active Workflows</div>
          </div>
          <div className="stat-card">
            <div className="stat-number">{inactiveWorkflows}</div>
            <div className="stat-label">Inactive Workflows</div>
          </div>
        </div>

        <button 
          className="sync-button" 
          onClick={handleSync} 
          disabled={syncLoading}
        >
          {syncLoading ? 'Synchronizing...' : '🔄 Sync Workflows'}
        </button>

        {loading ? (
          <div className="loading">Loading workflows...</div>
        ) : (
          <div className="workflow-table">
            <table>
              <thead>
                <tr>
                  <th>Workflow ID</th>
                  <th>Workflow Name</th>
                  <th>Status</th>
                  <th>Multi Exec Behavior</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {workflows.map((workflow) => (
                  <tr key={workflow.workflowId}>
                    <td>{workflow.workflowId}</td>
                    <td>{workflow.workflowName}</td>
                    <td>
                      <span className={`status-badge ${workflow.isActive ? 'status-active' : 'status-inactive'}`}>
                        {workflow.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>{workflow.multiExecBehavior}</td>
                    <td>
                      <button
                        className="run-button"
                        onClick={() => handleRunWorkflow(workflow.workflowId)}
                        disabled={runningWorkflows.has(workflow.workflowId)}
                      >
                        {runningWorkflows.has(workflow.workflowId) ? 'Running...' : 'Run'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {workflows.length === 0 && (
              <div className="loading">No workflows found. Try synchronizing with the Universal Loader API.</div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
