import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from '../App';
import { workflowApi } from '../services/api';
import toastr from 'toastr';

// Mock the API service
jest.mock('../services/api');
const mockWorkflowApi = workflowApi as jest.Mocked<typeof workflowApi>;

// Mock toastr
jest.mocked(toastr);

describe('App Component', () => {
  const mockWorkflows = [
    {
      workflowId: '1',
      workflowName: 'Test Workflow 1',
      isActive: true,
      multiExecBehavior: 'Allow'
    },
    {
      workflowId: '2',
      workflowName: 'Test Workflow 2',
      isActive: false,
      multiExecBehavior: 'Deny'
    }
  ];

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Initial Load', () => {
    it('should render the app title', () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue([]);
      render(<App />);
      
      expect(screen.getByText('🍦 IceSync - Workflow Management')).toBeInTheDocument();
    });

    it('should show loading state initially', () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue([]);
      render(<App />);
      
      expect(screen.getByText('Loading workflows...')).toBeInTheDocument();
    });

    it('should load workflows on mount', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      render(<App />);
      
      await waitFor(() => {
        expect(mockWorkflowApi.getWorkflows).toHaveBeenCalledTimes(1);
      });
    });

    it('should display workflows in table after loading', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Test Workflow 1')).toBeInTheDocument();
        expect(screen.getByText('Test Workflow 2')).toBeInTheDocument();
      });
    });

    it('should show correct workflow status badges', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Active')).toBeInTheDocument();
        expect(screen.getByText('Inactive')).toBeInTheDocument();
      });
    });

    it('should display correct statistics', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      render(<App />);
      
      await waitFor(() => {
        // Check statistics using more specific selectors
        const stats = screen.getByText('Total Workflows').closest('.stat-card');
        expect(stats).toHaveTextContent('2');
        
        const activeStats = screen.getByText('Active Workflows').closest('.stat-card');
        expect(activeStats).toHaveTextContent('1');
        
        const inactiveStats = screen.getByText('Inactive Workflows').closest('.stat-card');
        expect(inactiveStats).toHaveTextContent('1');
      });
    });
  });

  describe('Error Handling', () => {
    it('should display error message when loading workflows fails', async () => {
      mockWorkflowApi.getWorkflows.mockRejectedValue(new Error('API Error'));
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Failed to load workflows. Please try again.')).toBeInTheDocument();
      });
    });

    it('should show toastr error when loading fails', async () => {
      mockWorkflowApi.getWorkflows.mockRejectedValue(new Error('API Error'));
      render(<App />);
      
      await waitFor(() => {
        expect(toastr.error).toHaveBeenCalledWith(
          'Failed to load workflows. Please try again.',
          'Loading Error'
        );
      });
    });

    it('should show info toastr when no workflows are found', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue([]);
      render(<App />);
      
      await waitFor(() => {
        expect(toastr.info).toHaveBeenCalledWith(
          'No workflows found. Try synchronizing with the Universal Loader API.',
          'No Workflows'
        );
      });
    });
  });

  describe('Sync Functionality', () => {
    it('should call sync API when sync button is clicked', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.syncWorkflows.mockResolvedValue({
        message: 'Sync completed',
        inserted: 2,
        deleted: 0,
        updated: 0
      });

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('🔄 Sync Workflows')).toBeInTheDocument();
      });

      const syncButton = screen.getByText('🔄 Sync Workflows');
      fireEvent.click(syncButton);

      await waitFor(() => {
        expect(mockWorkflowApi.syncWorkflows).toHaveBeenCalledTimes(1);
      });
    });

    it('should show loading state during sync', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.syncWorkflows.mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve({
          message: 'Sync completed',
          inserted: 2,
          deleted: 0,
          updated: 0
        }), 100))
      );

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('🔄 Sync Workflows')).toBeInTheDocument();
      });

      const syncButton = screen.getByText('🔄 Sync Workflows');
      fireEvent.click(syncButton);

      expect(screen.getByText('Synchronizing...')).toBeInTheDocument();
    });

    it('should display sync results after successful sync', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.syncWorkflows.mockResolvedValue({
        message: 'Sync completed',
        inserted: 2,
        deleted: 1,
        updated: 3
      });

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('🔄 Sync Workflows')).toBeInTheDocument();
      });

      const syncButton = screen.getByText('🔄 Sync Workflows');
      fireEvent.click(syncButton);

      await waitFor(() => {
        expect(screen.getByText(/Sync completed - Inserted: 2, Deleted: 1, Updated: 3/)).toBeInTheDocument();
      });
    });

    it('should show toastr success message after sync', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.syncWorkflows.mockResolvedValue({
        message: 'Sync completed',
        inserted: 2,
        deleted: 0,
        updated: 0
      });

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('🔄 Sync Workflows')).toBeInTheDocument();
      });

      const syncButton = screen.getByText('🔄 Sync Workflows');
      fireEvent.click(syncButton);

      await waitFor(() => {
        expect(toastr.success).toHaveBeenCalledWith(
          'Sync completed - Inserted: 2, Deleted: 0, Updated: 0',
          'Synchronization Complete'
        );
      });
    });

    it('should handle sync errors', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.syncWorkflows.mockRejectedValue(new Error('Sync failed'));

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('🔄 Sync Workflows')).toBeInTheDocument();
      });

      const syncButton = screen.getByText('🔄 Sync Workflows');
      fireEvent.click(syncButton);

      await waitFor(() => {
        expect(screen.getByText('Failed to synchronize workflows. Please try again.')).toBeInTheDocument();
        expect(toastr.error).toHaveBeenCalledWith(
          'Failed to synchronize workflows. Please try again.',
          'Synchronization Error'
        );
      });
    });
  });

  describe('Workflow Execution', () => {
    it('should call run workflow API when run button is clicked', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.runWorkflow.mockResolvedValue({ message: 'Workflow triggered successfully' });

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getAllByText('Run')).toHaveLength(2);
      });

      const runButtons = screen.getAllByText('Run');
      fireEvent.click(runButtons[0]);

      await waitFor(() => {
        expect(mockWorkflowApi.runWorkflow).toHaveBeenCalledWith('1');
      });
    });

    it('should show loading state during workflow execution', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.runWorkflow.mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve({ message: 'Success' }), 100))
      );

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getAllByText('Run')).toHaveLength(2);
      });

      const runButtons = screen.getAllByText('Run');
      fireEvent.click(runButtons[0]);

      expect(screen.getByText('Running...')).toBeInTheDocument();
    });

    it('should show toastr success message after successful workflow execution', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.runWorkflow.mockResolvedValue({ message: 'Workflow triggered successfully' });

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getAllByText('Run')).toHaveLength(2);
      });

      const runButtons = screen.getAllByText('Run');
      fireEvent.click(runButtons[0]);

      await waitFor(() => {
        expect(toastr.success).toHaveBeenCalledWith(
          'Workflow triggered successfully',
          'Workflow Execution'
        );
      });
    });

    it('should handle workflow execution errors', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.runWorkflow.mockRejectedValue(new Error('Execution failed'));

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getAllByText('Run')).toHaveLength(2);
      });

      const runButtons = screen.getAllByText('Run');
      fireEvent.click(runButtons[0]);

      await waitFor(() => {
        expect(toastr.error).toHaveBeenCalledWith(
          'Failed to run workflow. Please try again.',
          'Workflow Execution Error'
        );
      });
    });

    it('should disable run button during execution', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.runWorkflow.mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve({ message: 'Success' }), 100))
      );

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getAllByText('Run')).toHaveLength(2);
      });

      const runButtons = screen.getAllByText('Run');
      fireEvent.click(runButtons[0]);

      expect(runButtons[0]).toBeDisabled();
    });
  });

  describe('Table Display', () => {
    it('should display all workflow columns correctly', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Workflow ID')).toBeInTheDocument();
        expect(screen.getByText('Workflow Name')).toBeInTheDocument();
        expect(screen.getByText('Status')).toBeInTheDocument();
        expect(screen.getByText('Multi Exec Behavior')).toBeInTheDocument();
        expect(screen.getByText('Actions')).toBeInTheDocument();
      });
    });

    it('should show correct workflow data in table', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('Test Workflow 1')).toBeInTheDocument(); // Workflow Name
        expect(screen.getByText('Allow')).toBeInTheDocument(); // Multi Exec Behavior
        expect(screen.getByText('Test Workflow 2')).toBeInTheDocument(); // Second workflow name
        expect(screen.getByText('Deny')).toBeInTheDocument(); // Second workflow behavior
        // Check that workflow IDs are present in table
        const table = screen.getByRole('table');
        expect(table).toHaveTextContent('1');
        expect(table).toHaveTextContent('2');
      });
    });

    it('should show message when no workflows exist', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue([]);
      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('No workflows found. Try synchronizing with the Universal Loader API.')).toBeInTheDocument();
      });
    });
  });

  describe('User Interactions', () => {
    it('should handle multiple rapid clicks on sync button', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.syncWorkflows.mockResolvedValue({
        message: 'Sync completed',
        inserted: 0,
        deleted: 0,
        updated: 0
      });

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getByText('🔄 Sync Workflows')).toBeInTheDocument();
      });

      const syncButton = screen.getByText('🔄 Sync Workflows');
      fireEvent.click(syncButton);
      fireEvent.click(syncButton);
      fireEvent.click(syncButton);

      await waitFor(() => {
        expect(mockWorkflowApi.syncWorkflows).toHaveBeenCalledTimes(1); // Should only call once due to loading state
      });
    });

    it('should handle multiple rapid clicks on run buttons', async () => {
      mockWorkflowApi.getWorkflows.mockResolvedValue(mockWorkflows);
      mockWorkflowApi.runWorkflow.mockResolvedValue({ message: 'Success' });

      render(<App />);
      
      await waitFor(() => {
        expect(screen.getAllByText('Run')).toHaveLength(2);
      });

      const runButtons = screen.getAllByText('Run');
      fireEvent.click(runButtons[0]);
      fireEvent.click(runButtons[0]); // Second click should be ignored

      await waitFor(() => {
        expect(mockWorkflowApi.runWorkflow).toHaveBeenCalledTimes(1); // Should only call once
      });
    });
  });
});
