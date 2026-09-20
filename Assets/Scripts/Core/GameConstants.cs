using UnityEngine;

namespace RetroSpaceInvader.Core
{
    /// <summary>
    /// 게임 전반에서 사용되는 상수 및 설정값 정의
    /// (Pygame constants.py와 1:1 대응)
    /// </summary>
    public static class GameConstants
    {
        // 1. 화면 및 좌표계 설정 (Orthographic Size = 300, 1 Unit = 1 Pixel)
        public const float ScreenWidth = 800f;
        public const float ScreenHeight = 600f;
        public const float HalfWidth = 400f;
        public const float HalfHeight = 300f;

        // 2. 플레이어 관련 수치
        public const float PlayerWidth = 48f;
        public const float PlayerHeight = 48f;
        public const float PlayerSpeed = 300f;              // 초당 300 픽셀 (5px * 60fps)
        public const int PlayerMaxLives = 3;
        public const int PlayerMaxMissiles = 5;            // 화면 동시 최대 미사일 수
        public const float PlayerInvincibleDuration = 1.0f; // 피격 무적 시간 1초
        public const float PlayerStartY = -265f;            // 하단 플레이어 위치

        // 3. 미사일 및 탄환 수치
        public const float MissileWidth = 12f;
        public const float MissileHeight = 28f;
        public const float MissileSpeed = 540f;             // 초당 540 픽셀 (9px * 60fps)

        public const float EnemyLaserWidth = 14f;
        public const float EnemyLaserHeight = 28f;
        public const float EnemyLaserSpeed = 240f;          // 초당 240 픽셀 (4px * 60fps)

        // 4. 외계인 편대 수치
        public const int AlienRows = 5;
        public const int AlienCols = 5;
        public const float AlienWidth = 42f;
        public const float AlienHeight = 38f;
        public const float AlienSpacingX = 20f;
        public const float AlienSpacingY = 14f;

        // 편대 시작 좌표 (화면 중심 (0,0) 기준)
        // 가로 폭: 5 * 42 + 4 * 20 = 290px -> 왼쪽 시작점: -145 + 21 = -124
        public const float AlienStartX = -130f;
        public const float AlienStartY = 180f;

        public const float AlienStepX = 12f;                // 1회 좌우 이동 픽셀
        public const float AlienMoveDownStep = 18f;         // 벽 충돌 시 하강 픽셀

        // 외계인 이동 주기 (초 단위: 32프레임 = 약 0.533초, 3프레임 = 0.05초)
        public const float AlienBaseMoveInterval = 32f / 60f;
        public const float AlienMinMoveInterval = 3f / 60f;

        // 적 탄환 발사 주기 (75프레임 = 1.25초)
        public const float EnemyShootInterval = 75f / 60f;

        // 플레이어 방어선 (외계인 침공 판정선)
        public const float DefenseLineY = -230f;

        // 5. 점수표 (1행, 2행, 3행, 4행, 5행)
        public const int ScoreAlienTop = 30;
        public const int ScoreAlienMiddle = 20;
        public const int ScoreAlienBottom = 10;
        public static readonly int[] RowScores = { ScoreAlienTop, ScoreAlienMiddle, ScoreAlienMiddle, ScoreAlienBottom, ScoreAlienBottom };

        // 6. 색상
        public static readonly Color ColorBg = new Color(10f / 255f, 10f / 255f, 16f / 255f);
        public static readonly Color ColorPlayer = new Color(57f / 255f, 255f / 255f, 20f / 255f);
        public static readonly Color ColorAlienRow1 = new Color(255f / 255f, 0f / 255f, 127f / 255f);
        public static readonly Color ColorAlienRow2 = new Color(0f / 255f, 240f / 255f, 255f / 255f);
        public static readonly Color ColorAlienRow3 = new Color(255f / 255f, 230f / 255f, 0f / 255f);
        public static readonly Color ColorEnemyLaser = new Color(255f / 255f, 60f / 255f, 60f / 255f);
        public static readonly Color ColorGold = new Color(255f / 255f, 215f / 255f, 0f / 255f);
        public static readonly Color ColorCyan = new Color(0f / 255f, 240f / 255f, 255f / 255f);
    }

    public enum GameState
    {
        Start,
        Playing,
        StageClear,
        GameOver
    }
}
