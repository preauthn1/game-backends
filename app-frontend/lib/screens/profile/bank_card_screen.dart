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

class BankCardScreen extends StatefulWidget {
  const BankCardScreen({super.key});

  @override
  State<BankCardScreen> createState() => _BankCardScreenState();
}

class _BankCardScreenState extends State<BankCardScreen> {
  late Future<void> _future;
  late MemberBank? _bank;
  final TextEditingController _accountNoController = TextEditingController();
  final TextEditingController _accountNameController = TextEditingController();
  WithdrawalBank? _selected;
  bool _isSubmitting = false;

  Future<void> _fetchData() async {
    await context.read<WalletProvider>().fetchWithdrawalBank();
    if (!mounted) return;
    if (context.read<WalletProvider>().withdrawalBankList.isNotEmpty) {
      _selected = context.read<WalletProvider>().withdrawalBankList.first;
    }
  }

  bool get _canSubmit {
    return _accountNoController.text.isNotEmpty &&
        _accountNameController.text.isNotEmpty &&
        _selected != null;
  }

  Future<void> _submit() async {
    final bankId = _selected?.bankId;
    if (bankId == null) return;
    try {
      setState(() {
        _isSubmitting = true;
      });

      await context.read<MemberProvider>().createBank(
            bankId: bankId,
            accountNo: _accountNoController.text,
            accountName: _accountNameController.text,
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
            message: 'Your bank card has been saved',
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
    _bank = context.read<MemberProvider>().member?.bank;
    _future = _fetchData();
    _accountNameController.addListener(() {
      setState(() {});
    });
    _accountNoController.addListener(() {
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _accountNameController.dispose();
    _accountNoController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Bank Card'),
        ),
        body: _bank?.status == "0"
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
                      context.read<WalletProvider>().withdrawalBankList;

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
                            controller: _accountNoController,
                            labelText: "Card Number",
                            hintText: 'Please enter the card number',
                          ),
                          const SizedBox(height: 20),
                          BaseTextField(
                            controller: _accountNameController,
                            labelText: "Name",
                            hintText: 'Please enter name',
                          ),
                          const SizedBox(height: 20),
                        ],
                      ),
                      const SizedBox(height: 20),
                      BaseButton(
                        loading: _isSubmitting,
                        onPressed: _canSubmit ? _submit : null,
                        text: 'Save',
                      ),
                    ],
                  );
                },
              )
            : Padding(
                padding: const EdgeInsets.all(20),
                child: BaseCard(
                  height: 94,
                  padding: const EdgeInsets.all(20),
                  child: Row(
                    children: [
                      // Card Ratio
                      Flexible(
                        flex: 1,
                        child: Image.network(
                          _bank?.image ?? '',
                          width: 80,
                          height: 54,
                          fit: BoxFit.contain,
                        ),
                      ),
                      const SizedBox(width: 10),
                      Flexible(
                        flex: 3,
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(_bank?.name ?? '-'),
                            const SizedBox(height: 6),
                            Text(_bank?.account ?? '-'),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
      ),
    );
  }
}
