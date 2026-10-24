import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/wallet_provider.dart';
import '../widgets/base_tab_bar.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/top_up_bank_section.dart';
import '../widgets/top_up_crypto_section.dart';

class TopUpScreen extends StatelessWidget {
  const TopUpScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: FutureBuilder(
        future: context.read<WalletProvider>().fetchTopUpBank(),
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Deposit'),
              ),
              body: const Center(
                child: CircularProgressIndicator(),
              ),
            );
          }

          if (snapshot.hasError) {
            return Scaffold(
              appBar: AppBar(
                title: const Text('Deposit'),
              ),
              body: const Center(
                child: Text('Error'),
              ),
            );
          }

          final walletProvider = context.read<WalletProvider>();
          final List<Map<String, Widget>> tabs = [];
          if (walletProvider.topUpBankList.isNotEmpty) {
            tabs.add({
              'tab': const Tab(text: 'Bank'),
              'content': TopUpBankSection(
                list: [...walletProvider.topUpBankList],
              ),
            });
          }
          if (walletProvider.topUpCryptoList.isNotEmpty) {
            final promo = walletProvider.topUpUSDTPromo;
            tabs.add({
              'tab': Tab(
                child: Text.rich(
                  TextSpan(
                    children: [
                      const TextSpan(text: 'Crypto'),
                      if (promo != null && promo.isNotEmpty)
                        TextSpan(
                          text: ' $promo',
                          style: const TextStyle(
                            color: Colors.red,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                    ],
                  ),
                ),
              ),
              'content': TopUpCryptoSection(
                list: [...walletProvider.topUpCryptoList],
              ),
            });
          }

          return DefaultTabController(
            length: tabs.length,
            child: Scaffold(
              appBar: AppBar(
                title: const Text('Deposit'),
                bottom: BaseTabBar(
                  tabs: tabs.map((tab) => tab['tab'] as Tab).toList(),
                ),
              ),
              body: TabBarView(
                children: tabs.map((tab) => tab['content'] as Widget).toList(),
              ),
            ),
          );
        },
      ),
    );
  }
}
