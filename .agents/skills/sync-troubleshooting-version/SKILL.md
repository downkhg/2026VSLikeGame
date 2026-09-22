---
name: sync-troubleshooting-version
description: >-
  자동으로 'Doc/트러블슈팅 수정사항.xlsx' 문서의 최신 버전 및 트러블슈팅 이력을 읽어와
  Unity 'GameManager.cs'의 버전 정보와 OnGUI 트러블슈팅 리스트에 동기화하고 커밋 준비를 수행하는 스킬.
---

# Sync Troubleshooting & Version Management Skill

이 스킬은 프로젝트 작업 완료 후 또는 깃(Git) 커밋 단계에서 트러블슈팅 엑셀 문서와 인게임 `GameManager.cs`의 버전 정보를 일치시키기 위한 워크플로우를 정의합니다.

## 규칙 및 원칙

1. **버전 체계 (`Major.Release.Patch`)**:
   - `Major`: 기존 버전에 비해 시스템이 전면 개편된 경우 (릴리즈 전에는 0).
   - `Release`: 실제 빌드 출시 단위 (작업자가 명시적으로 지정할 때만 증가).
   - `Patch`: 버그 수정, 밸런스 패치, 기능 트러블슈팅 시 우선적으로 자동 증가 (`0.00.01` -> `0.00.02` ...).
2. **완료 여부 (`status`)**:
   - 작업자가 명시적으로 결정하며, AI가 임의로 '완료'로 단정 짓지 않고 검토/테스트요청 상태를 존중합니다.
3. **인코딩**:
   - `GameManager.cs` 수정 시 한글 주석 깨짐 방지를 위해 반드시 **UTF-8 with BOM**을 유지합니다.

## 실행 절차

1. `Doc/트러블슈팅 수정사항.xlsx` 파일 내용 확인 (PowerShell 또는 스크립트 활용)
2. 시트의 `적용버전` 및 최신 트러블슈팅 항목 추출
3. `UnityVSLike/Assets/Scripts/GameManager.cs`의 `majorVersion`, `releaseVersion`, `patchVersion` 및 `troubleshootingList` 항목 갱신
4. Git 상태 확인 후 스테이징
