import 'package:flutter/material.dart';
import 'package:flutter_inappwebview/flutter_inappwebview.dart';

import '../models/customer_service.dart';
import '../utils/utils.dart';

dynamic get sessionStorage => null;
bool get isAndroidDevice => false;
/*
void startTawk() {}

void shutdownTawk() {}

void openTawk({
  required BuildContext context,
  required String hash,
  required String userId,
  required String name,
}) {
  showModalBottomSheet(
    context: context,
    isScrollControlled: true,
    enableDrag: false,
    builder: (context) {
      final url = '${env.apiUrl}/tawk?hash=$hash&userId=$userId&name=$name';
      return FractionallySizedBox(
        heightFactor: 0.8,
        child: InAppWebView(
          initialUrlRequest: URLRequest(
            url: WebUri(url),
          ),
          onWebViewCreated: (controller) {
            controller.addJavaScriptHandler(
              handlerName: "sendToFlutter",
              callback: (args) {
                var data = args[0];
                if (data['event'] == 'CHAT_MINIMIZED') {
                  Navigator.of(context).pop();
                }
              },
            );
          },
        ),
      );
    },
  );
}

void hideTawk() {}
*/

chatLogin({
  required CustomerService data,
}) {}

chatClear() {}

chatOpen({
  required BuildContext context,
  required CustomerService? data,
}) {
  showModalBottomSheet(
    context: context,
    isScrollControlled: true,
    enableDrag: false,
    builder: (context) {
      final url = data == null
          ? '${env.apiUrl}/ssq'
          : '${env.apiUrl}/ssq?userId=${data.userId}&username=${data.username}&phone=${data.phone}';
      return FractionallySizedBox(
        heightFactor: 0.8,
        child: InAppWebView(
          initialUrlRequest: URLRequest(
            url: WebUri(url),
          ),
          onWebViewCreated: (controller) {
            controller.addJavaScriptHandler(
              handlerName: "sendToFlutter",
              callback: (args) {
                var data = args[0];
                if (data['event'] == 'CHAT_MINIMIZED') {
                  Navigator.of(context).pop();
                }
              },
            );
          },
        ),
      );
    },
  );
}

chatClose() {}

void setMatomoUserId(String userId) {}
