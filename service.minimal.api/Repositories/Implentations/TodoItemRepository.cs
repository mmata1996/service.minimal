using service.minimal.api.Models;
using service.minimal.api.Repositories.Contracts;
using System.Linq.Expressions;

namespace service.minimal.api.Repositories.Implentations
{
    public class TodoItemRepository(List<TodoItem> todoItems) : ITodoItemRepository
    {
        public  Task<List<TodoItem>> GetAllAsync()
            => Task.FromResult(todoItems);

        public Task<TodoItem> GetByExpresionAsync(Expression<Func<TodoItem, bool>> expression)
            => Task.FromResult(todoItems.FirstOrDefault(expression.Compile()));

        public Task<TodoItem> AddNewEntityAsync(TodoItem entity)
        {
            todoItems.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<TodoItem> UpdateEntityAsync(TodoItem entity)
        {
            var existingEntity = todoItems.FirstOrDefault(x => x.Id == entity.Id);
            
            if (existingEntity != null)
                throw new Exception("Entity not found");

            existingEntity.Title = entity.Title;
            existingEntity.Description = entity.Description;
            existingEntity.IsComplete = entity.IsComplete;
            
            return Task.FromResult(existingEntity);
        }

        public Task<bool> DeleteEntityAsync(Expression<Func<TodoItem, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
