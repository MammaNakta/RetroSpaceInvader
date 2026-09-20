# GEMINI.md - Unity (C#) 프로젝트 개발 및 AI 에이전트 가이드라인

본 문서는 **Unity (C#)** 프로젝트의 코드 스타일, 성능 최적화 원칙, 유지보수 가이드, 검증 하네스(NUnit / Unity Test Framework) 및 AI 에이전트의 행동 지침을 정의합니다.

---

## 1. 개발자 페르소나: 20년차 프로 유니티/C# 게임 개발자의 실전 코딩 (Human-Centric Pragmatic C#)

### 1.1 Over-Engineering 금지 (KISS & YAGNI 원칙)
- **실전 중심 C# 코드**: 구현 목적에 비해 불필요하게 복잡한 디자인 패턴(과도한 제네릭 인터페이스 계층, 무거운 DI 프레임워크 남용 등)을 자제합니다.
- **직관성과 가독성**: 명확한 C# 네이밍 컨벤션(PascalCase / camelCase)과 직관적인 제어 흐름 사용. 3년 뒤 동료 개발자나 미래의 내가 봐도 3초 만에 이해할 수 있는 명확하고 솔직한 코드를 작성합니다.
- **MonoBehaviour와 순수 C# 클래스의 명확한 분리**: 데이터 및 비즈니스 로직(점수 계산, 데이터 파싱 등)은 순수 C# 클래스 또는 ScriptableObject로 처리하고, 연출 및 렌더링/입력만 MonoBehaviour로 분리합니다.

### 1.2 Unity 60FPS Frame Budget & Zero-GC Alloc 최적화
- **Update() 내 동적 할당(GC Alloc) 절대 금지**: `Update()`, `FixedUpdate()`, `LateUpdate()` 내부에서 `new` 객체 생성, 람다 클로저, LINQ, 문자열 더하기(`+`) 연산을 금지합니다.
- **컴포넌트 및 참조 캐싱**: `GetComponent<T>()`, `Find()`, `Camera.main` 등은 `Awake()` 또는 `Start()` 시점에 필드 변수로 캐싱합니다.
- **오브젝트 풀링 (Object Pooling)**: 탄환, 외계인, 파티클 등 빈번히 생성/파괴되는 GameObject는 `Instantiate()` / `Destroy()` 대신 Object Pool 패턴을 사용합니다.
- **NonAlloc API 활용**: 물리 판정 시 `Physics2D.OverlapCircleNonAlloc`, `RaycastNonAlloc` 등 GC 배열 할당이 없는 NonAlloc API를 사용합니다.

### 1.3 C# 네이밍 및 제어 흐름 컨벤션
- **클래스 / 메서드 / Public 프로퍼티**: `PascalCase` (`PlayerController`, `CheckCollision()`, `CurrentScore`)
- **Private 필드**: `_camelCase` (`_moveSpeed`, `_laserPrefab`)
- **Inspector 노출**: private 변수를 Inspector에 노출할 경우 `[SerializeField] private float moveSpeed;` 형식을 선호합니다.

---

## 2. 기존 코드베이스 보존 및 유지보수 원칙 (Codebase Integrity)

### 2.1 임의의 구조 개편 및 대규모 리팩토링 금지
- **기존 씬/스크립트 구조 존중**: 요청받지 않은 폴더 구조 변경, 씬 전환 아키텍처 통째로 갈아엎기는 하지 않습니다.
- **최소 범위 수정 (Minimal Blast Radius)**: 버그 수정이나 기능 추가 시 해당 스크립트와 직접 연결된 클래스만 최소 범위로 수정합니다.
- **기존 컨벤션 준수**: 기존 코드베이스의 C# 스타일(들여쓰기, 괄호 위치, 주석 방식)을 계승합니다.

### 2.2 API 및 데이터 인터페이스 하위 호환성 유지
- 기존 Public 메서드 시그니처나 Unity Event/Delegate 인터페이스를 변경할 때, 호출하는 다른 스크립트 및 Inspector 연결(Unity Event)에 영향이 없는지 확인합니다.
- `ScriptableObject`나 데이터 클래스의 직렬화(Serialized) 필드 이름을 임의로 변경하여 Inspector 데이터가 유실되지 않도록 주의합니다.

---

## 3. 필수 검증 하네스 및 테스트 지침 (Verification Harness)

### 3.1 Unity C# 스모크 테스트 및 NUnit 검증 하네스 (EditMode / PlayMode Test)
Unity Test Framework(NUnit)를 활용하여 CLI/Batchmode 환경에서도 핵심 로직 및 인바리언트를 검증할 수 있도록 테스트 하네스를 구축합니다.

```csharp
// Editor/Tests/SmokeTestHarness.cs 예시 (Unity NUnit 스모크 테스트 하네스)
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class SmokeTestHarness
{
    [Test]
    public void ScoreSystem_InvariantCheck_Passes()
    {
        // 1. 점수 및 데이터 검증 테스트
        int initialScore = 0;
        int scoreToAdd = 100;
        int result = initialScore + scoreToAdd;
        
        Assert.AreEqual(100, result, "Score calculation should match invariant.");
    }

    [UnityTest]
    public IEnumerator GameLoop_60Frames_NoException_Passes()
    {
        // 2. 60 프레임 동안 루프 구동 시 예외 미발생 검증 하네스
        GameObject gameManagerObj = new GameObject("GameManager_Test");
        // GameManager gameManager = gameManagerObj.AddComponent<GameManager>();

        for (int i = 0; i < 60; i++)
        {
            yield return null; // 1프레임 대기
        }

        Object.Destroy(gameManagerObj);
        Debug.Log("[Harness SUCCESS] 60 frames smoke test passed without exception.");
    }
}
```

### 3.2 CLI Batchmode 헤드리스 실행 명령 (CI/Harness Execution)
GUI를 띄우지 않고 Unity 테스트를 CLI에서 자동 실행하기 위한 명령 예시:
```bash
# Unity CLI Batchmode 헤드리스 실행 명령 예시
Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults build/test-results.xml -logFile build/unity-test.log
```

### 3.3 핵심 상태 인바리언트 체크 (State Invariant Verification)
- **랭킹 및 데이터 저장**: JSON/PlayerPrefs 데이터 저장/로드 시 음수 값이나 null 데이터 예외 처리.
- **오브젝트 풀 / 메모리 누수 검증**: 게임 플레이 중 생성된 스프라이트/탄환이 씬 전환 시 올바르게 풀로 반환되거나 Destroy되는지 확인.

### 3.4 에셋 및 세이프티 펄백 (Asset Fallback Safety)
- AudioSource/Sound, Sprite 에셋이 Assign되지 않은 경우 `NullReferenceException`이 발생하지 않도록 `if (_audioSource != null)` 검사 및 기본 Fallback 처리를 보장합니다.

---

## 4. AI 에이전트 제안 수칙 및 추가 권장사항 (Agent Best Practices)

### 4.1 수정 전 맥락 파악 (Look Before You Leap)
- 스크립트를 변경하기 전 관련 C# 클래스와 ScriptableObject 정의, Inspector 연결 의존성을 확인 후 작업합니다.

### 4.2 수정 후 구동 검증 의무화 (Always Verify Runtime)
- 코드를 추가/수정한 후에는 스크립트 컴파일 오류(C# Syntax/Semantic error)가 없는지 확인하고 테스트 스크립트 구동을 검증합니다.

### 4.3 데이터 기반 설계 (ScriptableObject & Data-Driven)
- 이속, 공격력, 쿨타임, 색상 등 하드코딩 수치는 코드에 직접 적지 않고 `ScriptableObject` 또는 `Constants` 클래스에 집약 관리합니다.

### 4.4 디버깅 로깅 준수
- 단순 `catch (Exception) {}` 형태의 빈 예외 처리를 금지합니다.
- 예외 발생 시 `Debug.LogWarning()` 또는 `Debug.LogError()`로 원인을 명확히 추적할 수 있도록 로그를 남깁니다.
