# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**이 파일은 반드시 한국어로 작성해야 합니다.**

## 모든 스크립트는 md파일로 설명이 작성되어 있습니다. 
### Docs/(ScriptName).md 파일을 참조하여 작성해합니다.
### 만약 존재하지 않는다면 Docs/(ScriptName).md로 새로 구현할 내용에 따라 새로 만들어야 합니다.

## 모든 기능을 구현할 때에 핵심 코드에는 한국어로 간단한 주석을 작성합니다.
### 주석이 만약 길어져야 한다면 <Summary></Summary>형식으로 작성하고 아닌 상황에는 //을 사용하여 작성하여야 합니다.

```csharp

// 주석 예제

/*
2줄 이상의 주석은
이렇게 작성합니다.
*/

```

## 변수 작성 규칙
### 1. [SerializeField] private 변수는 camelCase를 사용합니다.
### 2. private 변수는 _ prefix + camelCase를 사용합니다.
### 3. Enum속 변수는 all-uppercase를 사용합니다.
### 4. public field는 camelCase를 사용합니다.
### 5. Property는 public, private 상관없이 PascalCase를 사용합니다.
### 6. const변수는 all-upper + snakeCase를 사용합니다.
### 7. 함수 속 변수는 camelCase를 사용합니다.
### 8. 변수를 선언할 때에 어떤 타입인지 알기 힘든 상황이 아니라면 var를 선언합니다.