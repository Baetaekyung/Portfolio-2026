using System;
using System.Collections.Generic;

namespace ProjectQ.Pool
{
    /*
    제네릭 오브젝트 풀링 시스템
    LRU(Least Recently Used) 방식으로 오브젝트 관리
    */
    public class ObjectPool<T> where T : class
    {
        // 풀링된 오브젝트 정보
        private class PooledItem
        {
            public T Object;
            public float LastUsedTime;
            public bool IsActive;
        }

        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly PoolConfig _config;

        // 모든 생성된 오브젝트 추적
        private readonly List<PooledItem> _allItems = new List<PooledItem>();

        // 빠른 검색을 위한 딕셔너리
        private readonly Dictionary<T, PooledItem> _itemLookup = new Dictionary<T, PooledItem>();

        // 대기 중인 오브젝트 (LRU 순서 유지)
        private readonly LinkedList<PooledItem> _availableItems = new LinkedList<PooledItem>();

        // 시간 제공자 (테스트 용이성)
        private Func<float> _timeProvider;

        public int ActiveCount { get; private set; }
        public int PooledCount => _availableItems.Count;
        public int TotalCount => _allItems.Count;

        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            PoolConfig config = null)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _config = config ?? new PoolConfig();

            // 기본 시간 제공자 (Unity 환경에서는 Time.time 사용 권장)
            _timeProvider = () => (float)DateTime.Now.TimeOfDay.TotalSeconds;

            // 초기 오브젝트 생성
            Warmup(_config.InitialSize);
        }

        // 시간 제공자 설정 (Unity에서 Time.time 사용)
        public void SetTimeProvider(Func<float> timeProvider)
        {
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        // 풀에서 오브젝트 가져오기
        public T Get()
        {
            PooledItem item;

            if (_availableItems.Count > 0)
            {
                // 가장 최근에 사용된 것부터 가져옴 (캐시 효율)
                item = _availableItems.Last.Value;
                _availableItems.RemoveLast();
            }
            else
            {
                // 새로 생성
                item = CreateItem();
            }

            item.IsActive = true;
            item.LastUsedTime = _timeProvider();
            ActiveCount++;

            _onGet?.Invoke(item.Object);

            return item.Object;
        }

        // 오브젝트를 풀에 반환
        public void Release(T obj)
        {
            if (obj == null)
                return;

            if (!_itemLookup.TryGetValue(obj, out var item))
            {
                // 풀에서 관리하지 않는 오브젝트
                return;
            }

            if (!item.IsActive)
            {
                // 이미 반환됨
                return;
            }

            item.IsActive = false;
            item.LastUsedTime = _timeProvider();
            ActiveCount--;

            _onRelease?.Invoke(obj);

            // LRU: 가장 최근에 반환된 것을 뒤에 추가
            _availableItems.AddLast(item);

            // 최대 크기 초과 시 정리
            TrimExcess();
        }

        // 자동 반환 핸들 가져오기
        public AutoReleaseHandle GetAutoRelease()
        {
            return new AutoReleaseHandle(this, Get());
        }

        // LRU 기반 정리
        public void Shrink()
        {
            var currentTime = _timeProvider();
            var toRemove = new List<PooledItem>();

            // 만료된 오브젝트 찾기
            foreach (var item in _availableItems)
            {
                if (currentTime - item.LastUsedTime > _config.ExpireTime)
                {
                    toRemove.Add(item);
                }
            }

            // InitialSize 이하로는 줄이지 않음
            var targetRemoveCount = Math.Min(
                toRemove.Count,
                _allItems.Count - _config.InitialSize
            );

            // LRU 순서로 정렬 (오래된 것 먼저)
            toRemove.Sort((a, b) => a.LastUsedTime.CompareTo(b.LastUsedTime));

            for (int i = 0; i < targetRemoveCount; i++)
            {
                DestroyItem(toRemove[i]);
            }
        }

        // 미리 오브젝트 생성
        public void Warmup(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_allItems.Count >= _config.MaxSize)
                    break;

                var item = CreateItem();
                item.LastUsedTime = _timeProvider();
                _availableItems.AddLast(item);
            }
        }

        // 모든 오브젝트 정리
        public void Clear()
        {
            foreach (var item in _allItems.ToArray())
            {
                DestroyItem(item);
            }

            _allItems.Clear();
            _itemLookup.Clear();
            _availableItems.Clear();
            ActiveCount = 0;
        }

        // 새 아이템 생성
        private PooledItem CreateItem()
        {
            var obj = _createFunc();
            var item = new PooledItem
            {
                Object = obj,
                LastUsedTime = _timeProvider(),
                IsActive = false
            };

            _allItems.Add(item);
            _itemLookup[obj] = item;

            return item;
        }

        // 아이템 제거
        private void DestroyItem(PooledItem item)
        {
            _onDestroy?.Invoke(item.Object);
            _availableItems.Remove(item);
            _allItems.Remove(item);
            _itemLookup.Remove(item.Object);
        }

        // 최대 크기 초과 시 정리
        private void TrimExcess()
        {
            while (_allItems.Count > _config.MaxSize && _availableItems.Count > 0)
            {
                // LRU: 가장 오래된 것 제거 (맨 앞)
                var oldest = _availableItems.First.Value;
                DestroyItem(oldest);
            }
        }

        // 자동 반환 핸들
        public struct AutoReleaseHandle : IDisposable
        {
            private readonly ObjectPool<T> _pool;
            private T _object;
            private bool _disposed;

            public T Object => _object;

            public AutoReleaseHandle(ObjectPool<T> pool, T obj)
            {
                _pool = pool;
                _object = obj;
                _disposed = false;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _pool.Release(_object);
                    _object = null;
                    _disposed = true;
                }
            }
        }
    }

    // 풀 설정
    [Serializable]
    public class PoolConfig
    {
        public int InitialSize = 10;
        public int MaxSize = 100;
        public float AutoShrinkTime = 30f;
        public float ExpireTime = 60f;
    }
}
