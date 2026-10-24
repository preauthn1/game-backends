import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../models/member.dart';
import '../../models/withdrawal_bank.dart';
import '../../providers/member_provider.dart';
import '../../providers/wallet_provider.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_card.dart';
import '../../widgets/base_dialog.dart';
import '../../widgets/base_dropdown.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';

class CryptoAddressScreen extends StatefulWidget {
  const CryptoAddressScreen({super.key});

  @override
  State<CryptoAddressScreen> createState() => _CryptoAddressScreenState();
}

class _CryptoAddressScreenState extends State<CryptoAddressScreen> {
  final TextEditingController _addressController = TextEditingController();
  late MemberCrypto? _crypto;
  late Future<void> _future;
  bool _isSubmitting = false;
  WithdrawalBank? _selected;

  Future<void> _fetchData() async {
    await context.read<WalletProvider>().fetchWithdrawalBank();
    if (!mounted) return;
    if (context.read<WalletProvider>().withdrawalCryptoList.isNotEmpty) {
      _selected = context.read<WalletProvider>().withdrawalCryptoList.first;
    }
  }

  bool get _canSubmit {
    return _addressController.text.isNotEmpty && _selected != null;
  }

  void _submit() async {
    final bankId = _selected?.bankId;
    if (bankId == null) return;
    try {
      setState(() {
        _isSubmitting = true;
      });
      await context.read<MemberProvider>().createCryptoAddress(
            bankId: bankId,
            address: _addressController.text,
          );
      if (!mounted) return;
      await Future.wait([
        context.read<MemberProvider>().fetchMember(),
        context.read<WalletProvider>().fetchWithdrawalBankAccount()
      ]);
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) {
          return BaseDialog(
            titleText: 'Success',
            message: 'Your crypto address has been saved',
            onConfirm: () {
              Navigator.of(context).pop();
            },
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
    _crypto = context.read<MemberProvider>().member?.crypto;
    _future = _fetchData();
    _addressController.addListener(() {
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _addressController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Crypto Address'),
        ),
        body: _crypto?.status == "0"
            ? FutureBuilder(
                future: _future,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const Center(
                      child: CircularProgressIndicator(),
                    );
                  }

                  if (snapshot.hasError) {
                    return const Center(
                      child: Text("Error"),
                    );
                  }

                  final list =
                      context.read<WalletProvider>().withdrawalCryptoList;
                  return ScrollableExpandedColumn(
                    children: [
                      Column(
                        children: [
                          BaseDropdown(
                            labelText: "Type",
                            value: _selected,
                            items: list
                                .map(
                                  (e) => DropdownMenuItem(
                                    value: e,
                                    child: Text(e.bankName ?? '-'),
                                  ),
                                )
                                .toList(),
                            onChanged: (value) {
                              setState(() {
                                _selected = value;
                              });
                            },
                          ),
                          const SizedBox(height: 20),
                          BaseTextField(
                            controller: _addressController,
                            labelText: "Crypto Address",
                            hintText: 'Please enter your crypto address',
                          ),
                        ],
                      ),
                      const SizedBox(height: 20),
                      BaseButton(
                        loading: _isSubmitting,
                        text: 'Save',
                        onPressed: _canSubmit ? _submit : null,
                      ),
                    ],
                  );
                },
              )
            : SingleChildScrollView(
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('Type'),
                    const SizedBox(height: 4),
                    BaseCard(
                      width: double.infinity,
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Text(_crypto?.name ?? '-'),
                      ),
                    ),
                    const SizedBox(height: 20),
                    const Text('Crypto Address'),
                    const SizedBox(height: 4),
                    BaseCard(
                      width: double.infinity,
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Text(_crypto?.address ?? '-'),
                      ),
                    ),
                  ],
                ),
              ),
      ),
    );
  }
}
