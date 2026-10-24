import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/miner_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../widgets/base_card.dart';
import '../widgets/linear_gradient_background.dart';

class MinerRentDetailScreen extends StatelessWidget {
  final String mmId;
  const MinerRentDetailScreen({super.key, required this.mmId});

  @override
  Widget build(BuildContext context) {
    final miner = context.read<MinerProvider>().getRentMinerByMmId(mmId);
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: Text(miner?.name ?? ''),
          backgroundColor: Colors.black,
        ),
        body: miner == null
            ? const Center(child: Text('Miner not found'))
            : SingleChildScrollView(
                child: SafeArea(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Container(
                        height: 230,
                        width: double.infinity,
                        padding: const EdgeInsets.symmetric(vertical: 10),
                        color: Colors.black,
                        child: Padding(
                          padding: const EdgeInsets.only(bottom: 20),
                          child: Image.network(
                            miner.image ?? '',
                            width: double.infinity,
                          ),
                        ),
                      ),
                      Transform.translate(
                        offset: const Offset(0, -20),
                        child: Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 20),
                          child: Column(
                            children: [
                              BaseCard(
                                width: double.infinity,
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(
                                      '${formatNumber(miner.rentPrice)} USDT',
                                      style: TextStyle(
                                        fontSize: 16,
                                        fontWeight: FontWeight.w600,
                                        color: AppColors.accentColor,
                                      ),
                                    ),
                                    const SizedBox(height: 8),
                                    Wrap(
                                      spacing: 8,
                                      children: [
                                        'Clearance',
                                        'Logistics',
                                        'Electricity'
                                      ]
                                          .map(
                                            (e) => Container(
                                              padding: const EdgeInsets.all(4),
                                              decoration: BoxDecoration(
                                                border: Border.all(
                                                  color: AppColors.accentColor,
                                                ),
                                                borderRadius:
                                                    BorderRadius.circular(2),
                                              ),
                                              child: Text(
                                                e,
                                                style: TextStyle(
                                                  color: AppColors.accentColor,
                                                  fontSize: 10,
                                                ),
                                              ),
                                            ),
                                          )
                                          .toList(),
                                    ),
                                    const SizedBox(height: 8),
                                    const Text(
                                      'Included in the price',
                                      style: TextStyle(color: Colors.grey),
                                    ),
                                    const SizedBox(height: 8),
                                    Text(
                                      miner.name ?? '',
                                      style: const TextStyle(fontSize: 16),
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(height: 20),
                              BaseCard(
                                width: double.infinity,
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const Text(
                                      'Details',
                                      style: TextStyle(
                                        fontWeight: FontWeight.w600,
                                      ),
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.spaceBetween,
                                      children: [
                                        const Text('Mining'),
                                        Text(miner.miningCurrency ?? '-'),
                                      ],
                                    ),
                                    const SizedBox(height: 10),
                                    Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.spaceBetween,
                                      children: [
                                        const Text('Hash Rate'),
                                        Text('${miner.hashRate} TH/s'),
                                      ],
                                    ),
                                    const SizedBox(height: 10),
                                    Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.spaceBetween,
                                      children: [
                                        const Text('Power Consumption'),
                                        Text('${miner.powerConsumption} W'),
                                      ],
                                    ),
                                    const SizedBox(height: 10),
                                    Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.spaceBetween,
                                      children: [
                                        const Text('Estimated revenue'),
                                        Text(
                                          "${formatNumber(miner.dayRevenue)} USDT/d",
                                        ),
                                      ],
                                    ),
                                  ],
                                ),
                              ),
                            ],
                          ),
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
