import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../providers/wallet_provider.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_dialog.dart';
import '../../widgets/base_text_button.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';

class ChangeFundPasswordScreen extends StatefulWidget {
  const ChangeFundPasswordScreen({super.key});

  @override
  State<ChangeFundPasswordScreen> createState() =>
      _ChangeFundPasswordScreenState();
}

class _ChangeFundPasswordScreenState extends State<ChangeFundPasswordScreen> {
  final _oldFundPasswordController = TextEditingController();
  final _newFundPasswordController = TextEditingController();
  final _confirmNewFundPasswordController = TextEditingController();
  bool _submitting = false;
  @override
  void dispose() {
    _oldFundPasswordController.dispose();
    _newFundPasswordController.dispose();
    _confirmNewFundPasswordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Change Fund Password'),
        ),
        body: ScrollableExpandedColumn(
          children: [
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                BaseTextField(
                  controller: _oldFundPasswordController,
                  obscureText: true,
                  labelText: "Old Fund Password",
                  hintText: 'Please enter old fund password',
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
                const SizedBox(height: 10),
                /*
                BaseTextButton(
                  onPressed: () {
                    context.replace('/profile/reset-fund-password');
                  },
                  child: const Text(
                    'Forget Fund Password?',
                    textAlign: TextAlign.left,
                  ),
                ),
                */
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
                  await context.read<WalletProvider>().changeFundPassword(
                        oldFundPassword: [REDACTED],
                        newFundPassword: [REDACTED],
                        confirmNewFundPassword:
                            [REDACTED],
                      );

                  if (!context.mounted) return;
                  await showDialog(
                    context: context,
                    builder: (context) => BaseDialog(
                      titleText: 'Success',
                      message: 'Fund password changed successfully',
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
