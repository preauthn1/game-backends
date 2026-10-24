import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../providers/member_provider.dart';
import '../../providers/wallet_provider.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';

class CreateFundPasswordScreen extends StatefulWidget {
  const CreateFundPasswordScreen({super.key});

  @override
  State<CreateFundPasswordScreen> createState() =>
      _CreateFundPasswordScreenState();
}

class _CreateFundPasswordScreenState extends State<CreateFundPasswordScreen> {
  final _fundPasswordController = TextEditingController();
  final _confirmFundPasswordController = TextEditingController();
  bool _submitting = false;
  @override
  void dispose() {
    _fundPasswordController.dispose();
    _confirmFundPasswordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Create Fund Password'),
        ),
        body: ScrollableExpandedColumn(
          children: [
            Column(
              children: [
                BaseTextField(
                  controller: _fundPasswordController,
                  obscureText: true,
                  labelText: "Fund Password",
                  hintText: 'Please enter fund password',
                ),
                const SizedBox(height: 20),
                BaseTextField(
                  controller: _confirmFundPasswordController,
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
                  await context.read<WalletProvider>().createFundPassword(
                        fundPassword: [REDACTED],
                        confirmFundPassword:
                            [REDACTED],
                      );

                  if (!context.mounted) return;
                  final member = context.read<MemberProvider>().member;
                  if (member != null) {
                    member.fundPasswordStatus = "1";
                    context.read<MemberProvider>().setMember(member);
                  }
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('Create fund password successfully'),
                    ),
                  );
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
