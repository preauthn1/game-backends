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

class TopUpCryptoSection extends StatefulWidget {
  final List<TopUpBank> list;
  const TopUpCryptoSection({required this.list, super.key});

  @override
  State<TopUpCryptoSection> createState() => _TopUpCryptoSectionState();
}

class _TopUpCryptoSectionState extends State<TopUpCryptoSection> {
  final TextEditingController _amountController = TextEditingController();
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

    return true;
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
    required String image,
    required String text,
  }) {
    return Column(
      children: [
        Row(
          children: [
            // Image
            Image.network(image, width: 24, height: 24),
            const SizedBox(width: 8),
            Text(text),
          ],
        ),
        const SizedBox(height: 8),
        BaseTextField(
          controller: controller,
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
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _amountController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final currency = _selected?.currencyName ?? '';
    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildAmountTextField(
            controller: _amountController,
            image:
                'https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/currency/${currency.toLowerCase()}.png',
            text: _selected?.currencyName ?? '',
          ),
          const SizedBox(height: 4),
          Text(
            'Amount: $currency${formatNumber(_selected?.bankMinAmount)} - $currency${formatNumber(_selected?.bankMaxAmount)}',
            style: TextStyle(
              color: AppColors.primaryColor,
            ),
          ),
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
              });
            },
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
