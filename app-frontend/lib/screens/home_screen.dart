import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../providers/promotion_provider.dart';
import '../providers/wallet_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../widgets/balance_text.dart';
import '../widgets/base_button.dart';
import '../widgets/base_card.dart';
import '../widgets/base_text_button.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  @override
  void initState() {
    context.read<WalletProvider>().fetchBalance();
    context.read<PromotionProvider>().fetchHomeInfo();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final homeInformation = context.watch<PromotionProvider>().homeInformation;
    final topSellingMiners = homeInformation?.topSellingMiners ?? [];
    final topSalesPeople = homeInformation?.topSalesPeople ?? [];
    return SingleChildScrollView(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Balance', style: TextStyle(fontSize: 12)),
            Row(
              children: [
                const BalanceText(),
                const Spacer(),
                BaseButton.small(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 12,
                    vertical: 8,
                  ),
                  onPressed: () {
                    context.push('/top-up');
                  },
                  text: "Deposit",
                ),
              ],
            ),
            const SizedBox(height: 12),
            AspectRatio(
              aspectRatio: 33.5 / 10,
              child: Container(
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(8),
                  image: DecorationImage(
                    image: Image.asset('assets/images/home_banner.png').image,
                    fit: BoxFit.cover,
                  ),
                ),
              ),
            ),
            const SizedBox(height: 20),
            if (topSellingMiners.isNotEmpty)
              Row(
                children: [
                  const Text(
                    "Top-selling Miners",
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const Spacer(),
                  BaseTextButton(
                    onPressed: () {
                      context.push('/miners');
                    },
                    child: const Row(
                      children: [
                        Text(
                          'View all',
                          style: TextStyle(
                            color: Color(0xFF6F6F6F),
                          ),
                        ),
                        SizedBox(width: 4),
                        Icon(
                          Icons.arrow_forward_ios_rounded,
                          size: 16,
                          color: Color(0xFFFFFFFF),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            if (topSellingMiners.isNotEmpty) const SizedBox(height: 10),
            if (topSellingMiners.isNotEmpty)
              for (int i = 0; i < topSellingMiners.length; i++) ...[
                BaseCard(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                  child: Row(
                    children: [
                      BaseCard(
                        height: 80,
                        width: 86,
                        child: Image.network(topSellingMiners[i].image ?? ''),
                      ),
                      const SizedBox(width: 12),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(topSellingMiners[i].name ?? '-'),
                          Text(
                            '${formatNumber(topSellingMiners[i].amount)} USDT',
                            style: TextStyle(
                              color: AppColors.primaryColor,
                            ),
                          ),
                          const SizedBox(height: 18),
                          Text(
                            topSellingMiners[i].info ?? '-',
                            style: const TextStyle(
                              fontSize: 12,
                              color: Colors.grey,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                if (i != topSellingMiners.length - 1)
                  const SizedBox(height: 10),
              ],
            if (topSellingMiners.isNotEmpty) const SizedBox(height: 22),
            const Text(
              "Top 10 Salespeople",
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(height: 16),
            BaseCard(
              padding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 20,
              ),
              width: double.infinity,
              child: Column(
                children: [
                  const Row(
                    children: [
                      Expanded(
                        child: Text(
                          'Member',
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                      Expanded(
                        child: Text(
                          'Quantity',
                          textAlign: TextAlign.center,
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                      Expanded(
                        child: Text(
                          'Rebate',
                          textAlign: TextAlign.right,
                          style: TextStyle(color: Colors.grey),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  for (var i = 0; i < topSalesPeople.length; i++) ...[
                    Row(
                      children: [
                        Expanded(
                          child: Row(
                            children: [
                              SizedBox(
                                height: 24,
                                width: 24,
                                child: CircleAvatar(
                                  backgroundImage: Image.network(
                                    topSalesPeople[i].image ?? '',
                                  ).image,
                                ),
                              ),
                              const SizedBox(width: 4),
                              Text(topSalesPeople[i].username ?? '-'),
                            ],
                          ),
                        ),
                        Expanded(
                          child: Text(
                            topSalesPeople[i].quantitySold ?? '-',
                            textAlign: TextAlign.center,
                          ),
                        ),
                        Expanded(
                          child: Text(
                            formatNumber(topSalesPeople[i].rebateAmount),
                            textAlign: TextAlign.right,
                          ),
                        ),
                      ],
                    ),
                    if (i != topSalesPeople.length - 1)
                      const SizedBox(
                        height: 16,
                      ),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
