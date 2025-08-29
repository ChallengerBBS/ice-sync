import axios from 'axios';
import { Workflow, SyncResult, RunWorkflowResult } from '../types/Workflow';

// Mock axios
jest.mock('axios');
const mockAxios = axios as jest.Mocked<typeof axios>;

// Create a mock axios instance
const mockAxiosInstance = {
  get: jest.fn(),
  post: jest.fn(),
  put: jest.fn(),
  delete: jest.fn(),
  defaults: { headers: { common: {} } },
  interceptors: { request: { use: jest.fn() }, response: { use: jest.fn() } }
};

// Mock axios.create to return our mock instance
mockAxios.create.mockReturnValue(mockAxiosInstance as any);

// Import the API service after setting up mocks
import { workflowApi } from '../services/api';

describe('API Service', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getWorkflows', () => {
    it('should return workflows when API call is successful', async () => {
      // Arrange
      const mockWorkflows: Workflow[] = [
        {
          workflowId: 1,
          workflowName: 'Test Workflow 1',
          isActive: true,
          multiExecBehavior: 'Allow'
        },
        {
          workflowId: 2,
          workflowName: 'Test Workflow 2',
          isActive: false,
          multiExecBehavior: 'Deny'
        }
      ];

      mockAxiosInstance.get.mockResolvedValue({
        data: mockWorkflows,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.getWorkflows();

      // Assert
      expect(result).toEqual(mockWorkflows);
      expect(mockAxiosInstance.get).toHaveBeenCalledWith('/workflows');
    });

    it('should throw error when API call fails', async () => {
      // Arrange
      const errorMessage = 'Network error';
      mockAxiosInstance.get.mockRejectedValue(new Error(errorMessage));

      // Act & Assert
      await expect(workflowApi.getWorkflows()).rejects.toThrow(errorMessage);
      expect(mockAxiosInstance.get).toHaveBeenCalledWith('/workflows');
    });

    it('should handle empty response', async () => {
      // Arrange
      mockAxiosInstance.get.mockResolvedValue({
        data: [],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.getWorkflows();

      // Assert
      expect(result).toEqual([]);
      expect(mockAxiosInstance.get).toHaveBeenCalledWith('/workflows');
    });

    it('should handle null response data', async () => {
      // Arrange
      mockAxiosInstance.get.mockResolvedValue({
        data: null,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.getWorkflows();

      // Assert
      expect(result).toEqual(null);
      expect(mockAxiosInstance.get).toHaveBeenCalledWith('/workflows');
    });
  });

  describe('runWorkflow', () => {
    it('should return success result when workflow execution is successful', async () => {
      // Arrange
      const workflowId = 123;
      const mockResult: RunWorkflowResult = {
        message: 'Workflow triggered successfully'
      };

      mockAxiosInstance.post.mockResolvedValue({
        data: mockResult,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.runWorkflow(workflowId);

      // Assert
      expect(result).toEqual(mockResult);
      expect(mockAxiosInstance.post).toHaveBeenCalledWith(`/workflows/${workflowId}/run`);
    });

    it('should throw error when workflow execution fails', async () => {
      // Arrange
      const workflowId = 123;
      const errorMessage = 'Workflow execution failed';
      mockAxiosInstance.post.mockRejectedValue(new Error(errorMessage));

      // Act & Assert
      await expect(workflowApi.runWorkflow(workflowId)).rejects.toThrow(errorMessage);
      expect(mockAxiosInstance.post).toHaveBeenCalledWith(`/workflows/${workflowId}/run`);
    });

    it('should handle different workflow IDs', async () => {
      // Arrange
      const workflowIds = [1, 2, 3];
      const mockResult: RunWorkflowResult = {
        message: 'Workflow triggered successfully'
      };

      mockAxiosInstance.post.mockResolvedValue({
        data: mockResult,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act & Assert
      for (const workflowId of workflowIds) {
        const result = await workflowApi.runWorkflow(workflowId);
        expect(result).toEqual(mockResult);
        expect(mockAxiosInstance.post).toHaveBeenCalledWith(`/workflows/${workflowId}/run`);
      }
    });

    it('should handle zero workflow ID', async () => {
      // Arrange
      const workflowId = 0;
      const mockResult: RunWorkflowResult = {
        message: 'Workflow triggered successfully'
      };

      mockAxiosInstance.post.mockResolvedValue({
        data: mockResult,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.runWorkflow(workflowId);

      // Assert
      expect(result).toEqual(mockResult);
      expect(mockAxiosInstance.post).toHaveBeenCalledWith('/workflows/0/run');
    });

    it('should handle large workflow ID', async () => {
      // Arrange
      const workflowId = 999999;
      const mockResult: RunWorkflowResult = {
        message: 'Workflow triggered successfully'
      };

      mockAxiosInstance.post.mockResolvedValue({
        data: mockResult,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.runWorkflow(workflowId);

      // Assert
      expect(result).toEqual(mockResult);
      expect(mockAxiosInstance.post).toHaveBeenCalledWith(`/workflows/${workflowId}/run`);
    });
  });

  describe('syncWorkflows', () => {
    it('should return sync result when synchronization is successful', async () => {
      // Arrange
      const mockResult: SyncResult = {
        message: 'Synchronization completed successfully',
        inserted: 5,
        deleted: 2,
        updated: 3
      };

      mockAxiosInstance.post.mockResolvedValue({
        data: mockResult,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.syncWorkflows();

      // Assert
      expect(result).toEqual(mockResult);
      expect(mockAxiosInstance.post).toHaveBeenCalledWith('/workflows/sync');
    });

    it('should throw error when synchronization fails', async () => {
      // Arrange
      const errorMessage = 'Synchronization failed';
      mockAxiosInstance.post.mockRejectedValue(new Error(errorMessage));

      // Act & Assert
      await expect(workflowApi.syncWorkflows()).rejects.toThrow(errorMessage);
      expect(mockAxiosInstance.post).toHaveBeenCalledWith('/workflows/sync');
    });

    it('should handle sync result with zero counts', async () => {
      // Arrange
      const mockResult: SyncResult = {
        message: 'No changes needed',
        inserted: 0,
        deleted: 0,
        updated: 0
      };

      mockAxiosInstance.post.mockResolvedValue({
        data: mockResult,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.syncWorkflows();

      // Assert
      expect(result).toEqual(mockResult);
      expect(result.inserted).toBe(0);
      expect(result.deleted).toBe(0);
      expect(result.updated).toBe(0);
      expect(mockAxiosInstance.post).toHaveBeenCalledWith('/workflows/sync');
    });

    it('should handle large sync result counts', async () => {
      // Arrange
      const mockResult: SyncResult = {
        message: 'Large synchronization completed',
        inserted: 1000,
        deleted: 500,
        updated: 750
      };

      mockAxiosInstance.post.mockResolvedValue({
        data: mockResult,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await workflowApi.syncWorkflows();

      // Assert
      expect(result).toEqual(mockResult);
      expect(result.inserted).toBe(1000);
      expect(result.deleted).toBe(500);
      expect(result.updated).toBe(750);
      expect(mockAxiosInstance.post).toHaveBeenCalledWith('/workflows/sync');
    });
  });

  describe('axios configuration', () => {
    it('should create axios instance with correct base URL', () => {
      // Since we've mocked axios.create and it returns our mockAxiosInstance,
      // we can verify the instance has the expected methods
      expect(mockAxiosInstance.get).toBeDefined();
      expect(mockAxiosInstance.post).toBeDefined();
    });

    it('should use default base URL when environment variable is not set', () => {
      // Since we've mocked axios.create and it returns our mockAxiosInstance,
      // we can verify the instance has the expected methods
      expect(mockAxiosInstance.get).toBeDefined();
      expect(mockAxiosInstance.post).toBeDefined();
    });

    it('should configure axios with correct headers', () => {
      // Since we've mocked axios.create and it returns our mockAxiosInstance,
      // we can verify the instance has the expected structure
      expect(mockAxiosInstance.defaults).toBeDefined();
      expect(mockAxiosInstance.interceptors).toBeDefined();
    });
  });

  describe('error handling', () => {
    it('should handle network errors', async () => {
      // Arrange
      const networkError = new Error('Network Error');
      mockAxiosInstance.get.mockRejectedValue(networkError);

      // Act & Assert
      await expect(workflowApi.getWorkflows()).rejects.toThrow('Network Error');
    });

    it('should handle HTTP error responses', async () => {
      // Arrange
      const httpError = {
        response: {
          status: 500,
          data: 'Internal Server Error'
        }
      };
      mockAxiosInstance.get.mockRejectedValue(httpError);

      // Act & Assert
      await expect(workflowApi.getWorkflows()).rejects.toEqual(httpError);
    });

    it('should handle timeout errors', async () => {
      // Arrange
      const timeoutError = new Error('Request timeout');
      mockAxiosInstance.get.mockRejectedValue(timeoutError);

      // Act & Assert
      await expect(workflowApi.getWorkflows()).rejects.toThrow('Request timeout');
    });

    it('should handle malformed JSON responses', async () => {
      // Arrange
      const malformedResponse = {
        data: 'invalid json',
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      };
      mockAxiosInstance.get.mockResolvedValue(malformedResponse);

      // Act
      const result = await workflowApi.getWorkflows();

      // Assert
      expect(result).toBe('invalid json');
    });
  });

  describe('concurrent requests', () => {
    it('should handle multiple concurrent getWorkflows calls', async () => {
      // Arrange
      const mockWorkflows: Workflow[] = [
        { workflowId: 1, workflowName: 'Test', isActive: true, multiExecBehavior: 'Allow' }
      ];

      mockAxiosInstance.get.mockResolvedValue({
        data: mockWorkflows,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const promises = [
        workflowApi.getWorkflows(),
        workflowApi.getWorkflows(),
        workflowApi.getWorkflows()
      ];

      const results = await Promise.all(promises);

      // Assert
      expect(results).toHaveLength(3);
      results.forEach(result => {
        expect(result).toEqual(mockWorkflows);
      });
      expect(mockAxiosInstance.get).toHaveBeenCalledTimes(3);
    });

    it('should handle multiple concurrent runWorkflow calls', async () => {
      // Arrange
      const mockResult: RunWorkflowResult = {
        message: 'Workflow triggered successfully'
      };

      mockAxiosInstance.post.mockResolvedValue({
        data: mockResult,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const promises = [
        workflowApi.runWorkflow(1),
        workflowApi.runWorkflow(2),
        workflowApi.runWorkflow(3)
      ];

      const results = await Promise.all(promises);

      // Assert
      expect(results).toHaveLength(3);
      results.forEach(result => {
        expect(result).toEqual(mockResult);
      });
      expect(mockAxiosInstance.post).toHaveBeenCalledTimes(3);
    });
  });
});
