export type Guid = string;

export type TaskListMemberRole = 'Contributor';

export type TaskListSortField = 'CreatedAt' | 'UpdatedAt' | 'Title';

export type QuerySortDirection = 'Ascending' | 'Descending';

export interface CreateTaskListRequest {
  title: string;
}

export interface UpdateTaskListRequest {
  title: string;
}

export interface AddTaskListMemberRequest {
  memberUserId: Guid;
  role: TaskListMemberRole;
}

export interface CreateTaskItemRequest {
  title: string;
  description?: string | null;
}

export interface UpdateTaskItemRequest {
  title: string;
  description?: string | null;
}

export interface CreateUserRequest {
  displayName?: string | null;
}

export interface TaskListSummaryResponse {
  id: Guid;
  title: string;
  ownerUserId: Guid;
  createdAt: string;
  updatedAt: string;
}

export interface TaskListDetailsResponse {
  id: Guid;
  title: string;
  ownerUserId: Guid;
  createdAt: string;
  updatedAt: string;
}

export interface TaskListMemberResponse {
  id: Guid;
  taskListId: Guid;
  memberUserId: Guid;
  role: TaskListMemberRole;
  createdAt: string;
  createdByUserId: Guid;
}

export interface TaskItemResponse {
  id: Guid;
  taskListId: Guid;
  title: string;
  description?: string | null;
  isCompleted: boolean;
  createdByUserId: Guid;
  createdAt: string;
  updatedAt: string;
}

export interface UserResponse {
  id: Guid;
  displayName?: string | null;
  createdAt: string;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ErrorItemResponse {
  message: string;
  code?: string | null;
}

export interface ErrorResponse {
  errors: ErrorItemResponse[];
}

export interface GetTaskListsParams extends Record<string, string | number | boolean | null | undefined> {
  page?: number;
  pageSize?: number;
  sortBy?: TaskListSortField;
  sortDirection?: QuerySortDirection;
}

export interface GetTaskItemsParams extends Record<string, string | number | boolean | null | undefined> {
  page?: number;
  pageSize?: number;
}
