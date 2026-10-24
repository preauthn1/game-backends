import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../providers/member_provider.dart';
import '../../providers/wallet_provider.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_dialog.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';
import '../../widgets/send_code_button.dart';

class ResetFundPasswordScreen extends StatefulWidget {
  const ResetFundPasswordScreen({super.key});

  @override
  State<ResetFundPasswordScreen> createState() =>
      _ResetFundPasswordScreenState();
}

class _ResetFundPasswordScreenState extends State<ResetFundPasswordScreen> {
  final _codeController = TextEditingController();
  final _newFundPasswordController = TextEditingController();
  final _confirmNewFundPasswordController = TextEditingController();
  bool _submitting = false;

  @override
  void dispose() {
    _newFundPasswordController.dispose();
    _confirmNewFundPasswordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Reset Fund Password'),
        ),
        body: ScrollableExpandedColumn(
          children: [
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                BaseTextField(
                  controller: _codeController,
                  labelText: "SMS Code",
                  hintText: 'Please enter SMS code',
                  suffix: SendCodeButton(
                    countryId: '',
                    mobile: context.read<MemberProvider>().member?.mobile ?? '',
                  ),
                ),
                const SizedBox(height: 20),
                BaseTextField(
                  controller: _newFundPasswordController,
                  obscureText: true,
                  labelText: "New Fund Password",
                  hintText: 'Please enter new fund password',
                ),
                const SizedBox(height: 20),
                BaseTextField(
                  controller: _confirmNewFundPasswordController,
                  obscureText: true,
                  labelText: "Confirm Fund Password",
                  hintText: 'Please confirm fund password',
                ),
              ],
            ),
            const SizedBox(height: 20),
            BaseButton(
              loading: _submitting,
              onPressed: () async {
                setState(() {
                  _submitting = true;
                });
                try {
                  await context.read<WalletProvider>().resetFundPassword(
                        smsCode: _codeController.text,
                        newFundPassword: [REDACTED],
                        confirmNewFundPassword:
                            [REDACTED],
                      );
                  if (!context.mounted) return;
                  await showDialog(
                    context: context,
                    builder: (context) => BaseDialog(
                      titleText: 'Success',
                      message: 'Fund password reset successfully',
                      onConfirm: () {
                        Navigator.of(context).pop();
                      },
                    ),
                  );
                  if (!context.mounted) return;
                  Navigator.of(context).pop();
                } finally {
                  setState(() {
                    _submitting = false;
                  });
                }
              },
              text: 'Save',
            ),
          ],
        ),
      ),
    );
  }
}
