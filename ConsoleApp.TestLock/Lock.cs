using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.TestLock
{
    public class Lock
    {
        private readonly object _lock = new object();
        private Task<long> _task;
        private readonly Action<Task<long>> _nullifyContinuation;

        public Lock()
        {
            _nullifyContinuation = Nullify;
        }

        public Task<long> GetServiceResultAsync()
        {
            async Task<long> GetTaskAsync()
            {
                // to execute lock scope very fast
                await Task.Yield();

                return await GetServiceResultInternalAsync().ConfigureAwait(false);
            }

            var task = _task;
            if (task != null &&
                !task.IsCompleted)
            {
                return task;
            }

            lock (_lock)
            {
                if (_task == null ||
                    _task.IsCompleted)
                {
                    _task = GetTaskAsync();
                    _task.ContinueWith(_nullifyContinuation);
                }

                return _task;
            }
        }

        private void Nullify(Task<long> task)
        {
            lock (_lock)
            {
                _task = null;
            }
        }

        private async Task<long> GetServiceResultInternalAsync()
        {
            await Task.Delay(100_000).ConfigureAwait(false);
            return 1;
        }
    }
}