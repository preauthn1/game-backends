# app-frontend

> 本目录原始 README 见文末，本节为快照补充信息。

原始路径：`app-frontend`　|　文件 **177**　|　主语言：XML 10, JSON 5, C++ 4

## 目录构成

```
lib/screens/  — 49
lib/models/  — 33
lib/widgets/  — 23
android/app/  — 12
windows/runner/  — 9
lib/providers/  — 6
ios/Runner/  — 6
lib/utils/  — 5
linux/  — 4
windows/flutter/  — 3
android/  — 3
linux/flutter/  — 3
```

## 关键文件

- CI/构建：`.gitlab-ci.yml`
- 前端入口：`web/index.html`

---

## 原始说明

# miner

A new Flutter project.

## Getting Started

This project is a starting point for a Flutter application.

A few resources to get you started if this is your first Flutter project:

- [Lab: Write your first Flutter app](https://docs.flutter.dev/get-started/codelab)
- [Cookbook: Useful Flutter samples](https://docs.flutter.dev/cookbook)

For help getting started with Flutter development, view the
[online documentation](https://docs.flutter.dev/), which offers tutorials,
samples, guidance on mobile development, and a full API reference.

## Environment List

To setup environments, edit the following file:

/lib/utils/env_list.dart

## Switching Environments

To switch environments, use the following commands based on your target platform:

### Run in a Specific Environment
```
flutter run --dart-define=ENV=your_environment_name
```

Replace `your_environment_name` with the desired environment name.

### Build APK for a Specific Environment
```
flutter build apk --dart-define=ENV=your_environment_name
```

### Build for Web in a Specific Environment
```
flutter build web --dart-define=ENV=your_environment_name
```

In each case, replace `your_environment_name` with the desired environment.

