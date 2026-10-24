import '../platform/web_js.dart'
    if (example.invalid) '../platform/mobile_stub.dart';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../models/country.dart';
import '../providers/auth_provider.dart';
import '../widgets/base_button.dart';
import '../widgets/base_dropdown.dart';
import '../widgets/base_text_field.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/scrollable_expanded_column.dart';
import '../widgets/send_code_button.dart';

class ResetPasswordScreen extends StatefulWidget {
  const ResetPasswordScreen({super.key});

  @override
  State<ResetPasswordScreen> createState() => _ResetPasswordScreenState();
}

class _ResetPasswordScreenState extends State<ResetPasswordScreen> {
  final _mobileController = TextEditingController();
  final _verificationCodeController = TextEditingController();
  final _newPasswordController = TextEditingController();
  final _confirmNewPasswordController = TextEditingController();

  bool _loadingCountry = false;
  Country? _selectedCountry;
  int _step = 1;

  Future<void> _getCountryList() async {
    setState(() {
      _loadingCountry = true;
    });
    try {
      await context.read<AuthProvider>().fetchCountryList(type: 1);
      if (!mounted) return;
      if (context.read<AuthProvider>().countryList.isNotEmpty) {
        setState(() {
          _selectedCountry = context.read<AuthProvider>().countryList.first;
        });
      }
    } finally {}

    setState(() {
      _loadingCountry = false;
    });
  }

  @override
  void initState() {
    _mobileController.addListener(() {
      setState(() {});
    });
    _verificationCodeController.addListener(() {
      setState(() {});
    });
    _newPasswordController.addListener(() {
      setState(() {});
    });
    _confirmNewPasswordController.addListener(() {
      setState(() {});
    });
    _getCountryList();
    super.initState();
  }

  @override
  void dispose() {
    _mobileController.dispose();
    _verificationCodeController.dispose();
    _newPasswordController.dispose();
    _confirmNewPasswordController.dispose();
    super.dispose();
  }

  List<Widget> _buildStep1() {
    return [
      Column(
        children: [
          if (_loadingCountry)
            const SizedBox(
              height: 20,
              width: 20,
              child: CircularProgressIndicator(),
            ),
          if (!_loadingCountry)
            BaseDropdown(
              labelText: "Country",
              items: context
                  .read<AuthProvider>()
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
            controller: _mobileController,
            labelText: 'Mobile Number',
            hintText: 'Please enter mobile number',
          ),
          const SizedBox(height: 20),
          BaseTextField(
            controller: _verificationCodeController,
            labelText: 'Verification Code',
            hintText: 'Please enter verification code',
            suffix: SendCodeButton(
              countryId: _selectedCountry?.countryId ?? '',
              mobile: _mobileController.text,
            ),
          ),
          const SizedBox(height: 20),
        ],
      ),
      BaseButton(
        onPressed: _mobileController.text.isEmpty ||
                _verificationCodeController.text.isEmpty
            ? null
            : () {
                setState(() {
                  _step = 2;
                });
              },
        text: 'Next',
      ),
    ];
  }

  List<Widget> _buildStep2() {
    return [
      Column(
        children: [
          BaseTextField(
            controller: _newPasswordController,
            labelText: 'New Password',
            obscureText: true,
            hintText: 'Please enter your mobile number',
          ),
          const SizedBox(height: 20),
          BaseTextField(
            controller: _confirmNewPasswordController,
            labelText: 'Confirm New Password',
            obscureText: true,
            hintText: 'Please enter your mobile number',
          ),
          const SizedBox(height: 20),
        ],
      ),
      BaseButton(
        onPressed: _newPasswordController.text.isEmpty ||
                _confirmNewPasswordController.text.isEmpty
            ? null
            : () {
                context.go('/login');
              },
        text: 'Confirm',
      ),
    ];
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Reset Password'),
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
        body: ScrollableExpandedColumn(
          children: _step == 1 ? _buildStep1() : _buildStep2(),
        ),
      ),
    );
  }
}
