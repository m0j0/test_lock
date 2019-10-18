using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp.TestLock
{
    public class Interlock
    {
        private Lazy<Task<long>> _task;

        public Task<long> GetServiceResultAsync()
        {
            return TaskCacheHelper.GetOrCreateTask(ref _task, GetServiceResultInternalAsync, NullifyStartUpgradeTask);
        }

        private void NullifyStartUpgradeTask(Lazy<Task<long>> lazy)
        {
            Interlocked.CompareExchange(ref _task, null, lazy);
        }

        private async Task<long> GetServiceResultInternalAsync()
        {
            await Task.Delay(1000);
            return 1;
        }
    }

    public static class TaskCacheHelper
    {
        public static Task<T> GetOrCreateTask<T>(ref Lazy<Task<T>> field, Func<Task<T>> getTask, Action<Lazy<Task<T>>> nullifyField)
        {
            var oldField = field;
            if (oldField != null && !oldField.Value.IsCompleted)
            {
                return oldField.Value;
            }

            // use Lazy to prevent task from executing 2 times 
            var newValue = new Lazy<Task<T>>(getTask);
            var originalFieldValue = Interlocked.CompareExchange(ref field, newValue, oldField);

            if (originalFieldValue == oldField)
            {
                // external delegate used here because ref parameter can't be used inside lambda
                newValue.Value.ContinueWith(task => nullifyField(newValue));

                return newValue.Value;
            }

            return originalFieldValue.Value;
        }
    }
}
