import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:miner/widgets/base_button.dart';
import 'package:open_file/open_file.dart';
import 'package:permission_handler/permission_handler.dart';

import '../utils/utils.dart';
import 'base_card.dart';
import 'base_dialog.dart';

class AndroidUpdate extends StatefulWidget {
  final String url;
  const AndroidUpdate({super.key, required this.url});

  @override
  State<AndroidUpdate> createState() => _AndroidUpdateState();
}

class _AndroidUpdateState extends State<AndroidUpdate> {
  double _progress = 0.0;
  String _progressText = '0%';
  bool _needPermission = false;

  Future<String> getSavePath() async => await getTemporaryPath('app.apk');

  Future<void> _startDownload() async {
    final hasPermission = await checkAndRequestInstallPackagesPermission();
    if (!hasPermission) {
      _needPermission = true;
    }
    final savePath = await getSavePath();
    try {
      Dio dio = Dio();
      await dio.download(
        widget.url,
        savePath,
        onReceiveProgress: (received, total) {
          if (total != -1) {
            setState(() {
              _progress = received / total;
              _progressText = '${(_progress * 100).toStringAsFixed(0)}%';
            });
          }
        },
      );

      if (_progress == 1.0) {
        await OpenFile.open(savePath);
      }
    } catch (e) {
      if (!mounted) return;
      showDialog(
        context: context,
        builder: (_) => BaseDialog(
          titleText: 'Error',
          content: Text(e.toString()),
          onConfirm: () => Navigator.pop(context),
        ),
      );
    }
  }

  @override
  void initState() {
    super.initState();
    _startDownload();
  }

  @override
  Widget build(BuildContext context) {
    return Center(
      child: BaseCard(
        width: double.infinity,
        margin: const EdgeInsets.all(20),
        padding: const EdgeInsets.all(30),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          mainAxisSize: MainAxisSize.min,
          children: _needPermission
              ? [
                  const Text(
                    'Update failed',
                    style: TextStyle(fontSize: 16),
                  ),
                  const SizedBox(height: 20),
                  const Text(
                    'Error: Please allow the installation package permission',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      color: Colors.red,
                      fontSize: 16,
                    ),
                  ),
                  const SizedBox(height: 20),
                  SizedBox(
                    child: BaseButton(
                      text: 'Settings',
                      onPressed: () {
                        openAppSettings();
                      },
                    ),
                  ),
                ]
              : [
                  const Text('Updating...', style: TextStyle(fontSize: 16)),
                  const SizedBox(height: 20),
                  LinearProgressIndicator(value: _progress),
                  const SizedBox(height: 20),
                  Text(_progressText),
                  if (_progress == 1.0) const SizedBox(height: 20),
                  if (_progress == 1.0)
                    BaseButton(
                      text: 'Install',
                      onPressed: () async {
                        OpenFile.open(await getSavePath());
                      },
                    ),
                ],
        ),
      ),
    );
  }
}
