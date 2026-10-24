import 'package:flutter/material.dart';
import 'package:miner/utils/utils.dart';
import 'package:provider/provider.dart';

import '../providers/miner_provider.dart';
import '../widgets/base_card.dart';
import '../widgets/linear_gradient_background.dart';

class ReferralRewardsHelpScreen extends StatelessWidget {
  const ReferralRewardsHelpScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Referral Rewards'),
        ),
        body: FutureBuilder(
          future: context.read<MinerProvider>().fetchReferralRewardList(),
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            if (snapshot.hasError) {
              return const Center(
                child: Text('Error'),
              );
            }

            final list = context.read<MinerProvider>().referralRewards;

            return SingleChildScrollView(
              padding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 20,
              ),
              child: Column(
                children: [
                  const Text(
                    'According to the sales of different mining, the 3rd, 5th, and 10th people of your first level purchase of each mining and you will have different referral rewards.\n\nNote:\n1.The referral rewards must be the 3rd, 5th, or 10th people. Different mining cannot be accumulated.\n2.Group order does not count the number of valid invitations',
                  ),
                  const SizedBox(height: 20),
                  SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: BaseCard(
                      child: DataTable(
                        columns: const [
                          DataColumn(label: Text('Mining')),
                          DataColumn(label: Text('3 Members')),
                          DataColumn(label: Text('5 Members')),
                          DataColumn(label: Text('10 Members')),
                        ],
                        rows: [
                          ...list.map(
                            (item) => DataRow(
                              cells: [
                                DataCell(Text(item.minerName ?? '')),
                                DataCell(
                                  Text(formatNumber(
                                    item.getReward('3'),
                                  )),
                                ),
                                DataCell(
                                  Text(formatNumber(
                                    item.getReward('5'),
                                  )),
                                ),
                                DataCell(
                                  Text(formatNumber(
                                    item.getReward('10'),
                                  )),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  )
                ],
              ),
            );
          },
        ),
      ),
    );
  }
}
