using System;
using System.Collections.Generic;

namespace Common
{
    public class ObjectPoolWrapper<T>
    {
        private ObjectPool<T> _parent;
        private T _value;

        public T Value
        {
            get
            {
                return _value;
            }
            protected set
            {
                _value = value;
            }
        }

        public ObjectPoolWrapper(ObjectPool<T> pool, T value)
        {
            _parent = pool;
            _value = value;
        }
        public ObjectPoolWrapper(ObjectPool<T> pool)
        {
            _parent = pool;
        }

        public void Release()
        {
            _parent.Release(this);
        }
    }

    public class ObjectPool<T> : IDisposable
    {
        private class ObjectPoolException : ApplicationException
        {
            public ObjectPoolException() : base() { }
            public ObjectPoolException(string message) : base(message) { }
            public ObjectPoolException(string message, Exception innerException) : base(message, innerException) { }
        }

        public delegate T CreateFunc();
        public delegate void ActionFunc(T value);
        public delegate void ActionFuncWrapper(ObjectPoolWrapper<T> value);

        public CreateFunc OnCreate
        {
            get
            {
                return _onCreate;
            }
            set
            {
                _onCreate = value;
                if (objects == null)
                {
                    objects = new Stack<ObjectPoolWrapper<T>>();
                    CreateObjects(InitializeCreateCount);
                }
            }
        }
        private CreateFunc _onCreate;

        public ActionFunc OnTake;
        public ActionFuncWrapper OnTakeWrapper;
        public ActionFunc OnRelease;
        public ActionFunc OnObjectDestroy;
        public System.Action OnPoolDestroy;

        public int InitializeCreateCount = 10;
        public int ExpandCreateCount = 10;
        private int _createCount = 0;
        public int TotalCreated { get { return _createCount; } }

        public Stack<ObjectPoolWrapper<T>> Objects
        {
            get
            {
                return objects;
            }
        }

        private Stack<ObjectPoolWrapper<T>> objects;

        #region LifeCycle
        public ObjectPool()
        {
        }
        public ObjectPool(T[] array)
        {
            objects = new Stack<ObjectPoolWrapper<T>>();
            InitializeCreateCount = array.Length;
            for (int i = 0; i < InitializeCreateCount; i++)
            {
                objects.Push(new ObjectPoolWrapper<T>(this, array[i]));
            }
        }
        public ObjectPool(CreateFunc func)
        {
            OnCreate = func;
        }
        public ObjectPool(CreateFunc func, int initCount)
        {
            InitializeCreateCount = initCount;
            OnCreate = func;
        }
        #endregion

        #region clear
        public void Reset()
        {
            int len = objects.Count;
            int i = _createCount - len; 
            i = Math.Max(i, InitializeCreateCount);

            if (OnObjectDestroy != null)
            {
                for (; i <= len; i++)
                {
                    OnObjectDestroy(objects.Pop().Value);
                }
            }
            else
            {
                for (; i <= len; i++)
                {
                    objects.Pop();
                }
            }
        }

        public void Dispose()
        {
            if (OnObjectDestroy != null)
            {
                var e = objects.GetEnumerator();
                while (e.MoveNext())
                {
                    OnObjectDestroy(e.Current.Value);
                }
            }

            if (OnPoolDestroy != null)
                OnPoolDestroy();
        }
        #endregion


        #region creation
        private void CreateObjects(int count)
        {
            if (OnCreate == null)
                throw new ObjectPoolException("Not Support Create More Objects");

            _createCount += count;

            for (int i = 0; i < count; i++)
            {
                objects.Push(_creation());
            }
        }
        private ObjectPoolWrapper<T> _creation()
        {
            T temp = OnCreate();

            if (temp is ObjectPoolWrapper<T>)
            {
                return temp as ObjectPoolWrapper<T>;
            }
            else
            {
                return new ObjectPoolWrapper<T>(this, temp);
            }
        }
        #endregion

        #region Pooling
        public ObjectPoolWrapper<T> Take()
        {
            ObjectPoolWrapper<T> result;
            if (objects == null)
            {
                throw new ObjectPoolException("Not Initialize Pool. Please assign OnCreate Function");
            }
            else if (objects.Count == 0)
            {
                CreateObjects(ExpandCreateCount - 1);
                result = _creation();
            }
            else
            {
                result = objects.Pop();
            }

            if (OnTake != null)
                OnTake(result.Value);

            if (OnTakeWrapper != null)
                OnTakeWrapper(result);

            return result;
        }
        public void Release(ObjectPoolWrapper<T> obj)
        {
            if (OnRelease != null)
                OnRelease(obj.Value);

            objects.Push(obj);
        }
        #endregion

    }
}
