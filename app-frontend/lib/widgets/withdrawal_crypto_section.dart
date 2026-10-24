import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../models/withdrawal_bank.dart';
import '../models/withdrawal_create.dart';
import '../providers/wallet_provider.dart';
import '../utils/utils.dart';
import 'base_button.dart';
import 'base_card.dart';
import '../themes/app_colors.dart';
import 'withdrawal_footer.dart';

class WithdrawalCryptoSection extends StatefulWidget {
  final WithdrawalBank? crypto;
  final String balance;
  const WithdrawalCryptoSection({
    required this.balance,
    this.crypto,
    super.key,
  });

  @override
  State<WithdrawalCryptoSection> createState() =>
      _WithdrawalCryptoSectionState();
}

class _WithdrawalCryptoSectionState extends State<WithdrawalCryptoSection> {
  final TextEditingController _amountController1 =
      TextEditingController(text: '0');

  bool get _canSubmit {
    final amount = num.tryParse(_amountController1.text);

    if (amount == null) {
      return false;
    }

    final balance = num.tryParse(widget.balance);
    if (balance != null && amount > balance) {
      return false;
    }

    final minAmount = num.tryParse(widget.crypto?.bankMinAmount ?? '');
    if (minAmount != null && amount < minAmount) {
      return false;
    }

    final maxAmount = num.tryParse(widget.crypto?.bankMaxAmount ?? '');
    if (maxAmount != null && amount > maxAmount) {
      return false;
    }

    return true;
  }

  Widget _buildAmountTextField({
    required String title,
    required String subtitle,
    required String image,
    required String text,
    required TextEditingController amountController,
    required bool amountReadOnly,
  }) {
    return BaseCard(
      padding: const EdgeInsets.only(left: 16, right: 16, top: 20, bottom: 32),
      child: Column(
        children: [
          Row(
            children: [
              Text(title, style: const TextStyle(color: Colors.grey)),
              const Spacer(),
              Text(subtitle, style: const TextStyle(color: Colors.grey)),
            ],
          ),
          const SizedBox(height: 10),
          Row(
            children: [
              Image.network(image, width: 24, height: 24),
              const SizedBox(width: 8),
              Text(text),
              const SizedBox(width: 8),
              Expanded(
                child: TextField(
                  controller: amountController,
                  readOnly: amountReadOnly,
                  keyboardType: const TextInputType.numberWithOptions(
                    decimal: true,
                  ),
                  inputFormatters: [
                    FilteringTextInputFormatter.allow(
                      RegExp(r'^\d*\.?\d{0,2}'),
                    ),
                  ],
                  decoration: const InputDecoration(
                    border: InputBorder.none,
                    isDense: true,
                    contentPadding: EdgeInsets.all(0),
                  ),
                  style: const TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.w600,
                  ),
                  textAlign: TextAlign.end,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  @override
  void initState() {
    _amountController1.addListener(() {
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _amountController1.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final wallet = context.watch<WalletProvider>();
    final account = wallet.withdrawalAccount?.crypto;
    final crypto = widget.crypto;

    if (account == null) {
      return Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Center(
            child: Text('Please add your crypto address'),
          ),
          const SizedBox(height: 20),
          BaseButton.small(
            onPressed: () {
              context.push('/profile/crypto-address');
            },
            text: 'Set Up Now',
          ),
        ],
      );
    }

    if (crypto == null) {
      return const Center(
        child: Text('No bank data'),
      );
    }

    return Stack(
      children: [
        SingleChildScrollView(
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Amount: USDT${formatNumber(crypto.bankMinAmount)} - USDT${formatNumber(crypto.bankMaxAmount)}',
                style: TextStyle(
                  color: AppColors.primaryColor,
                ),
              ),
              const SizedBox(height: 12),
              _buildAmountTextField(
                title: "",
                image:
                    "https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/currency/usdt.png",
                text: 'USDT',
                subtitle: 'Balance: ${formatNumber(widget.balance)}',
                amountController: _amountController1,
                amountReadOnly: false,
              ),
              const SizedBox(height: 20),
              const Text(
                'Withdrawal Information',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 12),
              BaseCard(
                padding: const EdgeInsets.all(20),
                width: double.infinity,
                child: Column(
                  children: [
                    Row(
                      children: [
                        const Text(
                          'Type',
                          style: TextStyle(
                            fontSize: 16,
                            color: Color(0xffa3a2a4),
                          ),
                        ),
                        const Spacer(),
                        Text(account.bankName ?? '-'),
                      ],
                    ),
                    const SizedBox(height: 16),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text(
                          'Address',
                          style: TextStyle(
                            fontSize: 16,
                            color: Color(0xffa3a2a4),
                          ),
                        ),
                        Expanded(
                          child: Text(
                            account.cryptoAddress ?? '-',
                            textAlign: TextAlign.end,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
        Positioned(
          bottom: 0,
          left: 0,
          right: 0,
          child: WithdrawalFooter(
            totalAmount: _amountController1.text,
            bank: crypto,
            onPressed: _canSubmit
                ? () {
                    final WithdrawalCreate data = WithdrawalCreate(
                      amount: _amountController1.text,
                      accountId: account.accountId ?? "",
                      bankId: account.bankId ?? "",
                      bankTax: crypto.bankTax ?? "",
                      bankTaxType: crypto.bankTaxType ?? "",
                      rate: widget.crypto?.bankRate ?? "",
                      fundPassword: "",
                      gauthCode: "",
                    );
                    context.read<WalletProvider>().setWithdrawalCreate(data);
                    context.push('/withdrawal/verify');
                  }
                : null,
          ),
        ),
      ],
    );
  }
}
