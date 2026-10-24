import 'package:flutter/material.dart';

import '../models/withdrawal_bank.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import 'base_button.dart';

class WithdrawalFooter extends StatelessWidget {
  final String totalAmount;
  final WithdrawalBank bank;
  final Function()? onPressed;

  const WithdrawalFooter({
    super.key,
    required this.totalAmount,
    required this.bank,
    required this.onPressed,
  });

  String get feeText {
    if (bank.bankTax == null) {
      return '-';
    }
    final tax = num.tryParse(bank.bankTax!);
    if (tax == null) {
      return '-';
    }
    if (bank.bankTaxType == '0') {
      final amount = num.tryParse(totalAmount);
      final result = amount != null ? tax * amount : 0;
      return 'Withdrawal Fee ${formatNumber(tax * 100)}%: ${formatNumber(result)} USDT';
    }
    return 'Withdrawal Fee: ${formatNumber(tax)} USDT';
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
      decoration: const BoxDecoration(
        color: Color(0xFF0d0d0e),
        border: Border(
          top: BorderSide(
            color: Color(0xff252526),
          ),
        ),
      ),
      child: SafeArea(
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                const Text(
                  'Total amount',
                  style: TextStyle(
                    color: Color(0xFF9e9e9f),
                    fontSize: 12,
                  ),
                ),
                Text(
                  '${formatNumber(totalAmount)} USDT',
                  style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.w600,
                    color: AppColors.primaryColor,
                  ),
                ),
                Text(
                  feeText,
                  style: const TextStyle(fontSize: 12),
                ),
              ],
            ),
            const Spacer(),
            BaseButton.small(
              padding: const EdgeInsets.symmetric(
                horizontal: 28,
                vertical: 14,
              ),
              onPressed: onPressed,
              text: 'NEXT',
            ),
          ],
        ),
      ),
    );
  }
}
