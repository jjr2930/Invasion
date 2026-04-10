using UnityEngine.Pool;

namespace Jy
{
    public class CustomPoolBase<PooledItemT>
        where PooledItemT : class, new()
    {
        protected virtual int InitCapacity { get; } = 128;
        protected virtual int MaxCapacity { get; } = 256;

        protected ObjectPool<PooledItemT> Pool;
        protected CustomPoolBase()
        {
            Pool = new ObjectPool<PooledItemT>(
                CreateOne,
                OnGet,
                OnRelease,
                OnDestroy,
                true,
                InitCapacity,
                MaxCapacity
            );
        }

        protected virtual PooledItemT CreateOne()
        {
            return new PooledItemT();
        }
        protected virtual void OnRelease(PooledItemT t)
        {
            Pool.Release(t);
        }

        public PooledItemT Get()
        {
            return Pool.Get();
        }

        public void Release(PooledItemT old)
        {
            Pool.Release(old);
        }

        protected virtual void OnGet(PooledItemT t) { }

        protected virtual void OnDestroy(PooledItemT t) { }
    }
}
