import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/customer_service.dart';
import '../models/captcha.dart';
import '../models/country.dart';
import '../models/two_factor_auth.dart';
import '../utils/http_service.dart';
import '../utils/utils.dart';

class AuthProvider with ChangeNotifier {
  String? _token;
  List<Country> _countryList = [];
  TwoFactorAuth? _twoFactorAuth;
  CustomerService? _customerService;

  String? get token => _token;
  List<Country> get countryList => _countryList;
  TwoFactorAuth? get twoFactorAuth => _twoFactorAuth;
  CustomerService? get customerService => _customerService;

  Future<void> clear() async {
    _countryList = [];
    _customerService = null;
    _twoFactorAuth = null;
    await removeToken();
    notifyListeners();
  }

  Future<void> login({
    required String username,
    required String password,
  }) async {
    final res = await HttpService.post(
      path: 'api/login',
      data: {
        'username': username,
        'password': password,
      },
    );
    await setToken(res['data']['token']);
    notifyListeners();
  }

  Future<void> setToken(String token) async {
    _token = [REDACTED];
    if (kIsWeb) {
      final prefs = SharedPreferencesAsync();
      await prefs.setString('token', token);
    } else {
      await secureStorage.write(key: 'token', value: token);
    }
  }

  Future<void> readAndSetToken() async {
    String? token;
    if (kIsWeb) {
      final prefs = SharedPreferencesAsync();
      token = [REDACTED] prefs.getString('token');
    } else {
      token = [REDACTED] secureStorage.read(key: 'token');
    }
    if (token != null) {
      _token = [REDACTED];
    }
    notifyListeners();
  }

  Future<void> removeToken() async {
    _token = [REDACTED];
    if (kIsWeb) {
      final prefs = SharedPreferencesAsync();
      await prefs.remove('token');
    } else {
      await secureStorage.delete(key: 'token');
    }
  }

  Future<void> register({
    required String username,
    required String password,
    required String passwordConfirmation,
    required String mobile,
    required String countryId,
    required String countryCode,
    String? referralCode,
    String? smsCode,
    String? captchaKey,
    String? captchaCode,
  }) async {
    final data = {
      'username': username,
      'password': password,
      'password_confirmation': passwordConfirmation,
      'mobile': mobile,
      'country_id': countryId,
      'country_code': countryCode,
      if (referralCode != null) 'referral_code': referralCode,
      if (smsCode != null) 'sms_code': smsCode,
      if (captchaKey != null) 'captcha_key': captchaKey,
      if (captchaCode != null) 'captcha_code': captchaCode,
    };
    await HttpService.post(path: 'api/register', data: data);
  }

  // 變更登入密碼
  Future<void> changePassword({
    required String oldPassword,
    required String newPassword,
    required String newPasswordConfirmation,
  }) async {
    await HttpService.post(path: 'api/member/changePassword', data: {
      'old_password': oldPassword,
      'new_password': newPassword,
      'new_password_confirmation': newPasswordConfirmation,
    });
  }

  //取得國碼
  // type: 0=配置的國碼列表; 1=支援簡訊的國碼列表;
  Future<void> fetchCountryList({
    int? type,
  }) async {
    final res = await HttpService.post(
      path: "api/country/list",
      data: {
        if (type != null) 'type': type,
      },
    );
    _countryList = List<Country>.from(
      res['data']['list'].map(
        (x) => Country.fromJson(x),
      ),
    );
  }

  //發送手機驗證碼
  Future<void> fetchSmsCode({
    required String countryId,
    required String mobile,
  }) async {
    await HttpService.post(
      path: 'api/verification/getSmsCode',
      data: {
        'type': 8,
        'verify_by': 1,
        'country_id': countryId,
        'mobile': mobile,
      },
    );
  }

  //獲取驗證碼
  Future<Captcha> fetchCaptcha() async {
    final res = await HttpService.post(
      path: 'api/verification/getCaptcha',
    );
    return Captcha.fromJson(res['data']);
  }

  //2FA驗證
  Future<void> fetchTwoFactorAuth() async {
    final res = await HttpService.post(
      path: "api/member/enable2fa",
    );
    _twoFactorAuth = TwoFactorAuth.fromJson(res['data']);
  }

  //2FA驗證碼
  Future<void> verifyTwoFactorAuth(String code) async {
    await HttpService.post(
      path: "api/member/verify2fa",
      data: {
        'gauth_code': code,
      },
    );
  }

  //獲取客服資訊
  Future<void> fetchCustomerService() async {
    final res = await HttpService.post(path: 'api/tawk/getSSLogin');
    _customerService = CustomerService.fromJson(res['data']);
    notifyListeners();
  }
}
