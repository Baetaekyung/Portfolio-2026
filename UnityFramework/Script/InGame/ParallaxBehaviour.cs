using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배경 오브젝트를 한 방향으로 무한 스크롤하는 컴포넌트
/// 동일한 크기의 배경 2개를 할당하면 자동으로 순환합니다.
/// </summary>
public class ParallaxBehaviour : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("초당 이동 속도")] 
    private float moveSpeed = 5f;
    
    [SerializeField, Tooltip("왼쪽으로 이동할지 여부")] 
    private bool moveLeft = true;

    [Header("References")]
    private Camera _inGameCamera;
    [SerializeField] private BoxCollider2D[] backgrounds; // 2개의 배경을 배열로 관리

    private float _backgroundWidth;

    private void Awake()
    {
        _inGameCamera = Camera.main;
    }

    private void Start()
    {
        // 두 배경의 너비가 같다고 가정하고 첫 번째 것의 너비를 구함
        if (backgrounds.Length > 0)
        {
            _backgroundWidth = backgrounds[0].bounds.size.x;
        }
    }

    private void Update()
    {
        MoveBackgrounds();
        CheckAndReposition();
    }

    // 1. 배경 이동 로직
    private void MoveBackgrounds()
    {
        float direction = moveLeft ? -1f : 1f;
        Vector3 movement = Vector3.right * (direction * moveSpeed * Time.deltaTime);

        foreach (var bg in backgrounds)
        {
            bg.transform.position += movement;
        }
    }

    // 2. 화면 밖으로 나갔는지 확인하고 재배치 (수정됨)
    private void CheckAndReposition()
    {
        // 카메라의 현재 X 좌표
        float camX = _inGameCamera.transform.position.x;

        foreach (var bg in backgrounds)
        {
            // 배경과 카메라의 거리 차이 (X축 기준)
            float dist = bg.transform.position.x - camX;

            if (moveLeft)
            {
                // [수정 핵심] 카메라 왼쪽 경계선이 아니라, '배경 너비'만큼 멀어졌는지를 체크
                // 배경이 카메라 중심에서 왼쪽으로 '너비'만큼 벗어났다면 즉시 뒤로 보냄
                if (dist < -_backgroundWidth)
                {
                    RepositionBackground(bg, 1);
                }
            }
            else
            {
                // 오른쪽으로 이동 중일 때
                if (dist > _backgroundWidth)
                {
                    RepositionBackground(bg, -1);
                }
            }
        }
    }

    /// <summary>
    /// 배경을 반대편 끝으로 이동시킵니다.
    /// </summary>
    /// <param name="target">이동할 배경</param>
    /// <param name="direction">1: 오른쪽 끝에 붙임, -1: 왼쪽 끝에 붙임</param>
    private void RepositionBackground(BoxCollider2D target, int direction)
    {
        // 간단한 공식: 현재 위치에서 (배경 개수 * 너비) 만큼 이동하면 반대편 꼬리로 이동됨
        // 예: 2개의 배경이라면, 자신의 너비 * 2 만큼 옆으로 가면 됨
        float offset = _backgroundWidth * backgrounds.Length; 
        
        Vector3 newPos = target.transform.position;
        newPos.x += offset * direction;
        target.transform.position = newPos;
    }
}