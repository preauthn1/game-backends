import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:miner/utils/utils.dart';
import 'package:provider/provider.dart';

import '../models/withdrawal_bank.dart';
import '../models/withdrawal_create.dart';
import '../providers/wallet_provider.dart';
import 'base_button.dart';
import 'base_card.dart';
import '../themes/app_colors.dart';
import 'withdrawal_footer.dart';

class WithdrawalBankSection extends StatefulWidget {
  final WithdrawalBank? bank;
  final String balance;
  const WithdrawalBankSection({required this.balance, this.bank, super.key});

  @override
  State<WithdrawalBankSection> createState() => _WithdrawalBankSectionState();
}

class _WithdrawalBankSectionState extends State<WithdrawalBankSection> {
  final TextEditingController _amountController1 =
      TextEditingController(text: '0');
  final TextEditingController _amountController2 =
      TextEditingController(text: '0');

  num get rate => num.tryParse(widget.bank?.withdrawalRate ?? '1') ?? 1;
  bool get _canSubmit {
    final inputAmount = num.tryParse(_amountController1.text);
    final amount = num.tryParse(_amountController2.text);

    if (amount == null) {
      return false;
    }

    final balance = num.tryParse(widget.balance);
    if (balance != null && inputAmount != null && inputAmount > balance) {
      return false;
    }

    final minAmount = num.tryParse(widget.bank?.bankMinAmount ?? '');
    if (minAmount != null && amount < minAmount) {
      return false;
    }

    final maxAmount = num.tryParse(widget.bank?.bankMaxAmount ?? '');
    if (maxAmount != null && amount > maxAmount) {
      return false;
    }

    return true;
  }

  String? get _currencyName {
    if (widget.bank?.currencyId == "12") {
      return 'RM';
    }
    return widget.bank?.currencyName;
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
      final amount1 = double.tryParse(_amountController1.text) ?? 0;
      final amount2 = amount1 * rate;
      _amountController2.text = amount2.toStringAsFixed(2);
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _amountController1.dispose();
    _amountController2.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final wallet = context.watch<WalletProvider>();
    final account = wallet.withdrawalAccount?.bank;
    final bank = widget.bank;
    final currencyName = _currencyName ?? '-';

    if (account == null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Text('Please set up your bank card'),
            const SizedBox(height: 20),
            BaseButton.small(
              onPressed: () {
                context.push('/profile/bank-card');
              },
              text: 'Set Up Now',
            ),
          ],
        ),
      );
    }

    if (bank == null) {
      return const Center(
        child: Text('No bank data'),
      );
    }

    return Stack(
      children: [
        SingleChildScrollView(
          padding: const EdgeInsets.only(
            left: 20,
            right: 20,
            top: 20,
            bottom: 100, // footer height
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Amount: $currencyName${formatNumber(bank.bankMinAmount)} - $currencyName${formatNumber(bank.bankMaxAmount)}',
                style: TextStyle(color: AppColors.primaryColor),
              ),
              const SizedBox(height: 12),
              Stack(
                children: [
                  Column(
                    children: [
                      _buildAmountTextField(
                        title: "Exchange From",
                        image:
                            "https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/currency/usdt.png",
                        text: 'USDT',
                        subtitle: 'Balance: ${formatNumber(widget.balance)}',
                        amountController: _amountController1,
                        amountReadOnly: false,
                      ),
                      const SizedBox(height: 8),
                      _buildAmountTextField(
                        title: "To",
                        image:
                            "https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/currency/${currencyName.toLowerCase()}.png",
                        text: currencyName,
                        subtitle: 'Rate ≈ *${formatNumber(rate)}',
                        amountController: _amountController2,
                        amountReadOnly: true,
                      ),
                    ],
                  ),
                  Positioned.fill(
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
                          Icons.swap_vertical_circle_outlined,
                          size: 20,
                          color: Colors.white,
                        ),
                      ),
                    ),
                  ),
                ],
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
                          'Cardholder',
                          style: TextStyle(
                            fontSize: 16,
                            color: Color(0xffa3a2a4),
                          ),
                        ),
                        const Spacer(),
                        Text(account.accountName ?? '-'),
                      ],
                    ),
                    const SizedBox(height: 16),
                    Row(
                      children: [
                        const Text(
                          'Bank',
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
                      children: [
                        const Text(
                          'Card No',
                          style: TextStyle(
                            fontSize: 16,
                            color: Color(0xffa3a2a4),
                          ),
                        ),
                        const Spacer(),
                        Text(account.accountNo ?? '-'),
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
            bank: bank,
            onPressed: _canSubmit
                ? () {
                    final WithdrawalCreate data = WithdrawalCreate(
                      amount: _amountController1.text,
                      accountId: account.accountId ?? "",
                      bankId: account.bankId ?? "",
                      bankTax: bank.bankTax ?? "",
                      bankTaxType: bank.bankTaxType ?? "",
                      rate: rate.toString(),
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
