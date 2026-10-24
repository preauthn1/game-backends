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
