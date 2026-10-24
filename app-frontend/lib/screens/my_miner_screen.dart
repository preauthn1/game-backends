import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../models/my_miner.dart';
import '../providers/miner_provider.dart';
import '../themes/app_colors.dart';
import '../widgets/base_card.dart';
import '../widgets/base_tab_bar.dart';

class MyMinerScreen extends StatefulWidget {
  const MyMinerScreen({super.key});

  @override
  State<MyMinerScreen> createState() => _MyMinerScreenState();
}

class _MyMinerScreenState extends State<MyMinerScreen> {
  Widget _buildItem(MyMiner miner) {
    return Stack(
      children: [
        BaseCard(
          onTap: miner.mmId == null
              ? null
              : () async {
                  await context.push('/my-miner/purchase/${miner.mmId}/detail');
                },
          padding: const EdgeInsets.all(12),
          child: IntrinsicHeight(
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                BaseCard(
                  child: Image.network(
                    miner.image ?? '',
                    height: 80,
                  ),
                ),
                const SizedBox(width: 12),
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.start,
                  children: [
                    Text(miner.name ?? ''),
                    const SizedBox(height: 8),
                    const Text(
                      'Expire date:',
                      style: TextStyle(
                        color: Colors.grey,
                        fontSize: 12,
                      ),
                    ),
                    Text(
                      miner.endTime ?? '-',
                      style: const TextStyle(fontSize: 12),
                    ),
                  ],
                ),
                const Spacer(),
                const Icon(Icons.chevron_right),
              ],
            ),
          ),
        ),
        if (miner.type == "2")
          Positioned(
            top: 12,
            right: 12,
            child: Text(
              'Group Order',
              style: TextStyle(color: AppColors.accentColor),
            ),
          ),
      ],
    );
  }

  Widget _buildPurchase() {
    const space = 10.0;
    final list = context.watch<MinerProvider>().myPurchaseMiners;
    if (list.isEmpty) {
      return const Center(
        child: Text('No data'),
      );
    }
    return SingleChildScrollView(
      child: Padding(
        padding: const EdgeInsets.all(space),
        child: Wrap(
          spacing: space,
          runSpacing: space,
          children: list.map((miner) => _buildItem(miner)).toList(),
        ),
      ),
    );
  }

  Widget _buildRent() {
    const space = 10.0;
    final list = context.watch<MinerProvider>().myMinerRentMiners;
    if (list.isEmpty) {
      return const Center(
        child: Text('No data'),
      );
    }
    return SingleChildScrollView(
      child: Padding(
        padding: const EdgeInsets.all(space),
        child: Wrap(
          spacing: space,
          runSpacing: space,
          children: list.map((miner) => _buildItem(miner)).toList(),
        ),
      ),
    );
  }

  @override
  void initState() {
    context.read<MinerProvider>().fetchMyPurchaseMiners();
    context.read<MinerProvider>().fetchMyMinerRentMiners();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Column(
        children: [
          const BaseTabBar(
            tabs: [
              Tab(text: 'Purchase'),
              Tab(text: 'Rent'),
            ],
          ),
          Expanded(
            child: TabBarView(
              children: [
                _buildPurchase(),
                _buildRent(),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
