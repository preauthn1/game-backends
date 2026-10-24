import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../models/top_up_bank.dart';
import '../providers/wallet_provider.dart';
import '../utils/utils.dart';
import 'base_button.dart';
import 'base_dropdown.dart';
import 'base_text_field.dart';
import 'base_card.dart';
import '../themes/app_colors.dart';

class TopUpBankSection extends StatefulWidget {
  final List<TopUpBank> list;
  const TopUpBankSection({required this.list, super.key});

  @override
  State<TopUpBankSection> createState() => _TopUpBankSectionState();
}

class _TopUpBankSectionState extends State<TopUpBankSection> {
  final TextEditingController _amountController = TextEditingController();
  final TextEditingController _convertedAmountController =
      TextEditingController();
  final TextEditingController _cardholderController = TextEditingController();
  final amountList = [100, 500, 1000, 3000, 5000, 10000, 30000, 50000];

  TopUpBank? _selected;
  bool _isSubmitting = false;

  bool get _canSubmit {
    final amount = num.tryParse(_amountController.text);

    if (_selected == null || amount == null) {
      return false;
    }

    final minAmount = num.tryParse(_selected?.bankMinAmount ?? '');
    if (minAmount != null && amount < minAmount) {
      return false;
    }

    final maxAmount = num.tryParse(_selected?.bankMaxAmount ?? '');
    if (maxAmount != null && amount > maxAmount) {
      return false;
    }

    if (_cardholderController.text.isEmpty) {
      return false;
    }

    return true;
  }

  num get _convertedAmount {
    final amount = num.tryParse(_amountController.text);
    final rate = num.tryParse(_selected?.depositRate ?? '1');
    if (amount == null || rate == null) {
      return 0;
    }
    return amount * rate;
  }

  Future<void> _submit() async {
    final bankId = _selected?.bankId;
    if (bankId == null) {
      return;
    }
    setState(() {
      _isSubmitting = true;
    });
    try {
      final data = await context.read<WalletProvider>().topUpSubmit(
            bankId: bankId,
            amount: _amountController.text,
            cardholder: _cardholderController.text,
          );
      final orderId = data?['order_id'];
      final bankAccountId = data?['bank_account_id'];
      if (!mounted) return;
      final uri = Uri(
        path: '/top-up/receipt',
        queryParameters: {
          'bankId': bankId.toString(),
          if (orderId != null) 'orderId': orderId.toString(),
          if (bankAccountId != null) 'bankAccountId': bankAccountId.toString(),
        },
      );

      context.replace(uri.toString());
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  Widget _buildAmountTextField({
    required TextEditingController controller,
    required String text,
    required bool readOnly,
    String? image,
  }) {
    return Column(
      children: [
        Row(
          children: [
            // Image
            if (image != null) Image.network(image, width: 24, height: 24),
            if (image == null)
              Container(
                padding: const EdgeInsets.all(2),
                height: 24,
                width: 24,
                decoration: BoxDecoration(
                  color: const Color(0xFF3285e6),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Center(
                  child: FittedBox(
                    fit: BoxFit.fitWidth,
                    child: Text(
                      text,
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
              ),
            const SizedBox(width: 8),
            Text(text),
          ],
        ),
        const SizedBox(height: 8),
        BaseTextField(
          controller: controller,
          readOnly: readOnly,
        ),
      ],
    );
  }

  @override
  void initState() {
    if (widget.list.isNotEmpty) {
      _selected = widget.list.first;
    }
    _amountController.addListener(() {
      setState(() {
        _convertedAmountController.text = formatNumber(_convertedAmount);
      });
    });
    _cardholderController.addListener(() {
      setState(() {});
    });
    super.initState();
  }

  String? get _currencyName {
    if (_selected?.currencyId == "12") {
      return 'RM';
    }
    return _selected?.currencyName;
  }

  @override
  void dispose() {
    _amountController.dispose();
    _convertedAmountController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final currencyName = _currencyName ?? '-';
    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Stack(
            children: [
              Row(
                children: [
                  Expanded(
                    child: _buildAmountTextField(
                      controller: _amountController,
                      /*
                      image:
                          "https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/currency/${currencyName.toLowerCase()}.png",
                          */
                      text: currencyName,
                      readOnly: false,
                    ),
                  ),
                  const SizedBox(width: 17),
                  /*
                  Expanded(
                    child: _buildAmountTextField(
                      controller: _convertedAmountController,
                      image:
                          "https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/currency/usdt.png",
                      text: 'USDT',
                      readOnly: true,
                    ),
                  ),
                  */
                  Expanded(
                    child: _buildAmountTextField(
                      controller: _convertedAmountController,
                      image:
                          "https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/currency/usd.png",
                      text: 'USD',
                      readOnly: true,
                    ),
                  ),
                ],
              ),
              Positioned.fill(
                top: 30,
                child: Align(
                  alignment: Alignment.center,
                  child: Container(
                    width: 32,
                    height: 32,
                    decoration: BoxDecoration(
                      color: const Color(0xFF131213),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: const Icon(
                      Icons.arrow_forward_ios,
                      size: 16,
                      color: Colors.white,
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 4),
          Text(
            'Amount: $currencyName${formatNumber(_selected?.bankMinAmount ?? 0)} - $currencyName${formatNumber(_selected?.bankMaxAmount ?? 0)}',
            style: TextStyle(
              color: AppColors.primaryColor,
            ),
          ),
          Text('Rate ≈ *${formatNumber(_selected?.depositRate ?? 1)}'),
          const SizedBox(height: 20),
          GridView.builder(
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 4,
              crossAxisSpacing: 10,
              mainAxisSpacing: 10,
              childAspectRatio: 2,
            ),
            itemCount: amountList.length,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            itemBuilder: (context, index) {
              final amount = amountList[index];
              return BaseCard(
                onTap: () {
                  _amountController.text = amount.toString();
                },
                child: Center(
                  child: Text('$amount'),
                ),
              );
            },
          ),
          const SizedBox(height: 20),
          const Text(
            'Transfer information',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 12),
          BaseDropdown(
            labelText: 'Type',
            items: widget.list
                .map(
                  (e) => DropdownMenuItem(
                    value: e,
                    child: Text(e.bankName ?? ''),
                  ),
                )
                .toList(),
            value: _selected,
            onChanged: (p0) {
              setState(() {
                _selected = p0;
                _convertedAmountController.text =
                    formatNumber(_convertedAmount);
              });
            },
          ),
          const SizedBox(height: 20),
          BaseTextField(
            controller: _cardholderController,
            labelText: 'Cardholder',
            hintText: 'Please enter the cardholder',
          ),
          const SizedBox(height: 20),
          BaseButton(
            loading: _isSubmitting,
            text: 'Submit',
            onPressed: _canSubmit ? _submit : null,
          ),
        ],
      ),
    );
  }
}
