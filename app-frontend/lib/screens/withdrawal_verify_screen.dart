import 'package:flutter/material.dart';
import 'package:miner/providers/wallet_provider.dart';
import 'package:provider/provider.dart';

import '../widgets/base_button.dart';
import '../widgets/base_dialog.dart';
import '../widgets/base_text_field.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/scrollable_expanded_column.dart';

class WithdrawalVerifyScreen extends StatefulWidget {
  const WithdrawalVerifyScreen({super.key});

  @override
  State<WithdrawalVerifyScreen> createState() => _WithdrawalVerifyScreenState();
}

class _WithdrawalVerifyScreenState extends State<WithdrawalVerifyScreen> {
  final TextEditingController _fundPasswordController = TextEditingController();
  final TextEditingController _googleAuthController = TextEditingController();
  bool _isSubmitting = false;
  bool get _canSubmit {
    return _fundPasswordController.text.isNotEmpty &&
        _googleAuthController.text.isNotEmpty;
  }

  Future<void> _submit() async {
    setState(() {
      _isSubmitting = true;
    });

    try {
      final walletProvider = context.read<WalletProvider>();
      final data = walletProvider.withdrawalCreate;
      data?.fundPassword = [REDACTED];
      data?.gauthCode = _googleAuthController.text;
      walletProvider.setWithdrawalCreate(data);
      await context.read<WalletProvider>().createWithdrawal();
      if (!mounted) return;
      context.read<WalletProvider>().setWithdrawalCreate(null);
      await context.read<WalletProvider>().fetchBalance();
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) => BaseDialog(
          titleText: 'Success',
          message: 'Your withdrawal request has been submitted',
          onConfirm: () {
            Navigator.of(context).pop();
          },
        ),
      );
      if (!mounted) return;
      Navigator.of(context).pop();
      Navigator.of(context).pop();
    } finally {
      setState(() {
        _isSubmitting = false;
      });
    }
  }

  @override
  void initState() {
    _fundPasswordController.addListener(() {
      setState(() {});
    });
    _googleAuthController.addListener(() {
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _fundPasswordController.dispose();
    _googleAuthController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Withdrawal'),
        ),
        body: ScrollableExpandedColumn(
          children: [
            Column(
              children: [
                BaseTextField(
                  controller: _fundPasswordController,
                  labelText: "Fund Password",
                  hintText: 'Please enter the fund password',
                  obscureText: true,
                ),
                const SizedBox(height: 20),
                BaseTextField(
                  controller: _googleAuthController,
                  labelText: "Google Authenticator Code",
                  hintText: 'Please enter Google verification code',
                ),
              ],
            ),
            const SizedBox(height: 20),
            BaseButton(
              loading: _isSubmitting,
              text: 'Submit',
              onPressed: _canSubmit ? _submit : null,
            ),
          ],
        ),
      ),
    );
  }
}
