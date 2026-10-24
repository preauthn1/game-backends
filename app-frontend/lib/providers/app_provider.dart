import 'package:flutter/foundation.dart';
import 'package:package_info_plus/package_info_plus.dart';

import '../platform/web_js.dart'
    if (example.invalid) '../platform/mobile_stub.dart';
import '../models/template.dart';
import '../utils/http_service.dart';

class AppProvider with ChangeNotifier {
  PackageInfo? _packageInfo;
  Template? _template;
  bool _hiddenDownloadApp = sessionStorage?['hiddenDownloadApp'] == 'true';

  String get version => _packageInfo?.version ?? '-';
  Template? get template => _template;
  bool get hiddenDownloadApp => _hiddenDownloadApp;

  Future<void> loadPackageInfo() async {
    PackageInfo packageInfo = await PackageInfo.fromPlatform();
    _packageInfo = packageInfo;
    notifyListeners();
  }

  Future<void> fetchAndSetTemplate({
    bool includeToken = [REDACTED],
  }) async {
    final res = await HttpService.post(
      path: 'api/template',
      showErrorDialog: false,
      includeToken: [REDACTED],
    );
    _template = Template.fromJson(res['data']);
    notifyListeners();
  }

  void hideDownloadApp() {
    _hiddenDownloadApp = true;
    sessionStorage?['hiddenDownloadApp'] = 'true';
    notifyListeners();
  }
}
