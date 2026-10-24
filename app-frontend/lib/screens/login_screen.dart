import '../platform/web_js.dart'
    if (example.invalid) '../platform/mobile_stub.dart';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../providers/app_provider.dart';
import '../providers/auth_provider.dart';
import '../themes/app_colors.dart';
import '../widgets/base_button.dart';
import '../widgets/base_text_button.dart';
import '../widgets/base_text_field.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/scrollable_expanded_column.dart';
import '../widgets/version_text.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  bool _loading = false;

  Future<void> _submit() async {
    setState(() {
      _loading = true;
    });
    FocusScope.of(context).unfocus();

    try {
      await context.read<AuthProvider>().login(
            username: _usernameController.text,
            password: [REDACTED],
          );
      if (!mounted) return;
      if (context.read<AuthProvider>().token != null) {
        context.go('/home');
      }
    } finally {
      setState(() {
        _loading = false;
      });
    }
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  void initState() {
    context.read<AppProvider>().fetchAndSetTemplate(includeToken: [REDACTED];
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Login'),
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
          children: [
            const SizedBox(),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  'Start your\nfantasy journey\nwith Miner.',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: 40),
                BaseTextField(
                  controller: _usernameController,
                  labelText: 'Account',
                  hintText: 'User name, email or mobile number',
                  onFieldSubmitted: (p0) => _submit(),
                ),
                const SizedBox(height: 20),
                BaseTextField(
                  controller: _passwordController,
                  labelText: 'Password',
                  obscureText: true,
                  hintText: 'Please enter the password',
                  onFieldSubmitted: (p0) => _submit(),
                ),
                /*
                const SizedBox(height: 20),
                BaseTextButton(
                  onPressed: () {
                    context.push('/reset-password');
                  },
                  child: const Text('Forgot Password?'),
                ),
                */
                const SizedBox(height: 20),
              ],
            ),
            Column(
              children: [
                BaseButton(
                  onPressed: _submit,
                  text: 'Login',
                  loading: _loading,
                ),
                const SizedBox(height: 20),
                Row(
                  children: [
                    const Text(
                      'Need an entity account?',
                      style: TextStyle(fontSize: 16),
                    ),
                    BaseTextButton(
                      onPressed: () {
                        context.push('/register');
                      },
                      child: Text(
                        'Sign up',
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
          ],
        ),
      ),
    );
  }
}
