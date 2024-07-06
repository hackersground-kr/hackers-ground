# Get Random Assignee

이슈 담당자를 랜덤하게 선택하기 위한 파워셸 스크립트입니다. 아래 예시와 같이 GitHub 액션에서 활용할 수 있습니다.

> GitHub 시크릿에 `ASSIGNEES`라는 이름으로 담당자 콜렉션이 콤마 기준 혹은 줄바꿈 기준으로 저장되어 있다고 가정합니다.

```yml
- name: Get random assignee
  id: assignee
  shell: pwsh
  run: |
    $scriptUrl = "https://raw.githubusercontent.com/hackersground-kr/operations/main/get-randomassignee/Get-RandomAssignee.ps1"
    Invoke-RestMethod $scriptUrl | Out-File ~/Get-RandomAssignee.ps1
    $assignee = $(~/Get-RandomAssignee.ps1 -Assignees ${{ secrets.ASSIGNEES }})

    echo "value=$assignee" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding utf-8 -Append

- name: Show assignee
  shell: pwsh
  run: |
    echo "${{ steps.assignee.outputs.value }}"
```
