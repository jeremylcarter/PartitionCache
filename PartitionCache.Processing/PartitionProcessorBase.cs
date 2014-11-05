using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PartitionCache.Processing.Exceptions;

namespace PartitionCache.Processing
{
    public class PartitionProcessorBase<TQueueItem, TProperty>
    {

        public static ConcurrentUniqueQueue<TQueueItem, TProperty> Queue;
        public bool IsRunning { get; set; }
        public int Partition { get; set; }
        public Expression<Func<TQueueItem, TProperty>> Expression;

        private static CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private CancellationToken _token = _tokenSource.Token;

        public PartitionProcessorBase()
        {
            Queue = new ConcurrentUniqueQueue<TQueueItem, TProperty>();
        }
        public PartitionProcessorBase(int partition, Expression<Func<TQueueItem, TProperty>> exp)
        {
            Expression = exp;
            Partition = partition;
            Queue = new ConcurrentUniqueQueue<TQueueItem, TProperty>();
        }

        public int QueueCount
        {
            get { return Queue.Count(); }
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            IsRunning = true;
            ItemAdded += PartitionProcessorBase_ItemAdded;
        }

        private void PartitionProcessorBase_ItemAdded(object sender, EventArgs e)
        {
            try
            {
                TQueueItem item = default(TQueueItem);
                Queue.TryDequeue(out item);
                if (item != null && !_token.IsCancellationRequested)
                {
                    Task.Run(()=> ProcessItem(item));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            IsRunning = false;
            if (ItemAdded != null)
            {
                ItemAdded -= PartitionProcessorBase_ItemAdded;
            }
        }

        public virtual void ProcessItem(TQueueItem item)
        {
            
        }

        public void ClearItems()
        {
            if (IsRunning == false)
            {
                Queue.ClearAll();
                Queue = null;
                Queue = new ConcurrentUniqueQueue<TQueueItem, TProperty>();
            }

        }

        public void AddItem(TQueueItem item)
        {
            if (_token.IsCancellationRequested | !IsRunning) throw new ProcessorIsNotRunningException();

            var key = GetIdentifier(item, Expression);

            if (!Queue.KeyExists(key))
            {
                Queue.AddKey(key);
                Queue.Enqueue(item);
                OnItemAdded();

            }

        }

        public event EventHandler ItemAdded;
        protected virtual void OnItemAdded()
        {
            if (ItemAdded != null)
            {
                ItemAdded(this, EventArgs.Empty);
            }
        }

        public bool RemoveKey(TProperty key)
        {
            Queue.RemoveKey(key);
            return true;
        }
        public TProperty GetIdentifier(TQueueItem item, Expression<Func<TQueueItem, TProperty>> exp)
        {
            try
            {
                MemberExpression body = (MemberExpression)exp.Body;

                string propertyName = body.Member.Name;
                TProperty value = (TProperty)typeof(TQueueItem).GetProperty(propertyName).GetValue(item, null);
                return value;
            }
            catch (Exception)
            {
                return default(TProperty);
            }
        }
    }
}
