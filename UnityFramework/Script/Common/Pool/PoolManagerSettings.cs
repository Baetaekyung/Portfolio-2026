using System;
using System.Collections.Generic;

/*
풀 매니저 설정 클래스들
카테고리별 독립적인 설정 지원
*/

// 카테고리별 풀 설정
[Serializable]
public class CategoryPoolSettings
{
    public bool autoShrink = true;
    public float shrinkCheckInterval = 10f;
    public List<GameObjectPoolConfig> preloadPools = new List<GameObjectPoolConfig>();
}

// 풀 통계 정보
public class PoolManagerStats
{
    public (int active, int pooled, int total) uiStats;
    public (int active, int pooled, int total) effectStats;
    public (int active, int pooled, int total) inGameObjectStats;
    public (int active, int pooled, int total) audioStats;

    public (int active, int pooled, int total) TotalStats
    {
        get
        {
            return (
                uiStats.active + effectStats.active + inGameObjectStats.active + audioStats.active,
                uiStats.pooled + effectStats.pooled + inGameObjectStats.pooled + audioStats.pooled,
                uiStats.total + effectStats.total + inGameObjectStats.total + audioStats.total
            );
        }
    }
}
