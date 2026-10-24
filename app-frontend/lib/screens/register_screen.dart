import 'package:flutter/services.dart';
import 'package:miner/models/template.dart';

import '../platform/web_js.dart'
    if (example.invalid) '../platform/mobile_stub.dart';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:miner/utils/utils.dart';
import 'package:provider/provider.dart';

import '../models/captcha.dart';
import '../models/country.dart';
import '../providers/app_provider.dart';
import '../providers/auth_provider.dart';
import '../themes/app_colors.dart';
import '../widgets/base_button.dart';
import '../widgets/base_dropdown.dart';
import '../widgets/base_text_button.dart';
import '../widgets/base_text_field.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/send_code_button.dart';
import '../widgets/version_text.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final TextEditingController _confirmPasswordController =
      TextEditingController();
  final TextEditingController _mobileController = TextEditingController();
  final TextEditingController _verificationCodeController =
      TextEditingController();
  final TextEditingController _referralCodeController = TextEditingController();
  final TextEditingController _captchaTextController = TextEditingController();

  Template? _template;
  Captcha? _captcha;
  Uint8List? _captchaImage;
  bool _fetchingCaptcha = false;
  bool _submitting = false;
  Country? _selectedCountry;

  Future<void> _submit(BuildContext context) async {
    if (_submitting) return;
    FocusScope.of(context).unfocus();
    setState(() {
      _submitting = true;
    });

    final countryCode = _selectedCountry?.countryCode.toString() ?? "";
    final authProvider = context.read<AuthProvider>();
    try {
      await authProvider.register(
        username: _usernameController.text,
        password: [REDACTED],
        passwordConfirmation: _confirmPasswordController.text,
        countryId: _selectedCountry?.countryId ?? "",
        countryCode: countryCode,
        mobile: _mobileController.text,
        referralCode: _referralCodeController.text.isNotEmpty
            ? _referralCodeController.text
            : null,
        smsCode: _template?.smsVerify ?? false
            ? _verificationCodeController.text
            : null,
        captchaKey:
            _template?.captchaVerify ?? false ? _captcha?.key ?? "" : null,
        captchaCode: _template?.captchaVerify ?? false
            ? _captchaTextController.text
            : null,
      );
      if (!context.mounted) return;
      await authProvider.login(
        username: _usernameController.text,
        password: [REDACTED],
      );
      if (!context.mounted) return;
      if (authProvider.token != null) {
        context.go('/home');
      }
    } catch (e) {
      _refreshCaptcha();
    } finally {
      setState(() {
        _submitting = false;
      });
    }
  }

  Future<void> _refreshCaptcha() async {
    setState(() {
      _fetchingCaptcha = true;
    });

    try {
      _captchaTextController.clear();
      _captcha = await context.read<AuthProvider>().fetchCaptcha();
      setState(() {
        _captchaImage = imageBytes(_captcha!.imageUrl!);
      });
    } finally {
      setState(() {
        _fetchingCaptcha = false;
      });
    }
  }

  Future<void> _getCountryList() async {
    await context
        .read<AuthProvider>()
        .fetchCountryList(type: _template?.smsVerify ?? false ? 1 : 0);
    setState(() {
      _selectFirstCountry();
    });
  }

  void _selectFirstCountry() {
    if (context.read<AuthProvider>().countryList.isNotEmpty) {
      _selectedCountry = context.read<AuthProvider>().countryList.first;
    }
  }

  @override
  void initState() {
    _template = context.read<AppProvider>().template;
    _mobileController.addListener(() {
      setState(() {});
    });
    _refreshCaptcha();
    _getCountryList();
    _selectFirstCountry();
    super.initState();
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    _mobileController.dispose();
    _verificationCodeController.dispose();
    _referralCodeController.dispose();
    _captchaTextController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Register'),
          actions: [
            IconButton(
              icon: const Icon(Icons.support_agent),
              onPressed: () {
                chatOpen(
                  context: context,
                  data: null,
                );
              },
            ),
          ],
        ),
        body: SingleChildScrollView(
          child: SafeArea(
            child: Container(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  BaseTextField(
                    labelText: 'User name',
                    hintText: 'Please enter user name',
                    controller: _usernameController,
                    inputFormatters: [
                      FilteringTextInputFormatter.allow(RegExp(r'[a-zA-Z0-9]')),
                    ],
                  ),
                  const SizedBox(height: 20),
                  BaseTextField(
                    labelText: 'Password',
                    hintText: 'Please enter the password',
                    controller: _passwordController,
                    obscureText: true,
                    inputFormatters: [
                      FilteringTextInputFormatter.allow(RegExp(r'[a-zA-Z0-9]')),
                    ],
                  ),
                  const SizedBox(height: 20),
                  BaseTextField(
                    labelText: 'Confirm Password',
                    controller: _confirmPasswordController,
                    obscureText: true,
                    hintText: 'Please enter confirm password',
                    inputFormatters: [
                      FilteringTextInputFormatter.allow(RegExp(r'[a-zA-Z0-9]')),
                    ],
                  ),
                  const SizedBox(height: 20),
                  if (context.read<AuthProvider>().countryList.isEmpty)
                    const Center(child: CircularProgressIndicator())
                  else
                    BaseDropdown(
                      labelText: "Country",
                      items: context
                          .watch<AuthProvider>()
                          .countryList
                          .map(
                            (e) => DropdownMenuItem(
                              value: e,
                              child: Text(e.countryName ?? ""),
                            ),
                          )
                          .toList(),
                      onChanged: (value) {
                        setState(() {
                          _selectedCountry = value as Country;
                        });
                      },
                      value: _selectedCountry,
                    ),
                  const SizedBox(height: 20),
                  BaseTextField(
                    labelText: 'Mobile Number',
                    controller: _mobileController,
                    hintText: 'Please enter mobile number',
                    prefix: _selectedCountry == null
                        ? null
                        : Row(
                            children: [
                              Text(
                                '+${_selectedCountry!.countryCode}',
                                style: const TextStyle(fontSize: 16),
                              ),
                              const SizedBox(width: 4),
                            ],
                          ),
                  ),
                  if (_template?.smsVerify ?? false) const SizedBox(height: 20),
                  if (_template?.smsVerify ?? false)
                    BaseTextField(
                      labelText: 'Verification Code',
                      controller: _verificationCodeController,
                      hintText: 'Please enter verification code',
                      suffix: SendCodeButton(
                        countryId: _selectedCountry?.countryId ?? "",
                        mobile: _mobileController.text,
                      ),
                    ),
                  const SizedBox(height: 20),
                  BaseTextField(
                    labelText: 'Referral Code',
                    controller: _referralCodeController,
                    hintText: 'Please enter referral code',
                  ),
                  if (_template?.captchaVerify ?? false)
                    const SizedBox(height: 20),
                  if (_template?.captchaVerify ?? false)
                    BaseTextField(
                      labelText: 'Captcha',
                      controller: _captchaTextController,
                      hintText: 'Please enter captcha code',
                      suffix: _fetchingCaptcha
                          ? const SizedBox(
                              height: 24,
                              width: 24,
                              child: CircularProgressIndicator(),
                            )
                          : _captchaImage != null
                              ? GestureDetector(
                                  onTap: _refreshCaptcha,
                                  child:
                                      Image.memory(_captchaImage!, width: 120),
                                )
                              : null,
                    ),
                  const SizedBox(height: 20),
                  BaseButton(
                    text: 'Sign Up',
                    loading: _submitting,
                    onPressed: () => _submit(context),
                  ),
                  const SizedBox(height: 20),
                  Row(
                    children: [
                      const Text(
                        'Already have an account?',
                        style: TextStyle(fontSize: 16),
                      ),
                      BaseTextButton(
                        onPressed: () {
                          context.go('/login');
                        },
                        child: Text(
                          'Login',
                          style: TextStyle(
                            color: AppColors.primaryColor,
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ],
                  ),
                  if (!kIsWeb)
                    const Row(
                      mainAxisAlignment: MainAxisAlignment.end,
                      children: [VersionText()],
                    ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
