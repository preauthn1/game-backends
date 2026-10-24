import 'package:flutter/foundation.dart';

class Environment {
  final String name;
  final String apiUrl;
  final String apiMd5Key;
  final String apiAesKey;
  final String apiIv;
  final String _domain;

  Environment({
    required this.name,
    required this.apiUrl,
    required this.apiMd5Key,
    required this.apiAesKey,
    required this.apiIv,
    required String domain,
  }) : _domain = domain;

  String get domain {
    if (kIsWeb) {
      return Uri.base.host;
    }
    return _domain;
  }
}
