# 👾 Retro Space Invader (레트로 스페이스 인베이더)

[![Version](https://img.shields.io/badge/version-v0.1.0-brightgreen.svg)](https://github.com/MammaNakta/RetroSpaceInvader/releases/tag/v0.1.0)
[![Unity 6](https://img.shields.io/badge/Unity-6000.3.22f1-blue.svg?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Git LFS](https://img.shields.io/badge/Git-LFS%20Optimized-orange.svg)](https://git-lfs.github.com/)

> **1978년 오락실의 감성을 현대적 유니티 C# 아키텍처로 복원한 레트로 2D 아케이드 슈팅 게임**  
> 8비트 도트 픽셀 그래픽, 칩튠 사운드, 절차적 별빛 배경, 외계인 편대 지그재그 기동, 그리고 영구 보존되는 오락실 3글자 명예의 전당 랭킹 시스템을 제공합니다.

---

## 👨‍💻 개발자 정보 (Developer)

* **개발자**: **MammaNakta** ([@MammaNakta](https://github.com/MammaNakta))
* **이메일**: `gjdlrnzz@gmail.com`
* **저장소**: [https://github.com/MammaNakta/RetroSpaceInvader](https://github.com/MammaNakta/RetroSpaceInvader)

---

## 🎮 게임 조작법 (Controls)

| 키 입력 | 기능 |
| :--- | :--- |
| **`←` / `→`** 또는 **`A` / `D`** | 플레이어 우주선 좌우 이동 |
| **`Space`** | 미사일 발사 (화면 내 최대 5발 동시 발사) / 게임 시작 |
| **`A - Z` (키보드 영문)** | 게임오버 시 3글자 랭킹 이니셜 입력 |
| **`Backspace`** | 이니셜 한 글자 지우기 |
| **`Enter`** | 랭킹 등록 확정 |
| **`R` / `Space`** | 스테이지 클리어 후 다음 단계 진행 / 게임오버 후 재도전 |

---

## 🛠️ 게임 테스트 및 실행 환경 조건 (Test & Verification Guide)

본 프로젝트를 정상적으로 테스트하고 검증하기 위해 다음의 실행 조건과 절차를 준수해야 합니다.

### 1. 사전 필수 실행 환경 (Prerequisites)
* **Unity 에디터 버전**: `Unity 6 (6000.3.22f1)` 권장
* **렌더 파이프라인**: 2D Orthographic (Camera Size: 300)
* **권장 테스트 해상도**: **`800 x 600` (4:3 레트로 종횡비 고정)**
  > 💡 Game 뷰 해상도 드롭다운에서 `800x600` 또는 `Standalone (800x600)`으로 설정 시 최적의 도트 픽셀 비율로 테스트할 수 있습니다.

---

### 2. 프로젝트 실행 (Instant Play)
`Assets/Scenes/MainGameScene.unity` 씬을 열고 유니티 상단 **`Play (▶)`** 버튼만 누르면 모든 하트 UI와 게임 리소스가 자동으로 로드되어 즉시 플레이할 수 있습니다.

---

### 3. 게임플레이 기능별 검증 테스트 조건 (Acceptance Criteria)

#### ① 플레이어 조작 및 발사 인바리언트
- [ ] 우주선이 화면 좌우 경계(-370 ~ +370)를 벗어나지 않고 안전하게 클램프되는지 확인.
- [ ] 스페이스바를 연타해도 **화면 내에 동시에 존재하는 미사일은 최대 5발**로 제한되는지 확인.
- [ ] 미사일 발사 시 8비트 레이저 발사음(`laser_beep.wav`)이 정상 출력되는지 확인.

#### ② 외계인 편대 기동 및 피격 판정
- [ ] 5행 11열 총 55기의 외계인이 일정한 간격으로 생성되는지 확인:
  - **Top 행 (1행)**: 30 PTS
  - **Middle 행 (2~3행)**: 20 PTS
  - **Bottom 행 (4~5행)**: 10 PTS
- [ ] 벽면에 부딪힐 때마다 편대 전체가 한 칸 하강하며 반대 방향으로 전환되는지 확인.
- [ ] 외계인 수가 줄어들수록 기동 주기(이동 속도)가 가속되는지 확인.
- [ ] 외계인이 불규칙하게 레이저를 투하하며 플레이어를 공격하는지 확인.
- [ ] 외계인 피격 시 폭발 파티클 및 폭발음(`explosion.wav`) 재생 후 점수가 가산되는지 확인.

#### ③ 플레이어 피격 및 무적 상태
- [ ] 적 레이저 피격 시 우주선 목숨이 1 감소하고 상단 HUD 하트 아이콘이 갱신되는지 확인.
- [ ] 피격 직후 **1.0초간 스프라이트가 깜빡이며 무적 상태**가 유지되어 연속 피격을 방지하는지 확인.

#### ④ 스테이지 클리어
- [ ] 편대 55기를 모두 섬멸하면 `★ STAGE CLEAR ★` 오버레이가 활성화되는지 확인.
- [ ] `Space` 또는 `R` 입력 시 다음 스테이지로 진입하며 편대가 다시 리셋되는지 확인.

#### ⑤ 게임오버 및 명예의 전당(TOP 5) 랭킹 영구 보존
- [ ] 목숨 3개가 모두 소진되면 게임오버 사운드(`game_over.wav`)와 함께 `GAME OVER` 화면으로 전환되는지 확인.
- [ ] 키보드로 3자리 이니셜(예: `AAA`, `NAK`)을 입력하고 엔터를 누르면 명예의 전당 TOP 5 순위에 즉시 반영되는지 확인.
- [ ] 게임을 완전히 종료했다가 다시 실행해도 이전 랭킹 기록이 보존되는지 확인 (PlayerPrefs 무결성 검증).

---

### 4. 자동화 테스트 하네스 검증 (Automated Verification Harness)

본 프로젝트는 무인 자동화 테스트 및 인바리언트 검증을 위한 하네스([SmokeTestHarness.cs](file:///c:/Users/niji0/Desktop/antigravityStudy/RetroSpaceInvader/Assets/Editor/Tests/SmokeTestHarness.cs))를 내장하고 있습니다.

#### 방법 A: 유니티 에디터 메뉴에서 1클릭 실행
* 에디터 상단 메뉴에서 **`Retro Space Invader` > `Run Verification Harness`** 를 클릭합니다.
* 점수 계산 인바리언트, 랭킹 매니저 데이터 무결성(null/음수 방어), 오디오 매니저 널 세이프티 검증이 즉시 수행되며 결과 팝업창이 표시됩니다.

#### 방법 B: Unity Test Runner 창에서 실행
* Unity 에디터 메뉴 `Window` > `General` > `Test Runner`를 엽니다.
* **EditMode** 및 **PlayMode** 탭에서 `SmokeTestHarness`의 테스트를 실행하여 All Green(Pass) 여부를 확인합니다.

#### 방법 C: CI/CLI 배치모드 헤드리스 자동 실행
GUI 창을 띄우지 않고 터미널에서 헤드리스로 테스트를 검증할 때:
```powershell
Unity.exe -batchmode -nographics -projectPath . -executeMethod RetroSpaceInvader.Tests.SmokeTestHarness.RunAllTestsBatch -logFile build/smoke-test.log
```

---

## 📁 주요 프로젝트 구조

```text
RetroSpaceInvader/
├── Assets/
│   ├── Editor/               # 1클릭 셋업 마법사 & 테스트 하네스
│   │   ├── ProjectSetupWizard.cs
│   │   └── Tests/SmokeTestHarness.cs
│   ├── GameAssets/           # 8비트 스프라이트(PNG) & 사운드(WAV)
│   ├── Prefabs/              # 플레이어, 적, 탄환, 폭발 이펙트 프리팹
│   ├── Scenes/               # 메인 게임 씬 (MainGameScene.unity)
│   └── Scripts/
│       ├── Audio/            # 효과음 재생 및 오디오 매니저
│       ├── Core/             # 게임 상수 및 상태 머신 (GameManager)
│       ├── Effects/          # 별빛 스크롤러 및 폭발 연출
│       ├── Enemy/            # 외계인 개체 및 편대 기동 매니저
│       ├── Player/           # 플레이어 이동, 발사, 피격 제어
│       ├── Ranking/          # TOP 5 랭킹 데이터 정렬 및 저장 관리
│       └── UI/               # HUD, 3글자 이니셜 슬롯 입력 및 리더보드 UI
├── Packages/                 # Package Manager 의존성
├── ProjectSettings/          # 유니티 프로젝트 전역 설정
├── .gitattributes            # EOL 정규화 & Git LFS 최적화
└── .gitignore                # Unity 캐시 제외 완벽 구성
```
