# Minimoo

Minimoo는 Unity를 위한 기본 프레임워크 패키지입니다.

## 설치 방법

Unity Package Manager를 통해 설치할 수 있습니다:

1. Unity 프로젝트의 `Packages/manifest.json` 파일에 다음을 추가:
```json
{
  "dependencies": {
    "com.minimumstudio.minimoo": "https://github.com/Minimum-Studio-Dev/Minimoo.git#v0.0.1"
  }
}
```

2. 또는 Unity Editor의 Package Manager에서:
   - Window > Package Manager
   - '+' 버튼 클릭
   - 'Add package from git URL...' 선택
   - `https://github.com/Minimum-Studio-Dev/Minimoo.git#v0.0.1` 입력

## 주요 기능

### 로깅 시스템
```csharp
using Minimoo;

// 기본 로그
D.Log("Hello World");

// 경고 로그
D.Warn("경고 메시지");

// 에러 로그
D.Error("에러 메시지");

// 예외 로그
D.Exception(new Exception("예외 발생"));
```

### 네트워크 통신
```csharp
var serverInfo = new ServerInfo();
var networkService = new NetworkService();
// ... 네트워크 설정
```

### 로컬 캐시
```csharp
// 데이터 저장
LocalCacheService.Instance.Save("key", "value");

// 데이터 로드
var value = LocalCacheService.Instance.Load<string>("key");
```

## 라이센스

MIT License 