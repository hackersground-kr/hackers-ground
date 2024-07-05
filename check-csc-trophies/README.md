# 클라우드 스킬 챌린지 트로피 검증기

이 앱은 Microsoft Learn 클라우드 스킬 챌린지(CSC) 챌린지 완료 여부를 검증하는 도구입니다.

## 사전 준비 사항

- .NET SDK 8.0+

## 사용법

1. 이 리포지토리를 클론합니다.
1. `check-csc-trophies` 디렉토리로 이동합니다.

    ```bash
    # bash/zsh
    REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
    cd $REPOSITORY_ROOT/check-csc-trophies
    
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    cd $REPOSITORY_ROOT/check-csc-trophies
    ```

1. 아래 명령어를 실행하여 앱을 빌드합니다.

    ```bash
    dotnet restore && dotnet build
    ```

1. 아래 명령어를 실행하여 앱을 실행합니다.

    ```bash
    dotnet run --project ./src/HackersGround.Csc.Trophies.ConsoleApp -- -c AZ-900 -u https://learn.microsoft.com/ko-kr/users/<프로필ID>
    ```

1. 만약 강제로 에러를 발생시키고 싶다면 아래 명령어를 실행합니다. 숫자는 0-4 사이의 아무 숫자를 입력합니다. (0: `No challenge code`, 1: `No MS Learn Profile`, 2: `Invalid MS Learn Profile`, 3: `No trophy found`, 4: `Unknown error`)

    ```bash
    dotnet run --project ./src/HackersGround.Csc.Trophies.ConsoleApp -- -c AZ-900 -u https://learn.microsoft.com/ko-kr/users/<프로필ID> --force-error --error-code <숫자>
    ```
