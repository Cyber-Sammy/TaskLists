import { ApiClient, type ApiClientConfig } from './ApiClient';
import type { RequestOptions } from './ApiClient';
import type {
  AddTaskListMemberRequest,
  CreateTaskItemRequest,
  CreateTaskListRequest,
  CreateUserRequest,
  GetTaskItemsParams,
  GetTaskListsParams,
  Guid,
  PagedResponse,
  TaskItemResponse,
  TaskListDetailsResponse,
  TaskListMemberResponse,
  TaskListSummaryResponse,
  UpdateTaskItemRequest,
  UpdateTaskListRequest,
  UserResponse,
} from './types';

export class TaskListsApiProvider {
  private readonly apiClient: ApiClient;

  public constructor(config: ApiClientConfig | ApiClient) {
    this.apiClient = config instanceof ApiClient
      ? config
      : new ApiClient(config);
  }

  public getTaskLists(params: GetTaskListsParams = {}, signal?: AbortSignal): Promise<PagedResponse<TaskListSummaryResponse>> {
    return this.apiClient.request('/api/task-lists', {
      query: params,
      ...withSignal(signal),
    });
  }

  public createTaskList(request: CreateTaskListRequest, signal?: AbortSignal): Promise<TaskListDetailsResponse> {
    return this.apiClient.request('/api/task-lists', {
      method: 'POST',
      body: request,
      ...withSignal(signal),
    });
  }

  public getTaskList(taskListId: Guid, signal?: AbortSignal): Promise<TaskListDetailsResponse> {
    return this.apiClient.request(`/api/task-lists/${taskListId}`, withSignal(signal));
  }

  public updateTaskList(
    taskListId: Guid,
    request: UpdateTaskListRequest,
    signal?: AbortSignal,
  ): Promise<TaskListDetailsResponse> {
    return this.apiClient.request(`/api/task-lists/${taskListId}`, {
      method: 'PUT',
      body: request,
      ...withSignal(signal),
    });
  }

  public deleteTaskList(taskListId: Guid, signal?: AbortSignal): Promise<void> {
    return this.apiClient.requestEmpty(`/api/task-lists/${taskListId}`, {
      method: 'DELETE',
      ...withSignal(signal),
    });
  }

  public getTaskListMembers(taskListId: Guid, signal?: AbortSignal): Promise<TaskListMemberResponse[]> {
    return this.apiClient.request(`/api/task-lists/${taskListId}/members`, withSignal(signal));
  }

  public addTaskListMember(
    taskListId: Guid,
    request: AddTaskListMemberRequest,
    signal?: AbortSignal,
  ): Promise<TaskListMemberResponse> {
    return this.apiClient.request(`/api/task-lists/${taskListId}/members`, {
      method: 'POST',
      body: request,
      ...withSignal(signal),
    });
  }

  public removeTaskListMember(taskListId: Guid, memberUserId: Guid, signal?: AbortSignal): Promise<void> {
    return this.apiClient.requestEmpty(`/api/task-lists/${taskListId}/members/${memberUserId}`, {
      method: 'DELETE',
      ...withSignal(signal),
    });
  }

  public getTaskItems(
    taskListId: Guid,
    params: GetTaskItemsParams = {},
    signal?: AbortSignal,
  ): Promise<PagedResponse<TaskItemResponse>> {
    return this.apiClient.request(`/api/task-lists/${taskListId}/tasks`, {
      query: params,
      ...withSignal(signal),
    });
  }

  public createTaskItem(
    taskListId: Guid,
    request: CreateTaskItemRequest,
    signal?: AbortSignal,
  ): Promise<TaskItemResponse> {
    return this.apiClient.request(`/api/task-lists/${taskListId}/tasks`, {
      method: 'POST',
      body: request,
      ...withSignal(signal),
    });
  }

  public getTaskItem(taskItemId: Guid, signal?: AbortSignal): Promise<TaskItemResponse> {
    return this.apiClient.request(`/api/tasks/${taskItemId}`, withSignal(signal));
  }

  public updateTaskItem(
    taskItemId: Guid,
    request: UpdateTaskItemRequest,
    signal?: AbortSignal,
  ): Promise<TaskItemResponse> {
    return this.apiClient.request(`/api/tasks/${taskItemId}`, {
      method: 'PUT',
      body: request,
      ...withSignal(signal),
    });
  }

  public deleteTaskItem(taskItemId: Guid, signal?: AbortSignal): Promise<void> {
    return this.apiClient.requestEmpty(`/api/tasks/${taskItemId}`, {
      method: 'DELETE',
      ...withSignal(signal),
    });
  }

  public completeTaskItem(taskItemId: Guid, signal?: AbortSignal): Promise<TaskItemResponse> {
    return this.apiClient.request(`/api/tasks/${taskItemId}/complete`, {
      method: 'POST',
      ...withSignal(signal),
    });
  }

  public reopenTaskItem(taskItemId: Guid, signal?: AbortSignal): Promise<TaskItemResponse> {
    return this.apiClient.request(`/api/tasks/${taskItemId}/reopen`, {
      method: 'POST',
      ...withSignal(signal),
    });
  }

  public createUser(request: CreateUserRequest, signal?: AbortSignal): Promise<UserResponse> {
    return this.apiClient.request('/api/users', {
      method: 'POST',
      body: request,
      ...withSignal(signal),
    });
  }

  public getUsers(signal?: AbortSignal): Promise<UserResponse[]> {
    return this.apiClient.request('/api/users', withSignal(signal));
  }

  public getUser(userId: Guid, signal?: AbortSignal): Promise<UserResponse> {
    return this.apiClient.request(`/api/users/${userId}`, withSignal(signal));
  }
}

function withSignal(signal: AbortSignal | undefined): Pick<RequestOptions, 'signal'> {
  return signal === undefined
    ? {}
    : { signal };
}
