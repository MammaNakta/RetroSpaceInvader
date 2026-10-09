# GEMINI.md - Unity (C#) 실전 개발 지침 및 3계층 검증 하네스 가이드

본 문서는 **Unity (C#)** 프로젝트의 실용적이고 빠른 개발을 위한 코드 스타일, 성능 최적화 기준, 유지보수 원칙 및 **3계층 검증 하네스(3-Tier Verification Harness)** 체계를 정의합니다.

---

## 1. 개발자 페르소나: 실용적이고 민첩한 모던 유니티 개발자 (Pragmatic & Agile Game Developer)

### 1.1 KISS & 실용성 우선 (Pragmatism over Perfectionism)
- **가독성과 생산성의 균형**: 불필요한 제네릭 인터페이스 계층, 과도한 디자인 패턴(무거운 DI, 엔터프라이즈급 추상화 등)을 지양하고 직관적이고 읽기 쉬운 C# 코드를 작성합니다.
- **KISS (Keep It Simple, Stupid) & YAGNI**: 지금 당장 필요한 기능에 집중하며, 미래의 모호한 확장성을 위해 코드를 미리 복잡하게 만들지 않습니다.
- **MonoBehaviour와 순수 데이터의 합리적 분리**: 비즈니스 데이터나 계산 로직은 순수 C# 클래스나 `ScriptableObject`/`GameConstants`로 관리하고, 입력/연출/물리는 `MonoBehaviour`가 담당합니다.

### 1.2 상식적인 성능 최적화 (Common-Sense Performance)
- **핫패스(Hot Path) 주의**: 매 프레임 실행되는 `Update()`, `FixedUpdate()` 내부에서 눈에 띄는 대량의 `new` 객체 생성, 잦은 문자열 연결(`+`)을 지양합니다. (초기화나 이벤트 콜백 등 일회성 로직에서는 실용적인 람다나 편의 구문 사용 가능)
- **컴포넌트 및 참조 캐싱**: `GetComponent<T>()`, `Camera.main` 등 빈번히 조회되는 컴포넌트는 `Awake()` / `Start()`에서 변수에 캐싱하여 사용합니다.
- **오브젝트 풀링 (Object Pooling)**: 탄환, 폭발 이펙트처럼 1초에 수십 개씩 생성/파괴되는 요소는 가급적 풀링을 활용해 프레임 드랍을 방지합니다.

### 1.3 C# 네이밍 및 인스펙터 컨벤션
- **클래스 / 메서드 / 프로퍼티**: `PascalCase` (`PlayerController`, `TakeHit()`, `CurrentScore`)
- **Private 필드**: `_camelCase` (`_lives`, `_moveTimer`)
- **Inspector 노출 필드**: `[SerializeField] private float speed;` 또는 간결한 `public float speed;` 사용

---

## 2. 기존 코드베이스 보존 및 점진적 개선 (Codebase Integrity)

### 2.1 최소 영향 범위 (Minimal Blast Radius)
- **불필요한 대규모 재작성 금지**: 버그 수정이나 새 기능 추가 시 요청받지 않은 기존 폴더 구조나 아키텍처를 임의로 갈아엎지 않습니다.
- **점진적 개선**: 기존 파일의 코드 스타일(들여쓰기, 네이밍 규칙)을 존중하며 수정 대상과 직결된 코드만 정밀하게 변경합니다.

### 2.2 직렬화 및 인터페이스 하위 호환성
- `SerializeField` 변수명을 임의로 수정하여 Inspector에 할당된 기존 프리팹 데이터나 참조가 유실되지 않도록 주의합니다.
- 사운드 클립(`AudioClip`)이나 스프라이트(`Sprite`)가 아직 할당되지 않은 상태에서도 에러 없이 기본 동작하거나 로그만 남기도록 **Null Safety Fallback**을 기본 적용합니다.

---

## 3. 실전형 3계층 검증 하네스 체계 (3-Tier Verification Harness)

형식적인 단위 테스트를 넘어, **개발 속도를 높이고 버그를 즉각 잡아내는 실전형 하네스**를 운영합니다.

```
┌─────────────────────────────────────────────────────────────┐
│ Tier 1: 에디터 퀵 진단 (Editor Sanity Check)                │
│ - 누락된 스크립트(Missing Component) & 레퍼런스 진단         │
│ - 에디터 메뉴: Tools > Harness > Run Quick Sanity Check     │
├─────────────────────────────────────────────────────────────┤
│ Tier 2: 코어 로직 & 무결성 테스트 (Core Logic / Invariant)   │
│ - 점수 계산, 랭킹 데이터 정제, Null Safety 로직 검증        │
│ - 에디터 메뉴: Tools > Harness > Run Invariant Smoke Tests   │
├─────────────────────────────────────────────────────────────┤
│ Tier 3: 런타임 인게임 디버그/치트 (In-Game Runtime Debug)     │
│ - F1(무적), F2(점수+1000), F3(적전멸), F4(피격), F5(배속)    │
│ - 플레이테스트 및 엣지 케이스 즉시 재현                      │
└─────────────────────────────────────────────────────────────┘
```

### 3.1 Tier 1: 에디터 퀵 진단 하네스 (`EditorSanityCheckHarness.cs`)
- **목적**: 씬이나 프리팹을 수정했을 때 컴포넌트가 깨지거나(Missing Script) 필수 싱글톤/에셋 참조가 비어있는지 1초 만에 검증.
- **실행 방법**: 상단 메뉴 `Tools > Harness > Run Quick Sanity Check` (단축키: `Ctrl + Shift + S`)
- **검증 항목**:
  1. 씬 내 모든 오브젝트의 Missing Script 검사
  2. `GameManager`, `UIManager` 등 필수 매니저 배치 및 의존성 연결 확인
  3. `PlayerMissile`, `EnemyLaser`, `ExplosionEffect` 등 주요 프리팹 존재 여부 확인

### 3.2 Tier 2: 코어 로직 & 무결성 테스트 하네스 (`SmokeTestHarness.cs`)
- **목적**: 씬 렌더링과 무관한 순수 C# 게임 규칙, 점수 연산, 랭킹 데이터 저장/로드 및 Null 방어 로직의 무결성 검증.
- **실행 방법**:
  - **에디터 내부**: 상단 메뉴 `Tools > Harness > Run Invariant Smoke Tests` 클릭
  - **CLI 배치모드(CI)**:
    ```bash
    Unity.exe -batchmode -nographics -projectPath . -executeMethod RetroSpaceInvader.Tests.SmokeTestHarness.RunAllTestsBatch -logFile unity-harness.log
    ```
- **검증 항목**:
  1. 점수 연산 및 음수 방어 인바리언트
  2. 랭킹 데이터 정제(Null/공백 이름, 음수 점수 자동 방어 및 내림차순 정렬)
  3. 에셋 미할당 시 `AudioManager` 예외 미발생 검증

### 3.3 Tier 3: 런타임 인게임 디버그 하네스 (`RuntimeDebugHarness.cs`)
- **목적**: 게임 실행 중 특정 상황(보스전, 게임오버, 스테이지 클리어)을 테스트하기 위해 번거롭게 수동 플레이를 오래 하지 않고 핫키로 즉시 재현.
- **동작 방식**: `UNITY_EDITOR` 및 `DEVELOPMENT_BUILD`에서 씬 시작 시 자동 초기화(수동 배치 불필요).
- **디버그 단축키**:
  - `F1`: **무적 모드(God Mode)** 토글 (피격 무시)
  - `F2`: **점수 즉시 지급** (+1000점)
  - `F3`: **적 편대 전멸** (스테이지 클리어 즉시 전환 테스트)
  - `F4`: **플레이어 즉시 피격** (목숨 감소 및 게임오버 전이 테스트)
  - `F5`: **게임 배속 조절** (1.0x → 2.0x → 5.0x → 0.5x)
  - `F12`: 디버그 HUD 화면 표시/숨김 토글

---

## 4. AI 에이전트 작업 수칙

1. **사전 파악 (Read First)**: 코드를 수정하기 전 관련 클래스 필드와 Inspector 연결 구조를 확인합니다.
2. **사후 검증 (Verify Fast)**:
   - 스크립트 작성/수정 후 컴파일 에러가 없는지 확인합니다.
   - 씬/프리팹 관련 작업 시 **Tier 1 (Run Quick Sanity Check)** 을 확인합니다.
   - 로직/데이터 관련 작업 시 **Tier 2 (Run Invariant Smoke Tests)** 를 실행하여 회귀 버그를 방지합니다.
3. **명확한 로깅**: 빈 catch 블록(`catch (Exception) {}`)을 지양하고, 예외 상황 시 `Debug.LogWarning` 또는 `Debug.LogError`로 원인을 명시합니다.
