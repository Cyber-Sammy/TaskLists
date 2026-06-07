using Microsoft.Extensions.DependencyInjection;
using TaskLists.Application;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Models;
using TaskLists.Application.Models.Commands.TaskItems;
using TaskLists.Application.Models.Commands.TaskLists;
using TaskLists.Application.Models.Commands.Users;
using TaskLists.Application.Models.Queries;
using TaskLists.Application.Models.Queries.TaskItems;
using TaskLists.Application.Models.Queries.TaskLists;
using TaskLists.Application.Models.Queries.Users;
using TaskLists.Domain.Constants;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Tests;

public sealed class ApplicationFlowTests
{
    [Fact]
    public async Task CreateTaskListAsync_CreatesOwnedTaskList()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.CreateAsync(
            new CreateTaskListCommand(app.State.OwnerUserId, "  New list  "),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Equal("New list", result.Value.Title);
        Assert.Equal(app.State.OwnerUserId, result.Value.OwnerUserId);
        Assert.Contains(app.State.TaskLists, taskList => taskList.Id == result.Value.Id);
    }

    [Fact]
    public async Task CreateTaskListAsync_RejectsEmptyTitle()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.CreateAsync(
            new CreateTaskListCommand(app.State.OwnerUserId, "   "),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task GetTaskListByIdAsync_AllowsOwnerAndMember()
    {
        var app = TestApp.Create();

        var ownerResult = await app.TaskLists.GetByIdAsync(
            new GetTaskListByIdQuery(app.State.OwnerUserId, app.State.TaskListId),
            CancellationToken.None);
        var memberResult = await app.TaskLists.GetByIdAsync(
            new GetTaskListByIdQuery(app.State.MemberUserId, app.State.TaskListId),
            CancellationToken.None);

        Assert.True(ownerResult.Successful);
        Assert.True(memberResult.Successful);
    }

    [Fact]
    public async Task GetTaskListByIdAsync_HidesTaskListFromStranger()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.GetByIdAsync(
            new GetTaskListByIdQuery(app.State.StrangerUserId, app.State.TaskListId),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetTaskListsAsync_ReturnsOnlyVisibleListsSortedByCreatedAtDescending()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.GetPagedAsync(
            new GetTaskListsQuery(
                app.State.MemberUserId,
                Page: 1,
                PageSize: 20,
                SortBy: TaskListSortField.CreatedAt,
                SortDirection: QuerySortDirection.Descending),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(app.State.MemberOwnedTaskListId, result.Value.Items.First().Id);
        Assert.DoesNotContain(result.Value.Items, taskList => taskList.Id == app.State.HiddenTaskListId);
    }

    [Fact]
    public async Task GetTaskListsAsync_RejectsInvalidPagination()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.GetPagedAsync(
            new GetTaskListsQuery(app.State.OwnerUserId, Page: 0, PageSize: 20),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task UpdateTaskListAsync_AllowsMember()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.UpdateAsync(
            new UpdateTaskListCommand(app.State.MemberUserId, app.State.TaskListId, "Updated title"),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Equal("Updated title", result.Value.Title);
        Assert.Equal("Updated title", app.State.TaskLists.Single(taskList => taskList.Id == app.State.TaskListId).Title);
    }

    [Fact]
    public async Task UpdateTaskListAsync_RejectsTooLongTitle()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.UpdateAsync(
            new UpdateTaskListCommand(
                app.State.MemberUserId,
                app.State.TaskListId,
                new string('a', TaskListRules.MaxTitleLength + 1)),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task DeleteTaskListAsync_AllowsOnlyOwnerAndDeletesRelatedData()
    {
        var app = TestApp.Create();

        var memberResult = await app.TaskLists.DeleteAsync(
            new DeleteTaskListCommand(app.State.MemberUserId, app.State.TaskListId),
            CancellationToken.None);
        var ownerResult = await app.TaskLists.DeleteAsync(
            new DeleteTaskListCommand(app.State.OwnerUserId, app.State.TaskListId),
            CancellationToken.None);

        Assert.False(memberResult.Successful);
        Assert.Equal(Result.ResultStatus.Forbidden, memberResult.Status);
        Assert.True(ownerResult.Successful);
        Assert.DoesNotContain(app.State.TaskLists, taskList => taskList.Id == app.State.TaskListId);
        Assert.DoesNotContain(app.State.TaskListMembers, member => member.TaskListId == app.State.TaskListId);
        Assert.DoesNotContain(app.State.TaskItems, taskItem => taskItem.TaskListId == app.State.TaskListId);
    }

    [Fact]
    public async Task AddMemberAsync_AllowsExistingMemberToShareList()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.AddMemberAsync(
            new AddTaskListMemberCommand(
                app.State.MemberUserId,
                app.State.TaskListId,
                app.State.StrangerUserId,
                TaskListMemberRole.Contributor),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Contains(app.State.TaskListMembers, member =>
            member.TaskListId == app.State.TaskListId &&
            member.MemberUserId == app.State.StrangerUserId);
    }

    [Fact]
    public async Task AddMemberAsync_RejectsDuplicateMember()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.AddMemberAsync(
            new AddTaskListMemberCommand(
                app.State.OwnerUserId,
                app.State.TaskListId,
                app.State.MemberUserId,
                TaskListMemberRole.Contributor),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task AddMemberAsync_RejectsOwnerAsMember()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.AddMemberAsync(
            new AddTaskListMemberCommand(
                app.State.OwnerUserId,
                app.State.TaskListId,
                app.State.OwnerUserId,
                TaskListMemberRole.Contributor),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task GetMembersAsync_ReturnsMembersForVisibleTaskList()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.GetMembersAsync(
            new GetTaskListMembersQuery(app.State.MemberUserId, app.State.TaskListId),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Contains(result.Value, member => member.MemberUserId == app.State.MemberUserId);
        Assert.Contains(result.Value, member => member.MemberUserId == app.State.TargetUserId);
    }

    [Fact]
    public async Task RemoveMemberAsync_AllowsMemberButRejectsOwnerRemoval()
    {
        var app = TestApp.Create();

        var removeMemberResult = await app.TaskLists.RemoveMemberAsync(
            new RemoveTaskListMemberCommand(app.State.MemberUserId, app.State.TaskListId, app.State.TargetUserId),
            CancellationToken.None);
        var removeOwnerResult = await app.TaskLists.RemoveMemberAsync(
            new RemoveTaskListMemberCommand(app.State.MemberUserId, app.State.TaskListId, app.State.OwnerUserId),
            CancellationToken.None);

        Assert.True(removeMemberResult.Successful);
        Assert.DoesNotContain(app.State.TaskListMembers, member =>
            member.TaskListId == app.State.TaskListId &&
            member.MemberUserId == app.State.TargetUserId);
        Assert.False(removeOwnerResult.Successful);
        Assert.Equal(Result.ResultStatus.Conflict, removeOwnerResult.Status);
    }

    [Fact]
    public async Task RemoveMemberAsync_ReturnsNotFoundWhenTargetIsNotMember()
    {
        var app = TestApp.Create();

        var result = await app.TaskLists.RemoveMemberAsync(
            new RemoveTaskListMemberCommand(app.State.MemberUserId, app.State.TaskListId, app.State.StrangerUserId),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateTaskItemAsync_CreatesTaskInVisibleList()
    {
        var app = TestApp.Create();

        var result = await app.TaskItems.CreateAsync(
            new CreateTaskItemCommand(app.State.MemberUserId, app.State.TaskListId, "  New task  ", "  Details  "),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Equal("New task", result.Value.Title);
        Assert.Equal("Details", result.Value.Description);
        Assert.Contains(app.State.TaskItems, taskItem => taskItem.Id == result.Value.Id);
    }

    [Fact]
    public async Task CreateTaskItemAsync_RejectsStranger()
    {
        var app = TestApp.Create();

        var result = await app.TaskItems.CreateAsync(
            new CreateTaskItemCommand(app.State.StrangerUserId, app.State.TaskListId, "Task", null),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateTaskItemAsync_RejectsTooLongDescription()
    {
        var app = TestApp.Create();

        var result = await app.TaskItems.CreateAsync(
            new CreateTaskItemCommand(
                app.State.MemberUserId,
                app.State.TaskListId,
                "Task",
                new string('a', TaskItemRules.MaxDescriptionLength + 1)),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task GetTaskItemsAsync_ReturnsPagedItemsForVisibleList()
    {
        var app = TestApp.Create();

        var result = await app.TaskItems.GetPagedByTaskListAsync(
            new GetTaskItemsQuery(app.State.MemberUserId, app.State.TaskListId, Page: 1, PageSize: 20),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.All(result.Value.Items, taskItem => Assert.Equal(app.State.TaskListId, taskItem.TaskListId));
    }

    [Fact]
    public async Task GetTaskItemsAsync_RejectsInvalidPagination()
    {
        var app = TestApp.Create();

        var result = await app.TaskItems.GetPagedByTaskListAsync(
            new GetTaskItemsQuery(app.State.MemberUserId, app.State.TaskListId, Page: 1, PageSize: 101),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task GetTaskItemByIdAsync_HidesTaskFromStranger()
    {
        var app = TestApp.Create();

        var result = await app.TaskItems.GetByIdAsync(
            new GetTaskItemByIdQuery(app.State.StrangerUserId, app.State.TaskItemId),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateTaskItemAsync_UpdatesTitleAndDescription()
    {
        var app = TestApp.Create();

        var result = await app.TaskItems.UpdateAsync(
            new UpdateTaskItemCommand(app.State.MemberUserId, app.State.TaskItemId, "Updated task", "Updated details"),
            CancellationToken.None);

        var storedTaskItem = app.State.TaskItems.Single(taskItem => taskItem.Id == app.State.TaskItemId);

        Assert.True(result.Successful);
        Assert.Equal("Updated task", storedTaskItem.Title);
        Assert.Equal("Updated details", storedTaskItem.Description);
    }

    [Fact]
    public async Task CompleteAndReopenTaskItemAsync_ChangesCompletionState()
    {
        var app = TestApp.Create();

        var completeResult = await app.TaskItems.CompleteAsync(
            new CompleteTaskItemCommand(app.State.MemberUserId, app.State.TaskItemId),
            CancellationToken.None);
        var reopenResult = await app.TaskItems.ReopenAsync(
            new ReopenTaskItemCommand(app.State.MemberUserId, app.State.TaskItemId),
            CancellationToken.None);

        Assert.True(completeResult.Successful);
        Assert.True(completeResult.Value.IsCompleted);
        Assert.True(reopenResult.Successful);
        Assert.False(reopenResult.Value.IsCompleted);
    }

    [Fact]
    public async Task DeleteTaskItemAsync_DeletesVisibleTask()
    {
        var app = TestApp.Create();

        var result = await app.TaskItems.DeleteAsync(
            new DeleteTaskItemCommand(app.State.MemberUserId, app.State.TaskItemId),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.DoesNotContain(app.State.TaskItems, taskItem => taskItem.Id == app.State.TaskItemId);
    }

    [Fact]
    public async Task CreateUserAsync_CreatesUser()
    {
        var app = TestApp.Create();

        var result = await app.Users.CreateAsync(
            new CreateUserCommand("  New user  "),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Equal("New user", result.Value.DisplayName);
        Assert.Contains(app.State.Users, user => user.Id == result.Value.Id);
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsAllUsers()
    {
        var app = TestApp.Create();

        var result = await app.Users.GetAllAsync(CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Contains(result.Value, user => user.Id == app.State.OwnerUserId);
        Assert.Contains(result.Value, user => user.Id == app.State.MemberUserId);
        Assert.Contains(result.Value, user => user.Id == app.State.TargetUserId);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsNotFoundForMissingUser()
    {
        var app = TestApp.Create();

        var result = await app.Users.GetByIdAsync(
            new GetUserByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.Successful);
        Assert.Equal(Result.ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsExistingUser()
    {
        var app = TestApp.Create();

        var result = await app.Users.GetByIdAsync(
            new GetUserByIdQuery(app.State.OwnerUserId),
            CancellationToken.None);

        Assert.True(result.Successful);
        Assert.Equal(app.State.OwnerUserId, result.Value.Id);
        Assert.Equal("Owner", result.Value.DisplayName);
    }

    private sealed class TestApp
    {
        private TestApp(
            TestState state,
            ITaskListService taskLists,
            ITaskItemService taskItems,
            IUserService users)
        {
            State = state;
            TaskLists = taskLists;
            TaskItems = taskItems;
            Users = users;
        }

        public TestState State { get; }
        public ITaskListService TaskLists { get; }
        public ITaskItemService TaskItems { get; }
        public IUserService Users { get; }

        public static TestApp Create()
        {
            var state = TestState.Create();
            var services = new ServiceCollection();

            services.AddTaskListsApplication();
            services.AddSingleton(state);
            services.AddScoped<ITaskListRepository, InMemoryTaskListRepository>();
            services.AddScoped<ITaskListMemberRepository, InMemoryTaskListMemberRepository>();
            services.AddScoped<ITaskItemRepository, InMemoryTaskItemRepository>();
            services.AddScoped<IUserRepository, InMemoryUserRepository>();

            var provider = services.BuildServiceProvider();

            return new TestApp(
                state,
                provider.GetRequiredService<ITaskListService>(),
                provider.GetRequiredService<ITaskItemService>(),
                provider.GetRequiredService<IUserService>());
        }
    }

    private sealed class TestState
    {
        private TestState()
        {
        }

        public Guid TaskListId { get; private init; }
        public Guid MemberOwnedTaskListId { get; private init; }
        public Guid HiddenTaskListId { get; private init; }
        public Guid TaskItemId { get; private init; }
        public Guid OwnerUserId { get; private init; }
        public Guid MemberUserId { get; private init; }
        public Guid TargetUserId { get; private init; }
        public Guid StrangerUserId { get; private init; }

        public List<TaskList> TaskLists { get; } = [];
        public List<TaskListMember> TaskListMembers { get; } = [];
        public List<TaskItem> TaskItems { get; } = [];
        public List<User> Users { get; } = [];

        public static TestState Create()
        {
            var now = DateTimeOffset.UtcNow;
            var ownerUserId = Guid.NewGuid();
            var memberUserId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var strangerUserId = Guid.NewGuid();
            var taskListId = Guid.NewGuid();
            var memberOwnedTaskListId = Guid.NewGuid();
            var hiddenTaskListId = Guid.NewGuid();
            var taskItemId = Guid.NewGuid();

            var state = new TestState
            {
                TaskListId = taskListId,
                MemberOwnedTaskListId = memberOwnedTaskListId,
                HiddenTaskListId = hiddenTaskListId,
                TaskItemId = taskItemId,
                OwnerUserId = ownerUserId,
                MemberUserId = memberUserId,
                TargetUserId = targetUserId,
                StrangerUserId = strangerUserId
            };

            state.Users.Add(User.Restore(ownerUserId, "Owner", now.AddMinutes(-10)));
            state.Users.Add(User.Restore(memberUserId, "Member", now.AddMinutes(-9)));
            state.Users.Add(User.Restore(targetUserId, "Target", now.AddMinutes(-8)));
            state.Users.Add(User.Restore(strangerUserId, "Stranger", now.AddMinutes(-7)));

            state.TaskLists.Add(TaskList.Restore(taskListId, "Shared list", ownerUserId, now.AddMinutes(-5), now.AddMinutes(-5)));
            state.TaskLists.Add(TaskList.Restore(memberOwnedTaskListId, "Member owned list", memberUserId, now.AddMinutes(-1), now.AddMinutes(-1)));
            state.TaskLists.Add(TaskList.Restore(hiddenTaskListId, "Hidden list", targetUserId, now, now));

            state.TaskListMembers.Add(TaskListMember.Restore(
                Guid.NewGuid(),
                taskListId,
                memberUserId,
                TaskListMemberRole.Contributor,
                now.AddMinutes(-4),
                ownerUserId));
            state.TaskListMembers.Add(TaskListMember.Restore(
                Guid.NewGuid(),
                taskListId,
                targetUserId,
                TaskListMemberRole.Contributor,
                now.AddMinutes(-3),
                ownerUserId));

            state.TaskItems.Add(TaskItem.Restore(
                taskItemId,
                taskListId,
                "First task",
                "First details",
                isCompleted: false,
                ownerUserId,
                now.AddMinutes(-2),
                now.AddMinutes(-2)));
            state.TaskItems.Add(TaskItem.Restore(
                Guid.NewGuid(),
                taskListId,
                "Second task",
                null,
                isCompleted: false,
                memberUserId,
                now.AddMinutes(-1),
                now.AddMinutes(-1)));
            state.TaskItems.Add(TaskItem.Restore(
                Guid.NewGuid(),
                hiddenTaskListId,
                "Hidden task",
                null,
                isCompleted: false,
                targetUserId,
                now,
                now));

            return state;
        }
    }

    private sealed class InMemoryTaskListRepository : ITaskListRepository
    {
        private readonly TestState _state;

        public InMemoryTaskListRepository(TestState state)
        {
            _state = state;
        }

        public Task<Result<Guid>> AddAsync(TaskList taskList, CancellationToken cancellationToken)
        {
            _state.TaskLists.Add(taskList);

            return Task.FromResult(Result<Guid>.Success(taskList.Id));
        }

        public Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var deletedCount = _state.TaskLists.RemoveAll(taskList => taskList.Id == id);

            return Task.FromResult(deletedCount == 0
                ? Result<Guid>.NotFound("Task list was not found.")
                : Result<Guid>.Success(id));
        }

        public Task<Result<TaskList?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<TaskList?>.Success(_state.TaskLists.FirstOrDefault(taskList => taskList.Id == id)));
        }

        public Task<Result<PagedResponse<TaskList>>> GetVisibleToUserAsync(
            Guid userId,
            int page,
            int pageSize,
            TaskListSortField sortBy,
            QuerySortDirection sortDirection,
            CancellationToken cancellationToken)
        {
            var visibleTaskLists = _state.TaskLists
                .Where(taskList => CanAccessTaskList(taskList.Id, userId))
                .ToArray();

            var sortedTaskLists = Sort(visibleTaskLists, sortBy, sortDirection).ToArray();
            var pagedTaskLists = sortedTaskLists
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            return Task.FromResult(Result<PagedResponse<TaskList>>.Success(new PagedResponse<TaskList>(
                pagedTaskLists,
                page,
                pageSize,
                sortedTaskLists.Length)));
        }

        public Task<Result<TaskList>> GetVisibleToUserByIdAsync(Guid taskListId, Guid userId, CancellationToken cancellationToken)
        {
            var taskList = _state.TaskLists.FirstOrDefault(storedTaskList => storedTaskList.Id == taskListId);

            if (taskList is null || !CanAccessTaskList(taskListId, userId))
            {
                return Task.FromResult(Result<TaskList>.NotFound("Task list was not found."));
            }

            return Task.FromResult(Result<TaskList>.Success(taskList));
        }

        public Task<Result<bool>> IsTaskListExistsAsync(Guid taskListId, CancellationToken cancellation)
        {
            return Task.FromResult(Result<bool>.Success(_state.TaskLists.Any(taskList => taskList.Id == taskListId)));
        }

        public Task<Result<bool>> IsTaskListExistsForUserAsync(Guid taskListId, Guid userId, CancellationToken cancellation)
        {
            return Task.FromResult(Result<bool>.Success(CanAccessTaskList(taskListId, userId)));
        }

        public Task<Result<Guid>> UpdateAsync(TaskList taskList, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<Guid>.Success(taskList.Id));
        }

        private bool CanAccessTaskList(Guid taskListId, Guid userId)
        {
            return _state.TaskLists.Any(taskList => taskList.Id == taskListId && taskList.OwnerUserId == userId) ||
                _state.TaskListMembers.Any(member => member.TaskListId == taskListId && member.MemberUserId == userId);
        }

        private static IEnumerable<TaskList> Sort(
            IEnumerable<TaskList> taskLists,
            TaskListSortField sortBy,
            QuerySortDirection sortDirection)
        {
            return (sortBy, sortDirection) switch
            {
                (TaskListSortField.Title, QuerySortDirection.Ascending) => taskLists.OrderBy(taskList => taskList.Title),
                (TaskListSortField.Title, QuerySortDirection.Descending) => taskLists.OrderByDescending(taskList => taskList.Title),
                (TaskListSortField.UpdatedAt, QuerySortDirection.Ascending) => taskLists.OrderBy(taskList => taskList.UpdatedAt),
                (TaskListSortField.UpdatedAt, QuerySortDirection.Descending) => taskLists.OrderByDescending(taskList => taskList.UpdatedAt),
                (TaskListSortField.CreatedAt, QuerySortDirection.Ascending) => taskLists.OrderBy(taskList => taskList.CreatedAt),
                _ => taskLists.OrderByDescending(taskList => taskList.CreatedAt)
            };
        }
    }

    private sealed class InMemoryTaskListMemberRepository : ITaskListMemberRepository
    {
        private readonly TestState _state;

        public InMemoryTaskListMemberRepository(TestState state)
        {
            _state = state;
        }

        public Task<Result<Guid>> AddAsync(TaskListMember member, CancellationToken cancellationToken)
        {
            if (_state.TaskListMembers.Any(storedMember =>
                storedMember.TaskListId == member.TaskListId &&
                storedMember.MemberUserId == member.MemberUserId))
            {
                return Task.FromResult(Result<Guid>.Conflict("Task list member already exists."));
            }

            _state.TaskListMembers.Add(member);

            return Task.FromResult(Result<Guid>.Success(member.Id));
        }

        public Task<Result<Guid>> DeleteAsync(Guid taskListId, Guid memberUserId, CancellationToken cancellationToken)
        {
            var deletedCount = _state.TaskListMembers.RemoveAll(member =>
                member.TaskListId == taskListId &&
                member.MemberUserId == memberUserId);

            return Task.FromResult(deletedCount == 0
                ? Result<Guid>.NotFound("Task list member was not found.")
                : Result<Guid>.Success(memberUserId));
        }

        public Task<Result<long>> DeleteByTaskListIdAsync(Guid taskListId, CancellationToken cancellationToken)
        {
            var deletedCount = _state.TaskListMembers.RemoveAll(member => member.TaskListId == taskListId);

            return Task.FromResult(Result<long>.Success(deletedCount));
        }

        public Task<Result<bool>> ExistsAsync(Guid taskListId, Guid memberUserId, CancellationToken cancellationToken)
        {
            var exists = _state.TaskListMembers.Any(member => member.TaskListId == taskListId && member.MemberUserId == memberUserId);

            return Task.FromResult(Result<bool>.Success(exists));
        }

        public Task<Result<IReadOnlyCollection<TaskListMember>>> GetByTaskListIdAsync(Guid taskListId, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<TaskListMember> members = _state.TaskListMembers
                .Where(member => member.TaskListId == taskListId)
                .ToArray();

            return Task.FromResult(Result<IReadOnlyCollection<TaskListMember>>.Success(members));
        }

        public Task<Result<TaskListMember?>> GetByTaskListAndUserAsync(Guid taskListId, Guid memberUserId, CancellationToken cancellationToken)
        {
            var member = _state.TaskListMembers.FirstOrDefault(storedMember =>
                storedMember.TaskListId == taskListId &&
                storedMember.MemberUserId == memberUserId);

            return Task.FromResult(Result<TaskListMember?>.Success(member));
        }
    }

    private sealed class InMemoryTaskItemRepository : ITaskItemRepository
    {
        private readonly TestState _state;

        public InMemoryTaskItemRepository(TestState state)
        {
            _state = state;
        }

        public Task<Result<Guid>> AddAsync(TaskItem taskItem, CancellationToken cancellationToken)
        {
            _state.TaskItems.Add(taskItem);

            return Task.FromResult(Result<Guid>.Success(taskItem.Id));
        }

        public Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var deletedCount = _state.TaskItems.RemoveAll(taskItem => taskItem.Id == id);

            return Task.FromResult(deletedCount == 0
                ? Result<Guid>.NotFound("Task item was not found.")
                : Result<Guid>.Success(id));
        }

        public Task<Result<long>> DeleteByTaskListIdAsync(Guid taskListId, CancellationToken cancellationToken)
        {
            var deletedCount = _state.TaskItems.RemoveAll(taskItem => taskItem.TaskListId == taskListId);

            return Task.FromResult(Result<long>.Success(deletedCount));
        }

        public Task<Result<PagedResponse<TaskItem>>> GetByTaskListIdAsync(Guid taskListId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var taskItems = _state.TaskItems
                .Where(taskItem => taskItem.TaskListId == taskListId)
                .OrderByDescending(taskItem => taskItem.CreatedAt)
                .ToArray();
            var pagedItems = taskItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            return Task.FromResult(Result<PagedResponse<TaskItem>>.Success(new PagedResponse<TaskItem>(
                pagedItems,
                page,
                pageSize,
                taskItems.Length)));
        }

        public Task<Result<TaskItem?>> GetByIdVisibleToUserAsync(Guid id, Guid userId, CancellationToken cancellationToken)
        {
            var taskItem = _state.TaskItems.FirstOrDefault(storedTaskItem => storedTaskItem.Id == id);

            if (taskItem is null || !CanAccessTaskList(taskItem.TaskListId, userId))
            {
                return Task.FromResult(Result<TaskItem?>.Success(null));
            }

            return Task.FromResult(Result<TaskItem?>.Success(taskItem));
        }

        public Task<Result<Guid>> UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<Guid>.Success(taskItem.Id));
        }

        private bool CanAccessTaskList(Guid taskListId, Guid userId)
        {
            return _state.TaskLists.Any(taskList => taskList.Id == taskListId && taskList.OwnerUserId == userId) ||
                _state.TaskListMembers.Any(member => member.TaskListId == taskListId && member.MemberUserId == userId);
        }
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly TestState _state;

        public InMemoryUserRepository(TestState state)
        {
            _state = state;
        }

        public Task<Result<Guid>> AddAsync(User user, CancellationToken cancellationToken)
        {
            _state.Users.Add(user);

            return Task.FromResult(Result<Guid>.Success(user.Id));
        }

        public Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<bool>.Success(_state.Users.Any(user => user.Id == id)));
        }

        public Task<Result<IReadOnlyCollection<User>>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<User> users = _state.Users.ToArray();

            return Task.FromResult(Result<IReadOnlyCollection<User>>.Success(users));
        }

        public Task<Result<User?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<User?>.Success(_state.Users.FirstOrDefault(user => user.Id == id)));
        }
    }
}
