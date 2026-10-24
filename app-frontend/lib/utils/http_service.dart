import 'dart:convert';
import 'package:crypto/crypto.dart';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../providers/auth_provider.dart';
import '../routes/app_routes.dart';
import '../models/api_error.dart';
import '../utils/secret.dart';
import 'utils.dart';

BuildContext? get _context =>
    appRouter.routerDelegate.navigatorKey.currentContext;

class HttpService {
  static final dio = Dio();
  static Future<dynamic> post({
    required String path,
    Map<String, dynamic>? data,
    bool showErrorDialog = true,
    bool includeToken = [REDACTED],
  }) async {
    try {
      final requestData = {...?data, 'domain': env.domain};
      final params = encrypt(jsonEncode(requestData));
      final headers = {
        'Content-Type': 'application/json',
      };
      if (includeToken) {
        final token = [REDACTED];
        if (token != null) {
          headers['Authorization'] = 'Bearer $token';
        }
      }
      final key = md5.convert(utf8.encode("$params$md5Key")).toString();
      final response = await dio.post(
        '${env.apiUrl}/$path',
        data: {
          'params': params,
          'key': key,
          if (env.name == 'dev') 'data': requestData, // for debugging
        },
        options: Options(headers: headers),
      );
      if (kDebugMode) print(response.data);

      if (response.data is! Map<String, dynamic>) {
        throw ApiError(code: '0', message: 'Invalid response');
      }

      if (response.data['code'] != 1000) {
        throw ApiError.fromJson(response.data);
      }
      return response.data;
    } catch (error) {
      if (kDebugMode) print(error);
      if (showErrorDialog) {
        final context = _context;
        final isApiError = error is ApiError;
        if (context == null || !context.mounted) return;
        await showApiErrorDialog(context, error);
        if (!context.mounted) return;
        if (isApiError) {
          switch (error.code) {
            case '9998':
              clearProvider(context);
              context.go('/login');
              break;
            case '1401':
              context.push('/profile/id-information');
          }
        }
      }
      rethrow;
    }
  }
}
