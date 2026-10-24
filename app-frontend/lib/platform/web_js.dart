import 'dart:js' as js;
import 'dart:html' as html;
import 'package:flutter/material.dart';

import '../models/customer_service.dart';

dynamic get sessionStorage => html.window.sessionStorage;
bool get isAndroidDevice {
  final userAgent = html.window.navigator.userAgent.toLowerCase();
  return userAgent.contains('android');
}
/*
void startTawk() {
  js.context.callMethod('eval', ['window.Tawk_API.start();']);
}

void shutdownTawk() {
  js.context.callMethod('eval', ['window.Tawk_API.shutdown();']);
}

void openTawk({
  required BuildContext context,
  required String hash,
  required String userId,
  required String name,
}) {
  js.context.callMethod('eval', [
    '''
function loginToTawk() {
  if (window.loading || window.Tawk_API.isChatMaximized()) {
    return;
  }
  window.loading = true;
  window.Tawk_API.login(
    {
      hash: '$hash',
      userId: '$userId',
      name: '$name',
    },
    function (error) {
      window.loading = false;
      if (error) {
        alert('Error: ' + error);
      } else {
        window.Tawk_API.maximize();
      }
    }
  );
}

loginToTawk();
    '''
  ]);
}

void hideTawk() {
  js.context.callMethod('eval', ['window.Tawk_API.minimize();']);
}
*/

chatLogin({
  required CustomerService data,
}) {
  js.context.callMethod('eval', [
    '''
ssq.push('setLoginInfo', {
  user_id: '${data.userId}',
  user_name: '${data.username}',
  phone: '${data.phone}',
});
    '''
  ]);
}

chatClear() {
  js.context.callMethod('eval', [
    '''
ssq.push('clearUser');
    '''
  ]);
}

chatOpen({
  required BuildContext context,
  required CustomerService? data,
}) {
  js.context.callMethod('eval', [
    '''
ssq.push('chatOpen');
    '''
  ]);
}

chatClose() {
  js.context.callMethod('eval', [
    '''
ssq.push('chatClose');
    '''
  ]);
}

void setMatomoUserId(String userId) {
  js.context.callMethod('eval', [
    '''
var _uid = "$userId";
if (_uid.trim().length > 0) {
  _paq.push(["setUserId", _uid]);
}
    '''
  ]);
}
