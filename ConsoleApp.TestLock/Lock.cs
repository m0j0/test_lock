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

        public Task<long> GetServiceResultAsync()
        {
            async Task<long> GetTaskAsync()
            {
                // to execute lock scope very fast
                await Task.Yield();

                return await GetServiceResultInternalAsync();
            }

            lock (_lock)
            {
                if (_task == null ||
                    _task.IsCompleted)
                {
                    _task = GetTaskAsync();
                    _task.ContinueWith(task =>
                    {
                        lock (_lock)
                        {
                            _task = null;
                        }
                    });
                }

                return _task;
            }
        }

        private async Task<long> GetServiceResultInternalAsync()
        {
            await Task.Delay(1000);
            return 1;
        }
    }
}
