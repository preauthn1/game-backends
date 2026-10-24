import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:provider/provider.dart';

import '../../providers/auth_provider.dart';
import '../../providers/member_provider.dart';
import '../../themes/app_colors.dart';
import '../../utils/utils.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_card.dart';
import '../../widgets/base_dialog.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';

class GoogleAuthenticationScreen extends StatefulWidget {
  const GoogleAuthenticationScreen({super.key});

  @override
  State<GoogleAuthenticationScreen> createState() =>
      _GoogleAuthenticationScreenState();
}

class _GoogleAuthenticationScreenState
    extends State<GoogleAuthenticationScreen> {
  late Future<void> _future;
  final TextEditingController _codeController = TextEditingController();
  bool _isSubmitting = false;

  Future<void> _fetchData() async {
    await context.read<AuthProvider>().fetchTwoFactorAuth();
  }

  Future<void> _submit() async {
    setState(() {
      _isSubmitting = true;
    });
    try {
      await context
          .read<AuthProvider>()
          .verifyTwoFactorAuth(_codeController.text);
      if (!mounted) return;
      final member = context.read<MemberProvider>().member;
      if (member != null) {
        member.is2faEnable = "1";
        context.read<MemberProvider>().setMember(member);
      }
      await showDialog(
        context: context,
        builder: (context) {
          return BaseDialog(
            titleText: 'Success',
            message: 'Google authentication has been enabled.',
            onConfirm: () => Navigator.of(context).pop(),
          );
        },
      );
      if (!mounted) return;
      Navigator.of(context).pop();
    } finally {
      setState(() {
        _isSubmitting = false;
      });
    }
  }

  @override
  void initState() {
    _future = _fetchData();
    _codeController.addListener(() {
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _codeController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Google Authentication'),
        ),
        body: FutureBuilder(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            if (snapshot.hasError) {
              return const Center(
                child: Text('Error'),
              );
            }

            final twoFactorAuth = context.read<AuthProvider>().twoFactorAuth;

            return SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: SafeArea(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'For your account security, please record the 16-digit key on paper and keep it in a safe place. If you Iost your phone, you can use this key to recover your Google authenticator.',
                    ),
                    const SizedBox(height: 20),
                    BaseCard(
                      padding: const EdgeInsets.only(left: 16),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(twoFactorAuth?.secret ?? '-'),
                          IconButton(
                            icon: Icon(
                              Icons.copy,
                              color: AppColors.primaryColor,
                            ),
                            onPressed: () {
                              Clipboard.setData(
                                ClipboardData(
                                    text: twoFactorAuth?.secret ?? ''),
                              );
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(
                                  content: Text('Copied to clipboard'),
                                ),
                              );
                            },
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 20),
                    Stack(
                      clipBehavior: Clip.none,
                      children: [
                        BaseCard(
                          padding: const EdgeInsets.only(
                            top: 30,
                            bottom: 30 + 80,
                          ),
                          width: double.infinity,
                          child: Center(
                            child: SvgPicture.memory(
                              imageBytes(twoFactorAuth?.qrCodeImage ?? ''),
                              width: 200,
                            ),
                          ),
                        ),
                        Positioned(
                          bottom: 0,
                          left: 0,
                          right: 0,
                          child: Container(
                            padding: const EdgeInsets.all(20),
                            height: 80,
                            decoration: const BoxDecoration(
                              borderRadius: BorderRadius.only(
                                bottomLeft: Radius.circular(10),
                                bottomRight: Radius.circular(10),
                              ),
                              border: Border.fromBorderSide(
                                BorderSide(
                                  color: Color(0xFF3B363F),
                                ),
                              ),
                              color: Color(0xFF262329),
                            ),
                            child: Container(
                              padding:
                                  const EdgeInsets.symmetric(horizontal: 20),
                              child: const Center(
                                child: Text(
                                  'Add DOEX account to Google Authenticator and input the Key to bind.',
                                  textAlign: TextAlign.center,
                                ),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 20),
                    Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Icon(
                          Icons.help_outline,
                          color: AppColors.primaryColor,
                        ),
                        const SizedBox(width: 8),
                        const Expanded(
                          child: Text(
                            'Google Authenticator is a dynamic password tool, which works like SMS dynamic authentication. After binding, a dynamic authentication key will be generated every 30s, which can be used for security authentication of operations such as withdrawal, and modification of security settings.',
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 20),
                    const Text('How to set'),
                    const Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('1. '),
                        Expanded(
                          child: Text(
                            'Download Google Authenticator to your mobile phone. iOS users log in to the AppStore and search for the “Authenticator”to download Google Authenticator, while Android users login to the App store or use the mobile browser to search for “Google authenticator” to download it.',
                          ),
                        ),
                      ],
                    ),
                    const Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('2. '),
                        Expanded(
                          child: Text(
                            'Scan the QR code displayed on the binding page or manually enter the secret key shown in the red box of the screenshot below.',
                          ),
                        ),
                      ],
                    ),
                    const Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('3. '),
                        Expanded(
                          child: Text(
                            'At this time, the Google Authenticator App in your mobile phone will generate a 6-digit authentication key. You fill in the blank with the key, and click to submit.',
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 20),
                    BaseTextField(
                      controller: _codeController,
                      labelText: 'Code',
                    ),
                    const SizedBox(height: 20),
                    BaseButton(
                      loading: _isSubmitting,
                      text: 'Submit',
                      onPressed: _codeController.text.isEmpty ||
                              _codeController.text.length != 6
                          ? null
                          : () => _submit(),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}
