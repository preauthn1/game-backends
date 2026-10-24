import 'package:collection/collection.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../models/withdrawal_bank.dart';
import '../providers/member_provider.dart';
import '../providers/wallet_provider.dart';
import '../widgets/base_button.dart';
import '../widgets/base_tab_bar.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/withdrawal_bank_section.dart';
import '../widgets/withdrawal_crypto_section.dart';

class WithdrawalScreen extends StatefulWidget {
  const WithdrawalScreen({super.key});

  @override
  State<WithdrawalScreen> createState() => _WithdrawalScreenState();
}

class _WithdrawalScreenState extends State<WithdrawalScreen> {
  late Future<void> _future;

  Future<void> _fetchData() async {
    await Future.wait([
      context.read<WalletProvider>().fetchWithdrawalBankAccount(),
      context.read<WalletProvider>().fetchWithdrawalBank(),
      context.read<WalletProvider>().fetchBalance(),
    ]);
  }

  @override
  void initState() {
    context.read<WalletProvider>().setWithdrawalCreate(null);
    _future = _fetchData();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: FutureBuilder(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Withdrawal'),
              ),
              body: const Center(
                child: CircularProgressIndicator(),
              ),
            );
          }

          if (snapshot.hasError) {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Withdrawal'),
              ),
              body: const Center(
                child: Text('Error'),
              ),
            );
          }

          final member = context.watch<MemberProvider>().member;
          if (member?.fundPasswordStatus != "1") {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Withdrawal'),
              ),
              body: Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Text('Please set up your fund password'),
                    const SizedBox(height: 20),
                    BaseButton.small(
                      onPressed: () {
                        context.push('/profile/create-fund-password');
                      },
                      text: 'Set Up Now',
                    ),
                  ],
                ),
              ),
            );
          }

          if (member?.is2faEnable != "1") {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Withdrawal'),
              ),
              body: Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Text('Please enable 2FA first.'),
                    const SizedBox(height: 20),
                    BaseButton.small(
                      onPressed: () {
                        context.push('/profile/google-authentication');
                      },
                      text: 'Enable Now',
                    ),
                  ],
                ),
              ),
            );
          }

          final wallet = context.watch<WalletProvider>();
          final balance = wallet.mainWallet?.balance ?? '0';
          WithdrawalBank? bank;
          WithdrawalBank? crypto;
          if (wallet.withdrawalAccount?.bank != null) {
            bank = wallet.withdrawalBankList.firstWhereOrNull((element) =>
                element.bankId == wallet.withdrawalAccount?.bank?.bankId);
          }
          if (wallet.withdrawalAccount?.crypto != null) {
            crypto = wallet.withdrawalCryptoList.firstWhereOrNull(
              (element) =>
                  element.bankId == wallet.withdrawalAccount?.crypto?.bankId,
            );
          }

          return DefaultTabController(
            length: 2,
            child: Scaffold(
              appBar: AppBar(
                title: const Text('Withdrawal'),
                bottom: const BaseTabBar(
                  tabs: [
                    Tab(text: 'Bank'),
                    Tab(text: 'USDT'),
                  ],
                ),
              ),
              body: TabBarView(
                children: [
                  WithdrawalBankSection(bank: bank, balance: balance),
                  WithdrawalCryptoSection(crypto: crypto, balance: balance),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
