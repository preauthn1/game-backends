import 'dart:io';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:miner/utils/utils.dart';
import 'package:provider/provider.dart';

import '../providers/app_provider.dart';
import '../providers/auth_provider.dart';
import '../widgets/android_update.dart';
import '../widgets/linear_gradient_background.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  Widget _body = const Center(
    child: CircularProgressIndicator(),
  );

  Future<void> _init() async {
    try {
      final auth = context.read<AuthProvider>();
      final app = context.read<AppProvider>();
      await app.loadPackageInfo();
      await auth.readAndSetToken();
      await app.fetchAndSetTemplate(includeToken: [REDACTED];
      try {
        if (Platform.isAndroid &&
            isUpdateAvailable(
              app.version,
              app.template?.androidMinVersion ?? '1.0.0',
            ) &&
            app.template?.androidUrl != null &&
            app.template?.androidUrl != '') {
          setState(() {
            _body = AndroidUpdate(url: app.template!.androidUrl!);
          });
          return;
        }
      } catch (e) {
        //
      }
      if (!mounted) return;
      if (auth.token != null) {
        context.go('/home');
      } else {
        context.go('/login');
      }
    } catch (e) {
      context.go('/login');
    }
  }

  @override
  void initState() {
    _init();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(body: _body),
    );
  }
}
