import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/app_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../platform/web_js.dart'
    if (example.invalid) '../platform/mobile_stub.dart';

class LinearGradientBackground extends StatelessWidget {
  final Widget child;
  const LinearGradientBackground({super.key, required this.child});

  Widget _buildDownloadApp(BuildContext context, String url) {
    return Material(
      color: const Color(0xFF3a3a3a),
      child: Container(
        width: double.infinity,
        padding: const EdgeInsets.all(8),
        child: Row(
          children: [
            const Expanded(
              flex: 1,
              child: Text(
                'Download App for better experience',
                overflow: TextOverflow.ellipsis,
                style: TextStyle(fontSize: 12),
              ),
            ),
            ElevatedButton(
              onPressed: () {
                launchURL(url);
              },
              style: ElevatedButton.styleFrom(
                minimumSize: Size.zero,
                backgroundColor: AppColors.accentColor,
                padding: const EdgeInsets.all(12),
              ),
              child: const Text(
                'Download',
                style: TextStyle(
                  fontSize: 12,
                  color: Colors.black,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
            const SizedBox(width: 8),
            InkWell(
              borderRadius: BorderRadius.circular(50),
              onTap: () {
                context.read<AppProvider>().hideDownloadApp();
              },
              child: const Icon(Icons.close, size: 20),
            )
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final appProvider = context.watch<AppProvider>();
    String? appUrl;
    if (kIsWeb) {
      if (isAndroidDevice) {
        appUrl = appProvider.template?.androidUrl;
      }
    }
    return Column(
      children: [
        if (appUrl != null &&
            appUrl.isNotEmpty &&
            !appProvider.hiddenDownloadApp)
          _buildDownloadApp(context, appUrl),
        Expanded(
          child: DecoratedBox(
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: [
                  Color(0xFF202020),
                  Color(0xFF0D0D0E),
                ],
              ),
            ),
            child: Align(
              alignment: Alignment.topCenter,
              child: ConstrainedBox(
                constraints: const BoxConstraints(
                  maxWidth: 700,
                ),
                child: child,
              ),
            ),
          ),
        ),
      ],
    );
  }
}
