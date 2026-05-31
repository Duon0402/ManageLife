namespace App {
    export interface TodoTaskModel {
        id: string;
        title: string;
        description?: string;
        status: TodoStatus;
        priority: TodoPriority;
        startDate?: string;
        dueDate?: string;
        completedAt?: string;
        estimatedMinutes?: number;
        recurrence: RecurrenceType;
        recurrenceEndDate?: string;
        reminderAt?: string;
        isReminderSent: boolean;
        todoListId: string;
        todoListName?: string;
        parentTaskId?: string;
        subTasks: TodoTaskModel[];
    }
}
