import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:miner/providers/auth_provider.dart';
import 'package:provider/provider.dart';

import '../../utils/utils.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_dialog.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';

class LoginPasswordScreen extends StatefulWidget {
  const LoginPasswordScreen({super.key});

  @override
  State<LoginPasswordScreen> createState() => _LoginPasswordScreenState();
}

class _LoginPasswordScreenState extends State<LoginPasswordScreen> {
  final TextEditingController _oldPasswordController = TextEditingController();
  final TextEditingController _newPasswordController = TextEditingController();
  final TextEditingController _confirmNewPasswordController =
      TextEditingController();
  bool _isSubmitting = false;

  bool get _canSubmit =>
      _oldPasswordController.text.isNotEmpty &&
      _newPasswordController.text.isNotEmpty &&
      _confirmNewPasswordController.text.isNotEmpty;
  void _submit() async {
    setState(() {
      _isSubmitting = true;
    });
    try {
      await context.read<AuthProvider>().changePassword(
            oldPassword: [REDACTED],
            newPassword: [REDACTED],
            newPasswordConfirmation: _confirmNewPasswordController.text,
          );
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) => BaseDialog(
          titleText: 'Success',
          message: 'Password changed successfully, please login again.',
          onConfirm: () => Navigator.of(context).pop(),
        ),
      );
      if (!mounted) return;
      clearProvider(context);
      context.go('/login');
    } finally {
      setState(() {
        _isSubmitting = false;
      });
    }
  }

  @override
  void initState() {
    _oldPasswordController.addListener(() {
      setState(() {});
    });
    _newPasswordController.addListener(() {
      setState(() {});
    });
    _confirmNewPasswordController.addListener(() {
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _oldPasswordController.dispose();
    _newPasswordController.dispose();
    _confirmNewPasswordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Login Password'),
        ),
        body: ScrollableExpandedColumn(
          children: [
            Column(
              children: [
                BaseTextField(
                  controller: _oldPasswordController,
                  labelText: "Old Password",
                  hintText: 'Please enter your old password',
                  obscureText: true,
                ),
                const SizedBox(height: 20),
                BaseTextField(
                  controller: _newPasswordController,
                  labelText: "New Password",
                  hintText: 'Please enter your new password',
                  obscureText: true,
                ),
                const SizedBox(height: 20),
                BaseTextField(
                  controller: _confirmNewPasswordController,
                  labelText: "Confirm New Password",
                  hintText: 'Please confirm your new password',
                  obscureText: true,
                ),
              ],
            ),
            const SizedBox(height: 20),
            BaseButton(
              loading: _isSubmitting,
              onPressed: _canSubmit ? _submit : null,
              text: 'Save',
            ),
          ],
        ),
      ),
    );
  }
}
